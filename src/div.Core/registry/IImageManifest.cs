namespace div;

public interface IImageManifest
{
    int SchemaVersion { get; set; }
    string MediaType { get; }
    string ImageId { get; set; }
    string[] GetLayers();
}