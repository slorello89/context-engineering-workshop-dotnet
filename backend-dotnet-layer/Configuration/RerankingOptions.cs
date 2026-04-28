namespace BackendDotnetLayer.Configuration;

public sealed class RerankingOptions
{
    public const string SectionName = "Reranking";

    public string ModelPath { get; init; } =
        "Assets/ms-marco-MiniLM-L-6/model.onnx";

    public string TokenizerPath { get; init; } =
        "Assets/ms-marco-MiniLM-L-6/tokenizer.json";

    public int MaxSequenceLength { get; init; } = 512;

    public double? ScoreThreshold { get; init; } = 0.8d;

    public int CandidatesPerSource { get; init; } = 5;

    public int TopN { get; init; } = 3;
}
