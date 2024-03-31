using System.Text.Json;

namespace div;

public struct RepositoriesJson
{
    public RepositoriesJson()
    {
    }

    private Dictionary<string, Dictionary<string, string>> Data { get; set; } = new();

    public readonly void AddImageWithTags(DockerImage di, IImageManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(di, nameof(di));

        var imageName = di.Name;
        var k = di.ImageTagOrDigest;
        var v = manifest.ImageId;
        if (Data.ContainsKey(imageName))
        {
            var info = Data[imageName];
            if (!info.ContainsKey(k))
                info.Add(k, v);
        }
        else
        {
            Data.Add(imageName, new Dictionary<string, string>() { { k, v } });
        }
    }

    public readonly string Json
    {
        get
        {
            return JsonSerializer.Serialize(Data);
        }
    }
}