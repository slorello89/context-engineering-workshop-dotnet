# Lab 3: Knowledge Base with Embeddings, Parsers, and Splitters

## 🎯 Learning Objectives

By the end of this lab, you will:
- Configure PDF parsing for the dotnet workshop
- Implement document splitting for knowledge-base chunks
- Store processed document segments in the Agent Memory Server
- Understand how document ingestion prepares data for later RAG flows
- Verify that PDF files are processed and renamed after ingestion

#### 🕗 Estimated Time: 10 minutes

## 🏗️ What You're Building

In this lab, you'll add document processing capabilities to your AI application so it can ingest PDF files and create a searchable knowledge base. This includes:

- **Document Parser**: PDF text extraction through `PdfPig`
- **Chunking Strategy**: Paragraph-based splitting with overlap
- **Knowledge Storage**: Persisting chunks to the knowledge-base namespace
- **Background Scanner**: Polling an input directory for new PDF files

## 📋 Prerequisites Check

Before starting, ensure you have:

- [ ] Completed Lab 2 successfully
- [ ] Redis Agent Memory Server running
- [ ] Backend application configured with short-term memory support
- [ ] Sample PDF documents ready for testing

## 🚀 Setup Instructions

> 💡 This lab uses a filesystem input directory for PDF ingestion. Choose a location you can write to easily from your environment.

### Step 1: Configure the Knowledge Base Input Directory

Add this to your `.env` file:

```bash
KNOWLEDGE_BASE_INPUT_FILES=/tmp
```

You can replace `/tmp` with any folder you control.

### Step 2: Add a Sample PDF Document

Place at least one PDF file in the `KNOWLEDGE_BASE_INPUT_FILES` directory.

### Step 3: Review the Files Processor

Open `backend-dotnet-layer/Services/FilesProcessor.cs` and review the document-processing flow.

This service is responsible for:

- scanning the configured directory for `.pdf` files
- extracting text from each file
- splitting the extracted text into chunks
- storing the chunks in the knowledge-base namespace

### Step 4: Review the Memory Service

Open `backend-dotnet-layer/Services/MemoryService.cs` and review the `CreateKnowledgeBaseEntryAsync(...)` method.

This method stores semantic knowledge-base entries in the Agent Memory Server.

### Step 5: Enable the Background File Processor

Open `backend-dotnet-layer/Program.cs`.

The application already loads `FilesProcessorOptions` and `MemoryOptions`, but the hosted file processor is not registered yet on this starter branch.

Add the hosted service registration so the scanner starts with the application.

### Step 6: Enable PDF Processing in `FilesProcessor`

Open `backend-dotnet-layer/Services/FilesProcessor.cs`.

The scanner currently finds PDF files but does not process them yet:

```csharp
// TODO: Enable document processing for each discovered PDF file.
// await ProcessFileAsync(pdfPath, cancellationToken);
```

Uncomment that call so discovered files are processed.

### Step 7: Implement Text Extraction and Chunk Splitting

Still in `FilesProcessor.cs`, complete the two TODOs in `ProcessFileAsync(...)`.

Change from:

```csharp
// TODO: Extract document text from the PDF file.
var documentText = string.Empty;

// TODO: Split the extracted text into knowledge-base segments.
var segments = new List<string>();
```

To:

```csharp
var documentText = ExtractText(filePath);
var segments = SplitIntoSegments(documentText);
```

### Step 8: Rebuild and Run the Backend

```bash
dotnet build backend-dotnet-layer/BackendDotnetLayer.csproj
dotnet run --project backend-dotnet-layer
```

### Step 9: Monitor Document Processing

Watch the backend logs for PDF processing activity.

Once the lab is completed, you should see logs indicating that files were processed and renamed.

## 🧪 Testing Your Knowledge Base

### Document Processing Verification

1. Place a PDF file in the configured input directory
2. Wait a few seconds for the scanner to detect it
3. Check the backend logs for processing confirmation
4. Verify the file is renamed from `.pdf` to `.processed`

For example:

```bash
ls -la $KNOWLEDGE_BASE_INPUT_FILES
```

### Verify Knowledge Base Storage

Using Redis Insight at `http://localhost:5540`:

1. Connect to the Redis database
2. Look for keys or entries related to the knowledge-base namespace
3. Inspect the stored document segments

## 🎨 Understanding the Code

### 1. `FilesProcessor`
- Scans the input folder for PDF files
- Extracts text from each PDF
- Splits the text into manageable chunks
- Writes the chunks to the knowledge base

### 2. `MemoryService.createKnowledgeBaseEntryAsync(...)`
- Stores chunks in the dedicated knowledge-base namespace
- Marks them as semantic memory for later retrieval

### 3. `FilesProcessorOptions`
- Defines the input directory
- Controls scan interval and chunking behavior

## 🔍 What's Still Missing? (Context Engineering Perspective)

Your application now has knowledge-base ingestion, but still lacks:
- ❌ **No Retrieval**: The chat flow does not use knowledge-base content yet
- ❌ **No RAG Integration**: Stored document chunks are not injected into responses
- ❌ **No Query Routing**: The app cannot decide when to use knowledge-base content

**The next lab will connect this knowledge base to chat responses.**

## 🐛 Troubleshooting

### Common Issues and Solutions

<details>
<summary>PDF files are not being detected</summary>

Solution:
- Verify the `KNOWLEDGE_BASE_INPUT_FILES` path is correct
- Ensure the files use the `.pdf` extension
- Check that the backend process has permission to read the directory
</details>

<details>
<summary>Document parsing fails</summary>

Solution:
- Ensure the PDF is not corrupted or password-protected
- Try a text-based PDF rather than an image-only scanned PDF
- Review backend logs for parsing errors
</details>

<details>
<summary>Chunks are not being stored</summary>

Solution:
- Verify the Agent Memory Server is running
- Check `AGENT_MEMORY_SERVER_URL`
- Review backend logs for storage errors from `MemoryService`
</details>

## 🎉 Lab Completion

Congratulations! You've successfully:
- ✅ Added PDF ingestion scaffolding
- ✅ Configured chunking and knowledge-base storage
- ✅ Prepared the application for document-backed retrieval

## 📚 Additional Resources

- [PdfPig](https://github.com/UglyToad/PdfPig)
- [Redis Agent Memory Server](https://redis.github.io/agent-memory-server/)

## ➡️ Next Steps

You're ready for [Lab 4: Implementing Basic RAG with Knowledge Base Data](../lab-4-starter/README.md) where you'll integrate the knowledge base with chat responses.

- Switch to the `lab-4-starter` branch

```bash
git checkout lab-4-starter
```

- Then follow the README instructions
