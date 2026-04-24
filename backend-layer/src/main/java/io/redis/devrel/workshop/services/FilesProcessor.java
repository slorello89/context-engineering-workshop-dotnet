package io.redis.devrel.workshop.services;

import dev.langchain4j.data.document.Document;
import dev.langchain4j.data.document.DocumentParser;
import dev.langchain4j.data.document.DocumentSplitter;
import dev.langchain4j.data.document.parser.apache.pdfbox.ApachePdfBoxDocumentParser;
import dev.langchain4j.data.document.splitter.DocumentByParagraphSplitter;
import dev.langchain4j.data.segment.TextSegment;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Service;

import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.util.List;

@Service
public class FilesProcessor {

    private final Logger logger = LoggerFactory.getLogger(FilesProcessor.class);

    @Autowired
    private MemoryService memoryService;

    @Value("${knowledge.base.input.files}")
    private String knowledgeBaseInputFiles;

    @Scheduled(fixedRate = 5000)
    public void scanForPdfFiles() {
        File dir = new File(knowledgeBaseInputFiles);
        if (dir.exists() && dir.isDirectory()) {
            File[] pdfFiles = dir.listFiles((d, name) -> name.toLowerCase().endsWith(".pdf"));
            if (pdfFiles != null) {
                for (File pdf : pdfFiles) {
                    processFile(pdf);
                }
            }
        }
    }

    private void processFile(File file) {
        final DocumentParser documentParser = new ApachePdfBoxDocumentParser(true);
        final DocumentSplitter documentSplitter = new DocumentByParagraphSplitter(1000, 100);

        logger.info("Processing file {}", file.getAbsolutePath());
        try (InputStream inputStream = new FileInputStream(file)) {
            Document document = documentParser.parse(inputStream);
            if (document == null || document.text() == null || document.text().isBlank()) {
                logger.warn("Empty or unparseable document: {}", file.getName());
                renameToProcessed(file);
                return;
            }

            List<TextSegment> segments = documentSplitter.split(document);
            if (segments.isEmpty()) {
                logger.warn("No segments created from document: {}", file.getName());
                renameToProcessed(file);
                return;
            }

            int chunksStored = 0;
            for (int i = 0; i < segments.size(); i++) {
                TextSegment segment = segments.get(i);
                if (segment.text().trim().length() < 50) {
                    continue;
                }
                String entryText = formatSegmentWithMetadata(
                        segment.text(),
                        file.getName(),
                        i + 1,
                        segments.size()
                );
                try {
                    memoryService.createKnowledgeBaseEntry(entryText);
                    chunksStored++;
                } catch (Exception e) {
                    logger.warn("Failed to store segment {} of {} from {}", i + 1, segments.size(), file.getName(), e);
                }
            }
            logger.info("Processed {} - {} segments stored out of {} total", file.getName(), chunksStored, segments.size());
        } catch (Exception e) {
            logger.error("Failed to process {}", file.getName(), e);
        } finally {
            renameToProcessed(file);
        }
    }

    private void renameToProcessed(File file) {
        String filePath = file.getAbsolutePath();
        if (filePath.toLowerCase().endsWith(".pdf")) {
            File processedFile = new File(filePath.substring(0, filePath.length() - 4) + ".processed");
            boolean renamed = file.renameTo(processedFile);
            if (!renamed) {
                logger.warn("Could not rename file {} to .processed", file.getName());
            } else {
                logger.info("Renamed file {} to {}", file.getName(), processedFile.getName());
            }
        }
    }

    private String formatSegmentWithMetadata(String segmentText, String fileName, int segmentNumber, int totalSegments) {
        String metadata = String.format("[Document: %s | Section %d of %d]\n", fileName, segmentNumber, totalSegments);
        return metadata + segmentText.trim();
    }
}
