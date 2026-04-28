using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RedisVL.Vectorizers;
using Microsoft.Extensions.Options;
using BackendDotnetLayer.Configuration;

namespace BackendDotnetLayer.Services;

public sealed class OpenAiEmbeddingVectorizer : ITextVectorizer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _openAiOptions;
    private readonly SemanticRouterOptions _semanticRouterOptions;

    public OpenAiEmbeddingVectorizer(
        HttpClient httpClient,
        IOptions<OpenAiOptions> openAiOptions,
        IOptions<SemanticRouterOptions> semanticRouterOptions)
    {
        _httpClient = httpClient;
        _openAiOptions = openAiOptions.Value;
        _semanticRouterOptions = semanticRouterOptions.Value;
    }

    public async Task<float[]> VectorizeAsync(string input, CancellationToken cancellationToken = default)
    {
        var apiKey = string.IsNullOrWhiteSpace(_openAiOptions.ApiKey)
            ? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            : _openAiOptions.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is missing. Set OPENAI_API_KEY before using semantic routing.");
        }

        var requestBody = new EmbeddingRequest(
            _semanticRouterOptions.EmbeddingModel,
            input,
            _semanticRouterOptions.EmbeddingDimensions);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI embeddings request failed with status {(int)response.StatusCode}: {body}");
        }

        var embeddingResponse = JsonSerializer.Deserialize<EmbeddingResponse>(body, JsonOptions)
            ?? throw new InvalidOperationException("OpenAI embeddings response could not be parsed.");

        var embedding = embeddingResponse.Data.FirstOrDefault()?.Embedding;
        return embedding?.ToArray()
            ?? throw new InvalidOperationException("OpenAI embeddings response did not include an embedding vector.");
    }

    private sealed record EmbeddingRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] string Input,
        [property: JsonPropertyName("dimensions")] int Dimensions);

    private sealed record EmbeddingResponse(
        [property: JsonPropertyName("data")] List<EmbeddingItem> Data);

    private sealed record EmbeddingItem(
        [property: JsonPropertyName("embedding")] List<float> Embedding);
}
