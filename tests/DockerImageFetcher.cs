using div;
using FluentAssertions;

namespace tests;

public class DockerImageFetcher_Tests
{
    [Theory]
    [InlineData("alpine:2.6", 1024 * 1024 * 2, 1024 * 300, "should be about 1.9 MB")]
    public async Task DockerImageFetcher_FetchesAllLayers(string image, long imageSize, ulong delta, string because)
    {
        var fetcher = new DockerImageFetcher(new UnitTestMemoryStorage(), ImageRegistry.Dockerhub);
        await fetcher.FetchAsync(new[] { image }, new CancellationToken());
        fetcher.Storage.ImageLayersSize.Should().BeCloseTo(imageSize, delta, because);
    }
}