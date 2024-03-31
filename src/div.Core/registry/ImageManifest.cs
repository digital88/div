using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace div;

/*

Docker Image Manifest

V2 schema docs here: https://github.com/distribution/distribution/blob/v2.8.3/docs/spec/manifest-v2-2.md

V1 schema docs here: https://github.com/distribution/distribution/blob/v2.8.3/docs/spec/manifest-v2-1.md

Every manifest version also have schema version. Schema corresponds to layer contents described as json file.

Like "Manifest V2 Schema V2", etc. A bit of confusing, I know.

*/

/* ========== V1 ========== */

public struct ImageManifestV1FSLayer
{
    [JsonPropertyName("blobSum")]
    public string BlobSum { get; set; }
}

public struct ImageManifestV1History
{
    [JsonPropertyName("v1Compatibility")]
    public string V1Compatibility { get; set; } // contains raw json string
}

public struct ImageManifestV1 : IImageManifest
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("fsLayers")]
    public ImageManifestV1FSLayer[] FSLayers { get; set; }

    [JsonPropertyName("history")]
    public ImageManifestV1History[] History { get; set; }

    public readonly string MediaType { get { return Consts.Manifest.ManifestV1; } }

    public readonly string[] GetLayers()
    {
        if (FSLayers == null) return Array.Empty<string>();
        return FSLayers.Select(x => x.BlobSum).Where(x => !string.IsNullOrEmpty(x)).ToArray();
    }

    public readonly string ImageId
    {
        get
        {
            var jsonStr = History.FirstOrDefault().V1Compatibility;
            if (string.IsNullOrEmpty(jsonStr)) return string.Empty;
            var obj = JsonSerializer.Deserialize<ImageJsonV10>(jsonStr);
            return obj.Id;
        }
        set
        {

        }
    }

    private readonly string GetLayerInternal(int index)
    {
        if (index < 0 || index >= FSLayers.Length) throw new ArgumentOutOfRangeException(nameof(index));

        var jsonStr = History[index].V1Compatibility;
        if (string.IsNullOrEmpty(jsonStr)) throw new InvalidOperationException($"Manifest history has no data in field 'V1Compatibility' for layer index {index}.");

        return jsonStr;
    }

    public readonly T? GetLayerAs<T>(int index) where T : IImageJson
    {
        var jsonStr = GetLayerInternal(index);
        try
        {
            return JsonSerializer.Deserialize<T>(jsonStr);
        }
        catch (Exception e)
        {
            Debug.Print(e.ToString());
            throw;
        }
    }
}

/* ========== V2 ManifestList ========== */

public struct ImageManifestV2SubmanifestPlatform
{
    [JsonPropertyName("architecture")]
    public string Architecture { get; set; }

    [JsonPropertyName("os")]
    public string Os { get; set; }

    [JsonPropertyName("os.version")]
    public string OsVersion { get; set; }

    [JsonPropertyName("os.features")]
    public string[] OsFeatures { get; set; }

    [JsonPropertyName("variant")]
    public string Variant { get; set; }

    [JsonPropertyName("features")]
    public string[] Features { get; set; }
}

public struct ImageManifestV2Submanifest
{
    [JsonPropertyName("mediaType")]
    public string MediaType { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("digest")]
    public string Digest { get; set; }

    [JsonPropertyName("platform")]
    public ImageManifestV2SubmanifestPlatform Platform { get; set; }
}

public struct ImageManifestListV2 : IImageManifest
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("mediaType")]
    public string MediaType { get; set; }

    [JsonPropertyName("manifests")]
    public ImageManifestV2Submanifest[] Manifests { get; set; }

    public string ImageId { readonly get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string[] GetLayers()
    {
        throw new NotImplementedException();
    }
}

/* ========== V2 Manifest ========== */

public struct ImageManifestV2Layer
{
    [JsonPropertyName("mediaType")]
    public string MediaType { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("digest")]
    public string Digest { get; set; }

    [JsonPropertyName("urls")]
    public string[] Urls { get; set; }
}

public struct ImageManifestV2Config
{
    [JsonPropertyName("mediaType")]
    public string MediaType { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("digest")]
    public string Digest { get; set; }
}

public struct ImageManifestV2 : IImageManifest
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("mediaType")]
    public string MediaType { get; set; }

    [JsonPropertyName("config")]
    public ImageManifestV2Config Config { get; set; }

    [JsonPropertyName("layers")]
    public ImageManifestV2Layer[] Layers { get; set; }

    public string ImageId { readonly get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string[] GetLayers()
    {
        throw new NotImplementedException();
    }
}