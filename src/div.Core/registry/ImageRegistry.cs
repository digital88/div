namespace div;

public class ImageRegistry
{
    public string AuthBase { get; set; }
    public string AuthService { get; set; }
    public string RegistryBase { get; set; }
    public string? ImageNamePrefix { get; set; }

    public ImageRegistry(string authBase, string authService, string registryBase, string? imageNamePrefix)
    {
        ArgumentNullException.ThrowIfNull(authBase, nameof(authBase));
        ArgumentNullException.ThrowIfNull(authService, nameof(authService));
        ArgumentNullException.ThrowIfNull(registryBase, nameof(registryBase));

        AuthBase = authBase;
        AuthService = authService;
        RegistryBase = registryBase;

        ImageNamePrefix = imageNamePrefix;
    }

    public string GetImageNameWithPrefix(DockerImage image)
    {
        ArgumentNullException.ThrowIfNull(image, nameof(image));

        if (!string.IsNullOrEmpty(ImageNamePrefix))
            return $"{ImageNamePrefix}/{image.Name}";
        return image.Name;
    }

    public static readonly ImageRegistry Dockerhub = new("https://auth.docker.io", "registry.docker.io", "https://registry-1.docker.io", "library");
}