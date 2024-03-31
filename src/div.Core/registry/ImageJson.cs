using System.Text.Json.Serialization;

namespace div;

public struct ImageJsonV10Config
{
    [JsonPropertyName("User")]
    public string User { get; set; }

    [JsonPropertyName("Memory")]
    public long Memory { get; set; }

    [JsonPropertyName("MemorySwap")]
    public long MemorySwap { get; set; }

    [JsonPropertyName("CpuShares")]
    public long CpuShares { get; set; }

    [JsonPropertyName("ExposedPorts")]
    public Dictionary<string, object> ExposedPorts { get; set; }

    [JsonPropertyName("Env")]
    public string[] Env { get; set; }

    [JsonPropertyName("Entrypoint")]
    public string[] Entrypoint { get; set; }

    [JsonPropertyName("Cmd")]
    public string[] Cmd { get; set; }

    [JsonPropertyName("Volumes")]
    public Dictionary<string, object> Volumes { get; set; }

    [JsonPropertyName("WorkingDir")]
    public string WorkingDir { get; set; }
}

public struct ImageJsonV10 : IImageJson
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("parent")]
    public string Parent { get; set; }

    [JsonPropertyName("checksum")]
    public string Checksum { get; set; }

    [JsonPropertyName("created")]
    public string Created { get; set; }

    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("architecture")]
    public string Architecture { get; set; }

    [JsonPropertyName("os")]
    public string Os { get; set; }

    [JsonPropertyName("Size")]
    public long Size { get; set; }

    [JsonPropertyName("config")]
    public ImageJsonV10Config Config { get; set; }
}

public interface IImageJson
{

}