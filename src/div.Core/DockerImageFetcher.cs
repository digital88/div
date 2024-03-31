using System.Text;
using System.Text.Json;

namespace div;

public class DockerImageFetcher
{
    private readonly ILayerStorage _storage;
    private readonly ImageRegistryClient _client;

    public DockerImageFetcher(ILayerStorage storage, ImageRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(storage);

        _storage = storage;

        _client = new ImageRegistryClient(new HttpClient(), new ImageRegistryClientUriBuilder(registry));
    }

    public ILayerStorage Storage
    {
        get
        {
            return _storage;
        }
    }

    public async Task FetchAsync(IEnumerable<string?> images, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(images);
        foreach (var image in images)
        {
            if (string.IsNullOrEmpty(image)) continue;

            var di = DockerImage.FromString(image);
            var auth = await _client.GetAuthTokenAsync(di, token);
            var manifest = await _client.GetManifestAsync(di, auth, token);
            var ctx = new RegistryContext(di, auth, manifest);
            switch (manifest.SchemaVersion)
            {
                case 2:
                    {
                        switch (manifest.MediaType)
                        {
                            case Consts.Manifest.ManifestListV2: await FetchV2ManifestList(ctx, token); break;
                            case Consts.Manifest.ManifestV2: await FetchV2Manifest(ctx, token); break;
                            default: throw new ApplicationException($"MediaType {manifest.MediaType} is unsupported.");
                        }
                        break;
                    }
                case 1: await FetchV1Manifest(ctx, token); break;
                default: throw new ApplicationException($"SchemaVersion {manifest.SchemaVersion} is unsupported.");
            }
        }
    }
    const string jsonTemplate = @"{
  ""id"": ""%layerId"",
  ""created"": ""0001-01-01T00:00:00Z"",
  ""container_config"": {
    ""Hostname"": "",
    ""Domainname"": "",
    ""User"": "",
    ""AttachStdin"": false,
    ""AttachStdout"": false,
    ""AttachStderr"": false,
    ""Tty"": false,
    ""OpenStdin"": false,
    ""StdinOnce"": false,
    ""Env"": null,
    ""Cmd"": null,
    ""Image"": "",
    ""Volumes"": null,
    ""WorkingDir"": "",
    ""Entrypoint"": null,
    ""OnBuild"": null,
    ""Labels"": null
  }
}
";
    const string jsonTemplateWithParent = @"{
  ""id"": ""%layerId"",
  ""parent"": ""%parentId"",
  ""created"": ""0001-01-01T00:00:00Z"",
  ""container_config"": {
    ""Hostname"": "",
    ""Domainname"": "",
    ""User"": "",
    ""AttachStdin"": false,
    ""AttachStdout"": false,
    ""AttachStderr"": false,
    ""Tty"": false,
    ""OpenStdin"": false,
    ""StdinOnce"": false,
    ""Env"": null,
    ""Cmd"": null,
    ""Image"": "",
    ""Volumes"": null,
    ""WorkingDir"": "",
    ""Entrypoint"": null,
    ""OnBuild"": null,
    ""Labels"": null
  }
}
";
    private async Task FetchV2Manifest(RegistryContext ctx, CancellationToken token)
    {
        var di = ctx.DockerImage;
        var auth = ctx.ImageRegistryAccessToken;
        var manifest = (ImageManifestV2)ctx.Manifest;
        if (string.IsNullOrEmpty(manifest.Config.Digest)) throw new ApplicationException("manifest.Config.Digest should be set.");

        var configName = manifest.Config.Digest.Replace("sha256:", string.Empty) + ".json";
        using var blob = new MemoryStream(await _client.GetBlob(di, manifest.Config.Digest, auth, token));
        var configFilename = await _storage.StoreBlobAsync(blob, configName, token);

        var parentLayer = string.Empty;
        var layerId = string.Empty;
        foreach (var layer in manifest.Layers)
        {
            if (layer.MediaType != "application/vnd.docker.image.rootfs.diff.tar.gzip")
                throw new ApplicationException($"Unknown layer media type {layer.MediaType}");
            layerId = ShaUtils.ComputeSha256Hash(Encoding.UTF8.GetBytes($"{parentLayer}{Environment.NewLine}{layer.Digest}{Environment.NewLine}"));
            var template = string.IsNullOrEmpty(parentLayer) ? jsonTemplate : jsonTemplateWithParent;
            using var tar = new MemoryStream(await _client.GetBlob(di, layer.Digest, auth, token));
            var il = new ImageLayer(layerId, "1.0", template.Replace("%layerId", layerId).Replace("%parentId", parentLayer), tar);
            await _storage.StoreLayerAsync(il, token);
            parentLayer = layerId;
        }
        var imageId = layerId;
        using var s = File.Open(configFilename, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var json = JsonSerializer.Deserialize<ImageJsonV10>(s);
        json.Id = imageId;
        json.Parent = parentLayer;
        JsonSerializer.Serialize(s, json);
        await s.FlushAsync(token);
    }

    private async Task FetchV2ManifestList(RegistryContext ctx, CancellationToken token)
    {
        var di = ctx.DockerImage;
        var auth = ctx.ImageRegistryAccessToken;
        var manifest = (ImageManifestListV2)ctx.Manifest;
        var currArch = OsArchitecture.GetCurrent();
        var currentArchManifest = manifest.Manifests.Where(m => m.Platform.Architecture == currArch).FirstOrDefault();
        if (string.IsNullOrEmpty(currentArchManifest.Digest))
        {
            throw new ApplicationException($"(Sub)Manifest with current process arch: {currArch} not found.");
        }

        var subManifest = await _client.GetManifestAsync(di, currentArchManifest.Digest, auth, token);
        var childCtx = new RegistryContext(di, auth, subManifest);
        await FetchV2Manifest(ctx, token);
    }

    private async Task FetchV1Manifest(RegistryContext ctx, CancellationToken token)
    {
        var di = ctx.DockerImage;
        var auth = ctx.ImageRegistryAccessToken;
        var manifest = (ImageManifestV1)ctx.Manifest;
        for (var i = 0; i < manifest.FSLayers.Length; i++)
        {
            var layerId = manifest.GetLayerAs<ImageJsonV10>(i).Id;
            using var tar = new MemoryStream(await _client.GetBlob(di, manifest.FSLayers[i].BlobSum, auth, token));
            var il = new ImageLayer(layerId, "1.0", manifest.History[i].V1Compatibility, tar);
            await _storage.StoreLayerAsync(il, token);
        }
    }

    // public async Task MakeRepositories()
    // {
    //     await Task.Delay(0);
    // }
}