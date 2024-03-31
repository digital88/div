using System.Text.Json.Serialization;

namespace div;

public struct ImageRegistryAccessToken
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("issued_at")]
    public DateTimeOffset IssuedAt { get; set; }
}