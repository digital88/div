namespace div;

public class ImageLayer
{
    public string LayerId { get; set; }

    public string Version { get; set; }

    public string Json { get; set; }

    public Stream? Tar { get; set; }

    public ImageLayer(string layerId, string version, string json, Stream? tar)
    {
        if (string.IsNullOrEmpty(layerId)) throw new ArgumentNullException(nameof(layerId));
        if (string.IsNullOrEmpty(version)) throw new ApplicationException(nameof(version));
        if (string.IsNullOrEmpty(json)) throw new ApplicationException(nameof(json));

        LayerId = layerId;
        Version = version;
        Json = json;
        Tar = tar;
    }
}