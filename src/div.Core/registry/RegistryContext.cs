namespace div;

public class RegistryContext
{
    public DockerImage DockerImage { get; }

    public ImageRegistryAccessToken ImageRegistryAccessToken { get; }

    public IImageManifest Manifest { get; }

    public RegistryContext(DockerImage di, ImageRegistryAccessToken token, IImageManifest manifest)
    {
        DockerImage = di;
        ImageRegistryAccessToken = token;
        Manifest = manifest;
    }
}