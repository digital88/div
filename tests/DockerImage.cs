using div;
using FluentAssertions;

namespace tests;

public class DockerImage_Tests
{
    internal const string Name = "hello-world";
    internal const string Tag = "latest";
    internal const string Digest = "sha256:8be990ef2aeb16dbcb9271ddfe2610fa6658d13f6dfb8bc72074cc1ca36966a7";

    internal const string Image1 = $"{Name}:{Tag}@{Digest}";
    internal const string Image2 = $"{Name}:{Tag}";
    internal const string Image3 = $"{Name}@{Digest}";
    internal const string Image4 = $"{Name}";

    public static IEnumerable<object[]> DockerImage_FromString_Args(int _)
    {
        yield return new object[] {
            Image1,
            new DockerImage(Name, Tag, Digest)
        };
        yield return new object[] {
            Image2,
            new DockerImage(Name, Tag, null)
        };
        yield return new object[] {
            Image3,
            new DockerImage(Name, null, Digest)
        };
        yield return new object[] {
            Image4,
            new DockerImage(Name, null, null)
        };
    }

    [Theory]
    [MemberData(nameof(DockerImage_FromString_Args), 1)]
    public void DockerImage_FromString(string imageString, DockerImage equiv)
    {
        DockerImage.FromString(imageString).Should().BeEquivalentTo(equiv);
    }

    [Theory]
    [InlineData(Image1)]
    [InlineData(Image2)]
    [InlineData(Image3)]
    [InlineData(Image4)]
    public void DockerImage_ToString(string imageString)
    {
        var di = DockerImage.FromString(imageString);

        di.ToString().Should().Be(imageString);
    }
}