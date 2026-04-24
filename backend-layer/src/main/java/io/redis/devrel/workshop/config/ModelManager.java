package io.redis.devrel.workshop.config;

import jakarta.annotation.PostConstruct;
import org.springframework.context.annotation.Configuration;

import java.io.IOException;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.Path;

@Configuration
public class ModelManager {

    private String modelPath;
    private String tokenizerPath;

    @PostConstruct
    public void extractModels() throws IOException {
        Path tempDir = Files.createTempDirectory("onnx-models");

        Path modelFile = tempDir.resolve("model.onnx");
        try (InputStream is = getClass().getClassLoader()
                .getResourceAsStream("ms-marco-MiniLM-L-6/model.onnx")) {
            Files.copy(is, modelFile);
        }

        Path tokenizerFile = tempDir.resolve("tokenizer.json");
        try (InputStream is = getClass().getClassLoader()
                .getResourceAsStream("ms-marco-MiniLM-L-6/tokenizer.json")) {
            Files.copy(is, tokenizerFile);
        }

        this.modelPath = modelFile.toAbsolutePath().toString();
        this.tokenizerPath = tokenizerFile.toAbsolutePath().toString();
    }

    public String getModelPath() {
        return modelPath;
    }

    public String getTokenizerPath() {
        return tokenizerPath;
    }
}
