namespace SharedImage.Config;

public class AzureBlobStorageConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}