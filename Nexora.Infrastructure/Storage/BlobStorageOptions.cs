namespace Nexora.Infrastructure.Storage;

public class BlobStorageOptions
{
    public const string Section = "AzureStorage";
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}