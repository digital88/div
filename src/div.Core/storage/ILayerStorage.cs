namespace div;

public interface ILayerStorage
{
    string RootFolder { get; }

    long ImageLayersSize { get; }

    void Clean();

    Task StoreLayerAsync(ImageLayer layer, CancellationToken token);

    Task<string> StoreBlobAsync(Stream blob, string filename, CancellationToken token);
}