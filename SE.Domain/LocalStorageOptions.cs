namespace SE.Domain;

public class LocalStorageOptions : IStorageOptions
{
    public string Root { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
