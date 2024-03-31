namespace div;

public class DockerImage
{
    public string Name { get; set; }
    public string? Tag { get; set; }
    public string? Digest { get; set; }

    public DockerImage(string? name, string? tag, string? digest)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));

        Name = name;
        Tag = tag;
        Digest = digest;
    }

    public string ImageTagOrDigest
    {
        get
        {
            return !string.IsNullOrEmpty(Tag) ? Tag : !string.IsNullOrEmpty(Digest) ? Digest.Replace("sha256:", string.Empty) : Name;
        }
    }

    public string ImageManifestId
    {
        get
        {
            return !string.IsNullOrEmpty(Digest) ? Digest : !string.IsNullOrEmpty(Tag) ? Tag : Name;
        }
    }

    private static readonly string TagSeparator = ":";
    private static readonly int TagSeparatorLength = TagSeparator.Length;

    private static readonly string DigestSeparator = "@";
    private static readonly int DigestSeparatorLength = DigestSeparator.Length;

    public override string ToString()
    {
        var result = $"{Name}";
        if (!string.IsNullOrEmpty(Tag))
            result = $"{result}{TagSeparator}{Tag}";
        if (!string.IsNullOrEmpty(Digest))
            result = $"{result}{DigestSeparator}{Digest}";
        return result;
    }

    public static DockerImage FromString(string? imageString)
    {
        ArgumentNullException.ThrowIfNull(imageString, nameof(imageString));

        var indexOfDigestSeparator = imageString.IndexOf(DigestSeparator);
        var indexOfTagSeparator = imageString.IndexOf(TagSeparator);

        if (indexOfTagSeparator > indexOfDigestSeparator && indexOfDigestSeparator >= 0)
            return new(imageString[..indexOfDigestSeparator], null, imageString[(indexOfDigestSeparator + DigestSeparatorLength)..]);

        if (indexOfTagSeparator < 0)
            return new(imageString, null, null);

        var imageName = imageString[..indexOfTagSeparator];

        if (indexOfDigestSeparator < 0)
        {
            var tag = imageString[(indexOfTagSeparator + TagSeparatorLength)..];
            return new(imageName, tag, null);
        }
        else
        {
            var tag = imageString.Substring(indexOfTagSeparator + TagSeparatorLength, indexOfDigestSeparator - indexOfTagSeparator - TagSeparatorLength);
            var digest = imageString[(indexOfDigestSeparator + DigestSeparatorLength)..];
            return new(imageName, tag, digest);
        }
    }
}