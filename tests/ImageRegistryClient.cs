using div;
using FluentAssertions;

namespace tests;

public class ImageRegistryClient_Tests
{
    internal static readonly HttpClient Client = new();

    private readonly DockerImage _dockerImage;
    private readonly ImageRegistryClient _client;
    private readonly CancellationToken _ct;
    public ImageRegistryClient_Tests()
    {
        _dockerImage = DockerImage.FromString(DockerImage_Tests.Image1);
        _client = new ImageRegistryClient(Client, new ImageRegistryClientUriBuilder(ImageRegistry.Dockerhub));
        _ct = new CancellationToken();
    }

    [Fact]
    public async Task ImageRegistryClient_GetsToken()
    {
        var token = await _client.GetAuthTokenAsync(_dockerImage, _ct);
        token.Should().Match<ImageRegistryAccessToken>(t => !string.IsNullOrEmpty(t.Token));
    }

    [Theory]
    [InlineData(DockerImage_Tests.Image1)]
    [InlineData("debian:latest")]
    [InlineData("debian:buster")]
    [InlineData("postgres@sha256:a3eaf95f4c7c30a7dcdd80459891ac6b04f9602ff433124944457facb886c53b")]
    [InlineData("ubuntu:latest@sha256:26c68657ccce2cb0a31b330cb0be2b5e108d467f641c62e13ab40cbec258c68d")]
    public async Task ImageRegistryClient_GetsManifest(string imageString)
    {
        var di = DockerImage.FromString(imageString);
        var authToken = await _client.GetAuthTokenAsync(di, _ct);
        var manifest = await _client.GetManifestAsync(di, authToken, _ct);
        manifest.Should().Match<IImageManifest>(m => m.SchemaVersion == 1 || m.SchemaVersion == 2);
    }

    [Fact]
    public async Task ImageRegistryClient_GetsLayer()
    {
        const string layerHashPrefix = "sha256:";
        var authToken = await _client.GetAuthTokenAsync(_dockerImage, _ct);
        var manifest = await _client.GetManifestAsync(_dockerImage, authToken, _ct);
        var layers = manifest.GetLayers();
        layers.Should().NotBeEmpty();
        layers.Length.Should().BeGreaterThan(1);
        var hash = layers[1];
        hash.Should().NotBeNullOrEmpty();
        var layer = await _client.GetBlob(_dockerImage, hash, authToken, _ct);
        layer.Should().NotBeNull();
        layer.Length.Should().BePositive();
        var computedHash = ShaUtils.ComputeSha256Hash(layer);
        hash.Replace(layerHashPrefix, string.Empty).Should().BeEquivalentTo(computedHash);
    }
}