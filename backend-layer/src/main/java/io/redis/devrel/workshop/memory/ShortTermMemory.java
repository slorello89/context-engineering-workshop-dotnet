package io.redis.devrel.workshop.memory;

import dev.langchain4j.memory.ChatMemory;
import dev.langchain4j.memory.chat.TokenWindowChatMemory;
import dev.langchain4j.model.openai.OpenAiTokenCountEstimator;
import dev.langchain4j.store.memory.chat.ChatMemoryStore;
import io.redis.devrel.workshop.extensions.WorkingMemoryStore;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class ShortTermMemory {

    @Value("${agent.memory.server.url}")
    private String agentMemoryServerUrl;

    @Value("${langchain4j.open-ai.chat-model.model-name}")
    private String modelName;

    @Value("${chat.memory.max.tokens}")
    private int maxTokens;

    @Autowired
    private String userId;

    @Bean
    public ChatMemoryStore chatMemoryStore() {
        return WorkingMemoryStore.builder()
                .agentMemoryServerUrl(agentMemoryServerUrl)
                .storeAiMessages(true)
                .build();
    }

    @Bean
    public ChatMemory chatMemory(ChatMemoryStore chatMemoryStore) {
        return TokenWindowChatMemory.builder()
                .id(userId)
                .chatMemoryStore(chatMemoryStore)
                .maxTokens(maxTokens, new OpenAiTokenCountEstimator(modelName))
                .build();
    }

}
