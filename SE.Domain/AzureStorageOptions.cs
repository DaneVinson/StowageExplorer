namespace SE.Domain;

public class AzureStorageOptions : IStorageOptions
{
    public string AccountName { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
