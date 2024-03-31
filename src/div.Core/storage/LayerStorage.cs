namespace div;

public class FSLayerStorage : ILayerStorage
{
    private readonly DirectoryInfo _rootFolder;

    public FSLayerStorage()
    {
        _rootFolder = Directory.CreateDirectory(Path.Join(Path.GetTempPath(), $"div_{Guid.NewGuid()}"));
    }

    public string RootFolder
    {
        get
        {
            return _rootFolder.FullName;
        }
    }

    private static long DirSize(DirectoryInfo d, string pattern)
    {
        long size = 0;
        FileInfo[] files = d.GetFiles(pattern);
        foreach (FileInfo fi in files)
        {
            size += fi.Length;
        }
        DirectoryInfo[] dirs = d.GetDirectories();
        foreach (DirectoryInfo di in dirs)
        {
            size += DirSize(di, pattern);
        }
        return size;
    }

    public long ImageLayersSize
    {
        get
        {
            return DirSize(_rootFolder, "*.tar");
        }
    }

    public void Clean()
    {
        foreach (var file in _rootFolder.EnumerateFiles())
            file.Delete();
        foreach (var dir in _rootFolder.EnumerateDirectories())
            dir.Delete(true);
    }

    public async Task<string> StoreBlobAsync(Stream blob, string filename, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(blob, nameof(blob));

        var fullname = Path.Join(_rootFolder.FullName, filename);
        using var t = File.Create(fullname);
        await blob.CopyToAsync(t, token);
        await t.FlushAsync(token);
        return fullname;
    }

    public async Task StoreLayerAsync(ImageLayer layer, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(layer, nameof(layer));

        var layerId = layer.LayerId;
        var subdir = _rootFolder.CreateSubdirectory(layerId);

        if (token.IsCancellationRequested) return;

        using var v = File.CreateText(Path.Join(subdir.FullName, "VERSION"));
        await v.WriteAsync(layer.Version);

        using var j = File.CreateText(Path.Join(subdir.FullName, "json"));
        await v.WriteAsync(layer.Json);

        using var t = File.Create(Path.Join(subdir.FullName, "layer.tar"));
        if (layer.Tar != null)
        {
            await layer.Tar.CopyToAsync(t, token);
            await t.FlushAsync(token);
        }
    }
}