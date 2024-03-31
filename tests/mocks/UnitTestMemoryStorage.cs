namespace div;

public class UnitTestMemoryStorage : ILayerStorage
{
    private readonly string _dir = Path.Join(Path.GetTempPath(), $"div_{Guid.NewGuid()}");
    public string RootFolder => _dir;

    private long _size;
    public long ImageLayersSize => _size;

    public void Clean()
    {
        _size = 0;
    }

    public Task StoreLayerAsync(ImageLayer layer, CancellationToken token)
    {
        _size += layer.Tar?.Length ?? 0;

        return Task.CompletedTask;
    }

    public Task<string> StoreBlobAsync(Stream blob, string filename, CancellationToken token)
    {
        return Task.FromResult(string.Empty);
    }
}