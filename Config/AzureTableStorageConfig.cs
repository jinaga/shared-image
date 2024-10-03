namespace SharedImage.Config;

public class AzureTableStorageConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
}
