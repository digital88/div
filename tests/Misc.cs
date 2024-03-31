using System.Text;
using div;
using FluentAssertions;

namespace tests;

public class Misc_Tests
{
    [Fact]
    public void Sha256_Layer_WithParent()
    {
        var layerDigest = "sha256:e692418e4cbaf90ca69d05a66403747baa33ee08806650b51fab815ad7fc331f";
        var parentId = "44d6d488d0fcd5f054fe925c83cbab87d2c5ca827578211c622c01615b995e92";
        var layerId = ShaUtils.ComputeSha256Hash(Encoding.UTF8.GetBytes($"{parentId}{Environment.NewLine}{layerDigest}{Environment.NewLine}"));
        layerId.Should().BeEquivalentTo("7d84a855f8df259faebe8739ea824cda636a48217a8c0c0380dd72aeb8d1fc69");
    }
}