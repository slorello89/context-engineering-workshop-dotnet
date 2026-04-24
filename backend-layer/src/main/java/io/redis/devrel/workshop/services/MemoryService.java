package io.redis.devrel.workshop.services;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.*;

@Service
public class MemoryService {

    private final Logger logger = LoggerFactory.getLogger(MemoryService.class);

    @Autowired
    private ObjectMapper objectMapper;

    @Autowired
    private HttpClient httpClient;

    @Value("${agent.memory.server.url}")
    private String agentMemoryServerUrl;

    public List<String> searchUserMemories(String userId, String memory) {
        var searchRequest = Map.of(
                "session_id", Map.of("eq", userId),
                "namespace", Map.of("any",
                        List.of(SHORT_TERM_MEMORY_NAMESPACE,
                                LONG_TERM_MEMORY_NAMESPACE)),
                "text", memory,
                "limit", 5
        );

        return extractTexts(executeSearch(searchRequest));
    }

    public boolean createUserMemory(String sessionId, String userId,
                                    String timezone, String memory) {
        var memoryData = Map.of(
                "memories", List.of(Map.of(
                        "id", sessionId,
                        "session_id", userId,
                        "namespace", LONG_TERM_MEMORY_NAMESPACE,
                        "text", memory,
                        "memory_type", MEMORY_TYPE_SEMANTIC
                ))
        );

        try {
            var request = buildJsonRequest(
                    URI.create(agentMemoryServerUrl + "/v1/long-term-memory/"),
                    memoryData,
                    "POST"
            );

            var response = httpClient.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() == HttpStatus.OK.value()) {
                var root = objectMapper.readTree(response.body());
                return "ok".equals(root.path("status").asText());
            }
        } catch (Exception ex) {
            logger.error("Error saving long-term memory", ex);
        }

        return false;
    }

    public void createKnowledgeBaseEntry(String memory) {
        var sanitizedMemory = Optional.ofNullable(memory)
                .map(m -> m.replaceAll("[\\r\\n]+", " "))
                .map(m -> m.replaceAll("[\\p{Cntrl}&&[^\\r\\n\\t]]", ""))
                .orElse("");

        var memoryData = Map.of(
                "memories", List.of(Map.of(
                        "id", "knowledge.entry.%s".formatted(UUID.randomUUID()),
                        "namespace", KNOWLEDGE_NAMESPACE,
                        "text", sanitizedMemory,
                        "memory_type", MEMORY_TYPE_SEMANTIC
                ))
        );

        try {
            var request = buildJsonRequest(
                    URI.create(agentMemoryServerUrl + "/v1/long-term-memory/"),
                    memoryData,
                    "POST"
            );

            httpClient.send(request, HttpResponse.BodyHandlers.ofString());
        } catch (Exception ex) {
            logger.error("Exception occurred while creating long-term memory", ex);
        }
    }

    public List<String> searchKnowledgeBase(String memory) {
        var searchRequest = Map.of(
                "namespace", Map.of("eq", KNOWLEDGE_NAMESPACE),
                "text", memory,
                "limit", 1
        );

        return extractTexts(executeSearch(searchRequest));
    }

    private HttpRequest buildJsonRequest(URI uri, Object body, String method) {
        try {
            var requestBuilder = HttpRequest.newBuilder()
                    .uri(uri)
                    .header("Content-Type", "application/json");

            var bodyPublisher = body != null
                    ? HttpRequest.BodyPublishers.ofString(objectMapper.writeValueAsString(body))
                    : HttpRequest.BodyPublishers.noBody();

            return switch (method) {
                case "POST" -> requestBuilder.POST(bodyPublisher).build();
                case "DELETE" -> requestBuilder.DELETE().build();
                default -> throw new IllegalArgumentException("Unsupported method: " + method);
            };
        } catch (Exception e) {
            throw new RuntimeException("Failed to build request", e);
        }
    }

    private List<JsonNode> executeSearch(Map<String, Object> searchRequest) {
        try {
            var request = buildJsonRequest(
                    URI.create(agentMemoryServerUrl + "/v1/long-term-memory/search?optimize_query=false"),
                    searchRequest,
                    "POST"
            );

            logger.debug("Executing request: " + request.toString());
            var response = httpClient.send(request, HttpResponse.BodyHandlers.ofString());
            logger.debug("Finishing search execution. Response status: " + response.statusCode());

            if (response.statusCode() == HttpStatus.OK.value()) {
                var memories = objectMapper.readTree(response.body()).path("memories");
                if (!memories.isEmpty()) {
                    var result = new ArrayList<JsonNode>();
                    memories.forEach(result::add);
                    logger.debug("Number of memories returned: {}", memories.size());
                    return result;
                }
            }
        } catch (Exception ex) {
            logger.error("Error during memory search", ex);
        }

        return List.of();
    }

    private List<String> extractTexts(List<JsonNode> nodes) {
        return nodes.stream()
                .map(node -> node.path("text").asText())
                .filter(text -> !text.isEmpty())
                .toList();
    }

    private static final String SHORT_TERM_MEMORY_NAMESPACE = "short-term-memory";
    private static final String LONG_TERM_MEMORY_NAMESPACE = "long-term-memory";
    private static final String KNOWLEDGE_NAMESPACE = "knowledge-base";
    private static final String MEMORY_TYPE_SEMANTIC = "semantic";
}
