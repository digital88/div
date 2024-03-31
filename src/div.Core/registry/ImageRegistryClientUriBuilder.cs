namespace div;

public class ImageRegistryClientUriBuilder
{
    private readonly ImageRegistry _registry;

    public ImageRegistryClientUriBuilder(ImageRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry, nameof(registry));

        _registry = registry;
    }

    public string TokenUrl(DockerImage image)
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));

        return $"{_registry.AuthBase}/token?service={_registry.AuthService}&scope=repository:{_registry.GetImageNameWithPrefix(image)}:pull";
    }

    public string ManifestUrl(DockerImage image, string digest)
    {
        return $"{_registry.RegistryBase}/v2/{_registry.GetImageNameWithPrefix(image)}/manifests/{digest}";
    }

    public string ManifestUrl(DockerImage image)
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));

        return $"{_registry.RegistryBase}/v2/{_registry.GetImageNameWithPrefix(image)}/manifests/{image.ImageManifestId}";
    }

    public string BlobUrl(DockerImage image, string layerHash)
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));
        ArgumentNullException.ThrowIfNull(layerHash, nameof(layerHash));

        return $"{_registry.RegistryBase}/v2/{_registry.GetImageNameWithPrefix(image)}/blobs/{layerHash}";
    }
}