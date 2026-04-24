package io.redis.devrel.workshop.services;

import dev.langchain4j.service.SystemMessage;
import dev.langchain4j.service.UserMessage;
import dev.langchain4j.service.V;
import dev.langchain4j.service.spring.AiService;
@AiService
public interface BasicChatAssistant {

    @SystemMessage("""
            {{systemPrompt}}
            """)
    String chat(@V("systemPrompt") String systemPrompt, @UserMessage String query);

}
