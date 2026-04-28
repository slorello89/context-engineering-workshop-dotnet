using System.Text;
using Microsoft.Extensions.Options;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using BackendDotnetLayer.Configuration;

namespace BackendDotnetLayer.Services;

public sealed class FilesProcessor : BackgroundService
{
    private readonly FilesProcessorOptions _options;
    private readonly ILogger<FilesProcessor> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public FilesProcessor(
        IOptions<FilesProcessorOptions> options,
        ILogger<FilesProcessor> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _options = options.Value;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, _options.ScanIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanForPdfFilesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled error while scanning for PDF files");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task ScanForPdfFilesAsync(CancellationToken cancellationToken)
    {
        var inputDirectory = ResolveInputDirectory();
        if (string.IsNullOrWhiteSpace(inputDirectory) || !Directory.Exists(inputDirectory))
        {
            return;
        }

        foreach (var pdfPath in Directory.EnumerateFiles(inputDirectory, "*.pdf", SearchOption.TopDirectoryOnly))
        {
            await ProcessFileAsync(pdfPath, cancellationToken);
        }
    }

    private async Task ProcessFileAsync(string filePath, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing file {FilePath}", filePath);

        try
        {
            var documentText = ExtractText(filePath);
            if (string.IsNullOrWhiteSpace(documentText))
            {
                _logger.LogWarning("Empty or unparseable document: {FileName}", Path.GetFileName(filePath));
                return;
            }

            var segments = SplitIntoSegments(documentText);
            if (segments.Count == 0)
            {
                _logger.LogWarning("No segments created from document: {FileName}", Path.GetFileName(filePath));
                return;
            }

            var chunksStored = 0;
            using var scope = _serviceScopeFactory.CreateScope();
            var memoryService = scope.ServiceProvider.GetRequiredService<MemoryService>();

            for (var index = 0; index < segments.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var segment = segments[index].Trim();
                if (segment.Length < _options.MinimumChunkLength)
                {
                    continue;
                }

                var entryText = FormatSegmentWithMetadata(segment, Path.GetFileName(filePath), index + 1, segments.Count);
                try
                {
                    await memoryService.CreateKnowledgeBaseEntryAsync(entryText, cancellationToken);
                    chunksStored++;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Failed to store segment {SegmentNumber} of {TotalSegments} from {FileName}", index + 1, segments.Count, Path.GetFileName(filePath));
                }
            }

            _logger.LogInformation("Processed {FileName} - {ChunksStored} segments stored out of {TotalSegments} total", Path.GetFileName(filePath), chunksStored, segments.Count);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to process {FileName}", Path.GetFileName(filePath));
        }
        finally
        {
            RenameToProcessed(filePath);
        }
    }

    private string ExtractText(string filePath)
    {
        using var document = PdfDocument.Open(filePath);
        var builder = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            var pageText = ContentOrderTextExtractor.GetText(page);
            if (string.IsNullOrWhiteSpace(pageText))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
            }

            builder.Append(pageText.Trim());
        }

        return builder.ToString();
    }

    private List<string> SplitIntoSegments(string documentText)
    {
        var paragraphs = documentText
            .Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(paragraph => !string.IsNullOrWhiteSpace(paragraph))
            .ToList();

        if (paragraphs.Count == 0)
        {
            return [];
        }

        var segments = new List<string>();
        var current = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            var normalizedParagraph = paragraph.Replace("\r", " ").Replace("\n", " ").Trim();
            if (string.IsNullOrWhiteSpace(normalizedParagraph))
            {
                continue;
            }

            if (normalizedParagraph.Length > _options.MaxChunkLength)
            {
                FlushCurrentSegment(segments, current);
                segments.AddRange(SplitLongParagraph(normalizedParagraph));
                continue;
            }

            var separatorLength = current.Length == 0 ? 0 : 2;
            if (current.Length + separatorLength + normalizedParagraph.Length > _options.MaxChunkLength)
            {
                FlushCurrentSegment(segments, current);
            }

            if (current.Length > 0)
            {
                current.AppendLine();
                current.AppendLine();
            }

            current.Append(normalizedParagraph);
        }

        FlushCurrentSegment(segments, current);
        return segments;
    }

    private List<string> SplitLongParagraph(string paragraph)
    {
        var segments = new List<string>();
        var start = 0;
        var overlap = Math.Max(0, Math.Min(_options.ChunkOverlapLength, _options.MaxChunkLength / 2));

        while (start < paragraph.Length)
        {
            var length = Math.Min(_options.MaxChunkLength, paragraph.Length - start);
            var slice = paragraph.Substring(start, length).Trim();
            if (!string.IsNullOrWhiteSpace(slice))
            {
                segments.Add(slice);
            }

            if (start + length >= paragraph.Length)
            {
                break;
            }

            start += Math.Max(1, length - overlap);
        }

        return segments;
    }

    private static void FlushCurrentSegment(ICollection<string> segments, StringBuilder current)
    {
        if (current.Length == 0)
        {
            return;
        }

        segments.Add(current.ToString().Trim());
        current.Clear();
    }

    private static string FormatSegmentWithMetadata(string segmentText, string fileName, int segmentNumber, int totalSegments)
    {
        return $"[Document: {fileName} | Section {segmentNumber} of {totalSegments}]{Environment.NewLine}{segmentText.Trim()}";
    }

    private void RenameToProcessed(string filePath)
    {
        if (!filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var processedPath = Path.ChangeExtension(filePath, ".processed");
        try
        {
            if (File.Exists(processedPath))
            {
                File.Delete(processedPath);
            }

            File.Move(filePath, processedPath);
            _logger.LogInformation("Renamed file {OriginalName} to {ProcessedName}", Path.GetFileName(filePath), Path.GetFileName(processedPath));
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not rename file {FileName} to .processed", Path.GetFileName(filePath));
        }
    }

    private string ResolveInputDirectory()
    {
        return Environment.GetEnvironmentVariable("KNOWLEDGE_BASE_INPUT_FILES")
            ?? _options.InputDirectory;
    }
}
