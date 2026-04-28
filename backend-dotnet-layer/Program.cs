using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;
using BackendDotnetLayer.Configuration;
using BackendDotnetLayer.Services;

var builder = WebApplication.CreateBuilder(args);

const string FrontendDevCorsPolicy = "FrontendDevCorsPolicy";

var repoRootEnvPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".env"));
LoadDotEnv(repoRootEnvPath);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendDevCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(OpenAiOptions.SectionName));
builder.Services.Configure<WorkingMemoryOptions>(builder.Configuration.GetSection(WorkingMemoryOptions.SectionName));
builder.Services.Configure<MemoryOptions>(builder.Configuration.GetSection(MemoryOptions.SectionName));
builder.Services.Configure<FilesProcessorOptions>(builder.Configuration.GetSection(FilesProcessorOptions.SectionName));
builder.Services.Configure<SemanticRouterOptions>(builder.Configuration.GetSection(SemanticRouterOptions.SectionName));
builder.Services.Configure<RerankingOptions>(builder.Configuration.GetSection(RerankingOptions.SectionName));

var openAiOptions = builder.Configuration.GetSection(OpenAiOptions.SectionName).Get<OpenAiOptions>() ?? new OpenAiOptions();
var openAiApiKey = string.IsNullOrWhiteSpace(openAiOptions.ApiKey)
    ? builder.Configuration["OPENAI_API_KEY"]
    : openAiOptions.ApiKey;

if (!string.IsNullOrWhiteSpace(openAiApiKey))
{
    builder.Services.AddOpenAIChatCompletion(
        modelId: openAiOptions.Model,
        apiKey: openAiApiKey);
}

builder.Services.AddTransient(serviceProvider => new Kernel(serviceProvider));
builder.Services.AddHttpClient<WorkingMemoryStore>();
builder.Services.AddHttpClient<MemoryService>();
builder.Services.AddHttpClient<OpenAiEmbeddingVectorizer>();
builder.Services.AddSingleton<SemanticRoutingService>();
builder.Services.AddSingleton<RerankingService>();
builder.Services.AddTransient<RetrievalAugmentorService>();
builder.Services.AddTransient<OpenAiChatService>();
builder.Services.AddHostedService<FilesProcessor>();
builder.Services.AddHostedService<SemanticRouterInitializationService>();

var app = builder.Build();

app.UseCors(FrontendDevCorsPolicy);

var publishedWebRootPath = app.Environment.WebRootPath;
if (!string.IsNullOrWhiteSpace(publishedWebRootPath) && Directory.Exists(publishedWebRootPath))
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

var localFrontendBuildPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "frontend-layer", "build"));
if (Directory.Exists(localFrontendBuildPath))
{
    var provider = new PhysicalFileProvider(localFrontendBuildPath);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = provider });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = provider });
}

app.MapGet("/health", () => Results.Json(new { status = "UP" }));
app.MapControllers();

app.MapFallback(async context =>
{
    foreach (var path in new[]
             {
                 Path.Combine(app.Environment.WebRootPath ?? string.Empty, "index.html"),
                 Path.Combine(localFrontendBuildPath, "index.html")
             })
    {
        if (File.Exists(path))
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.SendFileAsync(path);
            return;
        }
    }

    context.Response.StatusCode = StatusCodes.Status404NotFound;
    await context.Response.WriteAsync(JsonSerializer.Serialize(new
    {
        error = "Frontend build not found. Run `npm run build` in frontend-layer or use Docker Compose."
    }));
});

app.Run();

static void LoadDotEnv(string envFilePath)
{
    if (!File.Exists(envFilePath))
    {
        return;
    }

    foreach (var rawLine in File.ReadAllLines(envFilePath))
    {
        var line = rawLine.Trim();
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
        {
            continue;
        }

        if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
        {
            line = line["export ".Length..].TrimStart();
        }

        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0)
        {
            continue;
        }

        var key = line[..separatorIndex].Trim();
        if (string.IsNullOrWhiteSpace(key) || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
        {
            continue;
        }

        var value = line[(separatorIndex + 1)..].Trim();
        if (value.Length >= 2)
        {
            var quote = value[0];
            if ((quote == '"' || quote == '\'') && value[^1] == quote)
            {
                value = value[1..^1];
            }
        }

        Environment.SetEnvironmentVariable(key, value);
    }
}
