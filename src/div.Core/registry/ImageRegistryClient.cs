using System.Net.Http.Json;
using System.Text.Json;

namespace div;

public class ImageRegistryClient
{
    private readonly ImageRegistryClientUriBuilder _registryUriBuilder;
    private readonly HttpClient _client;

    public ImageRegistryClient(HttpClient client, ImageRegistryClientUriBuilder registryUriBuilder)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(registryUriBuilder, nameof(registryUriBuilder));

        _client = client;
        _registryUriBuilder = registryUriBuilder;
    }

    private readonly JsonSerializerOptions _jso = new();

    public async Task<ImageRegistryAccessToken> GetAuthTokenAsync(DockerImage image, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));

        var response = await _client.GetAsync(_registryUriBuilder.TokenUrl(image), token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ImageRegistryAccessToken>(_jso, token);
    }

    private async Task<IImageManifest> InternalGetManifestAsync(DockerImage image, string? digest, ImageRegistryAccessToken auth, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));

        using var message = new HttpRequestMessage();
        if (!string.IsNullOrEmpty(digest))
            message.RequestUri = new Uri(_registryUriBuilder.ManifestUrl(image, digest));
        else
            message.RequestUri = new Uri(_registryUriBuilder.ManifestUrl(image));
        message.Method = HttpMethod.Get;
        message.Headers.Clear();
        message.Headers.Add("Authorization", $"Bearer {auth.Token}");
        message.Headers.Add("Accept", new[]
        {
            Consts.Manifest.ManifestV2,
            Consts.Manifest.ManifestListV2,
            Consts.Manifest.ManifestV1
        });
        var response = await _client.SendAsync(message, token);
        response.EnsureSuccessStatusCode();
        var str = await response.Content.ReadAsStringAsync(token);
        var manifestType = response.Content.Headers.ContentType?.MediaType;
        if (string.IsNullOrEmpty(manifestType)) throw new ApplicationException("Response header Content-type is null or empty.");
        return manifestType switch
        {
            Consts.Manifest.ManifestV1Jws => JsonSerializer.Deserialize<ImageManifestV1>(str),
            Consts.Manifest.ManifestV1 => JsonSerializer.Deserialize<ImageManifestV1>(str),
            Consts.Manifest.ManifestV2 => JsonSerializer.Deserialize<ImageManifestV2>(str),
            Consts.Manifest.ManifestListV2 => JsonSerializer.Deserialize<ImageManifestListV2>(str),
            _ => throw new ApplicationException($"Response header Content-type contains unsupported content type {manifestType}."),
        };
    }

    public async Task<IImageManifest> GetManifestAsync(DockerImage image, string digest, ImageRegistryAccessToken auth, CancellationToken token)
    {
        return await InternalGetManifestAsync(image, digest, auth, token);
    }

    public async Task<IImageManifest> GetManifestAsync(DockerImage image, ImageRegistryAccessToken auth, CancellationToken token)
    {
        return await InternalGetManifestAsync(image, null, auth, token);
    }

    public async Task<byte[]> GetBlob(DockerImage image, string layerHash, ImageRegistryAccessToken auth, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));
        ArgumentNullException.ThrowIfNull(layerHash, nameof(layerHash));

        using var message = new HttpRequestMessage();
        message.RequestUri = new Uri(_registryUriBuilder.BlobUrl(image, layerHash));
        message.Method = HttpMethod.Get;
        message.Headers.Clear();
        message.Headers.Add("Authorization", $"Bearer {auth.Token}");
        var response = await _client.SendAsync(message, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(token);
    }
}
