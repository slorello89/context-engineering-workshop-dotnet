package io.redis.devrel.workshop.memory;

import dev.langchain4j.model.chat.ChatModel;
import dev.langchain4j.model.input.PromptTemplate;
import dev.langchain4j.model.scoring.ScoringModel;
import dev.langchain4j.model.scoring.onnx.OnnxScoringModel;
import dev.langchain4j.rag.DefaultRetrievalAugmentor;
import dev.langchain4j.rag.RetrievalAugmentor;
import dev.langchain4j.rag.content.Content;
import dev.langchain4j.rag.content.aggregator.ContentAggregator;
import dev.langchain4j.rag.content.aggregator.ReRankingContentAggregator;
import dev.langchain4j.rag.content.injector.ContentInjector;
import dev.langchain4j.rag.content.injector.DefaultContentInjector;
import dev.langchain4j.rag.content.retriever.ContentRetriever;
import dev.langchain4j.rag.query.router.LanguageModelQueryRouter;
import dev.langchain4j.rag.query.router.QueryRouter;
import dev.langchain4j.rag.query.transformer.CompressingQueryTransformer;
import dev.langchain4j.rag.query.transformer.QueryTransformer;
import io.redis.devrel.workshop.config.ModelManager;
import io.redis.devrel.workshop.services.MemoryService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

import java.util.Map;

@Configuration
public class LongTermMemory {

    @Autowired
    private String userId;

    @Autowired
    private MemoryService memoryService;

    @Autowired
    private ModelManager modelManager;

    @Bean
    public RetrievalAugmentor getRetrievalAugmentor(ChatModel chatModel) {
        QueryTransformer queryTransformer = new CompressingQueryTransformer(chatModel);

        ContentInjector contentInjector = DefaultContentInjector.builder()
                .promptTemplate(PromptTemplate.from("{{userMessage}}\n\n[Context]\n{{contents}}"))
                .build();

        Map<ContentRetriever, String> retrieversToDesc = Map.of(
                getLongTermMemories(userId), "User specific memories like preferences, events, and interactions",
                getGeneralKnowledgeBase(), "General knowledge base (not really user related) with facts and data"
        );
        QueryRouter queryRouter = LanguageModelQueryRouter.builder()
                .chatModel(chatModel)
                .retrieverToDescription(retrieversToDesc)
                .fallbackStrategy(LanguageModelQueryRouter.FallbackStrategy.ROUTE_TO_ALL)
                .build();

        ScoringModel scoringModel = new OnnxScoringModel(
                modelManager.getModelPath(),
                modelManager.getTokenizerPath()
        );
        ContentAggregator contentAggregator = ReRankingContentAggregator.builder()
                .scoringModel(scoringModel)
                .minScore(0.8)
                .build();

        return DefaultRetrievalAugmentor.builder()
                .queryTransformer(queryTransformer)
                .contentInjector(contentInjector)
                .queryRouter(queryRouter)
                .contentAggregator(contentAggregator)
                .build();
    }

    private ContentRetriever getLongTermMemories(String userId) {
        return query -> memoryService.searchUserMemories(userId, query.text())
                .stream()
                .map(Content::from)
                .toList();
    }

    private ContentRetriever getGeneralKnowledgeBase() {
        return query -> memoryService.searchKnowledgeBase(query.text())
                .stream()
                .map(Content::from)
                .toList();
    }
}
