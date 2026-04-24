package io.redis.devrel.workshop.services;

import com.fasterxml.jackson.databind.ObjectMapper;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.List;
import java.util.Map;
import java.util.Optional;

@Service
public class LangCacheService {

    private static final Logger logger = LoggerFactory.getLogger(LangCacheService.class);

    @Value("${redis.langcache.service.baseurl}")
    private String baseUrl;

    @Value("${redis.langcache.service.apikey}")
    private String apiKey;

    @Value("${redis.langcache.service.cacheid}")
    private String cacheId;

    @Autowired
    private ObjectMapper objectMapper;

    @Autowired
    private HttpClient httpClient;

    public void addNewResponse(String prompt, String response) {
        try {
            String requestBody = objectMapper.writeValueAsString(Map.of(
                    "prompt", prompt,
                    "response", response,
                    "ttlMillis", TIME_TO_LIVE_IN_SECONDS * 1000
            ));

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(URI.create(String.format("%s/v1/caches/%s/entries", baseUrl, cacheId)))
                    .header("Authorization", "Bearer " + apiKey)
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString(requestBody))
                    .build();

            HttpResponse<String> responseHttp = httpClient.send(request, HttpResponse.BodyHandlers.ofString());
            objectMapper.readValue(responseHttp.body(), Map.class);
        } catch (Exception ex) {
            logger.error("Failed to add new entry", ex);
        }
    }

    public Optional<String> searchForResponse(String prompt) {
        logger.debug("Searching for response for prompt {}", prompt);
        try {
            String requestBody = objectMapper.writeValueAsString(Map.of(
                    "prompt", prompt,
                    "similarityThreshold", SIMILARITY_THRESHOLD,
                    "searchStrategies", List.of("semantic")
            ));

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(URI.create(String.format("%s/v1/caches/%s/entries/search", baseUrl, cacheId)))
                    .header("Authorization", "Bearer " + apiKey)
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString(requestBody))
                    .build();

            HttpResponse<String> response = httpClient.send(request, HttpResponse.BodyHandlers.ofString());

            Map<String, Object> responseMap = objectMapper.readValue(response.body(), Map.class);
            Object dataObj = responseMap.get("data");
            logger.debug("Data found on LangCache: {}", dataObj);
            if (dataObj == null) return Optional.empty();

            List<LangCacheEntry> data = objectMapper.convertValue(
                    dataObj,
                    objectMapper.getTypeFactory().constructCollectionType(List.class, LangCacheEntry.class)
            );

            return data.stream()
                    .map(langCacheEntry -> langCacheEntry.response)
                    .findFirst();
        } catch (Exception ex) {
            logger.error("Failed to search for entries", ex);
        }
        return Optional.empty();
    }

    private record LangCacheEntry(
            String id,
            String prompt,
            String response,
            String searchStrategy,
            Double similarity,
            Map<String, Object> attributes
    ) {}

    private static final int TIME_TO_LIVE_IN_SECONDS = 60;
    private static final float SIMILARITY_THRESHOLD = 0.7f;
}
