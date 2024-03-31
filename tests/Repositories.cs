using div;
using FluentAssertions;

namespace tests;

internal struct TestImageManifest : IImageManifest
{
    private string _imageId;
    public string ImageId { readonly get => _imageId; set => _imageId = value; }
    public int SchemaVersion { readonly get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public readonly string MediaType => throw new NotImplementedException();

    public readonly string[] GetLayers()
    {
        return Array.Empty<string>();
    }
}

public class Repositories_Tests
{
    public static IEnumerable<object[]> Repositories_AddImageWithTags_Test2_Args(int _)
    {
        yield return new object[] {
            "imageId123123",
            @"{""hello-world"":{""latest"":""imageId123123""},""debian"":{""latest"":""imageId123123"",""buster"":""imageId123123""},""postgres"":{""a3eaf95f4c7c30a7dcdd80459891ac6b04f9602ff433124944457facb886c53b"":""imageId123123""},""ubuntu"":{""latest"":""imageId123123""}}",
            new[] {
                "hello-world:latest",
                "debian:latest",
                "debian:buster",
                "postgres@sha256:a3eaf95f4c7c30a7dcdd80459891ac6b04f9602ff433124944457facb886c53b",
                "ubuntu:latest@sha256:26c68657ccce2cb0a31b330cb0be2b5e108d467f641c62e13ab40cbec258c68d"
            }
        };
    }

    [Theory]
    [MemberData(nameof(Repositories_AddImageWithTags_Test2_Args), 1)]
    public void Repositories_AddImageWithTags_Test2(string imageIdMock, string repositoryJson, string[] images)
    {
        var r = new RepositoriesJson();
        var manifest = new TestImageManifest
        {
            ImageId = imageIdMock
        };
        foreach (var imgStr in images)
        {
            var di = DockerImage.FromString(imgStr);
            r.AddImageWithTags(di, manifest);
        }
        r.Json.Should().BeEquivalentTo(repositoryJson);
    }

    [Fact]
    public void Repositories_AddImageWithTags_Test1()
    {
        var manifest = new TestImageManifest()
        {
            ImageId = Guid.NewGuid().ToString()
        };

        var r = new RepositoriesJson();
        var di = DockerImage.FromString(DockerImage_Tests.Image1);
        r.AddImageWithTags(di, manifest);
        r.Json.Should().BeEquivalentTo($"{{\"{di.Name}\":{{\"{di.ImageTagOrDigest}\":\"{manifest.ImageId}\"}}}}");
    }
}