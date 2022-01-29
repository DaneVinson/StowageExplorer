namespace SE.Domain;

public class StorageManager
{
    private readonly Dictionary<StorageNames, IFileStorage> _fileStorages;

    public StorageManager(List<IStorageOptions> options)
    {
        if (options == null) { throw new ArgumentNullException(nameof(options)); }

        var names = options.Select(o => o.Name);
        if (options.Count != names.Distinct().Count() ||
            names.Any(name => !Enum.TryParse(name, out StorageNames _))) 
        { 
            throw new ArgumentException($"Option names must be unique and defined by {nameof(StorageNames)}"); 
        }

        _fileStorages = GetInitializedFileStores(options);
    }

    public IFileStorage GetFileStorage(StorageNames name)
    {
        if (!_fileStorages.ContainsKey(name)) 
        { 
            throw new InvalidOperationException($"No file storage is registered for storage name {name}"); 
        }

        return _fileStorages[name];
    }

    private Dictionary<StorageNames, IFileStorage> GetInitializedFileStores(List<IStorageOptions> options)
    {
        var fileStorages = new Dictionary<StorageNames, IFileStorage>();
        foreach (var option in options)
        {
            var storageName = Enum.Parse<StorageNames>(option.Name);
            var fileStorage = option.GetType() switch
            {
                Type type when type == typeof(AzureStorageOptions) => GetAzureStorage1((AzureStorageOptions)option),
                Type type when type == typeof(LocalStorageOptions) => GetLocalStorage1((LocalStorageOptions)option),
                _ => throw new NotSupportedException($"{option.GetType()} is not a supported option type")
            };
            fileStorages.Add(storageName, fileStorage);
        }
        return fileStorages;


        IFileStorage GetAzureStorage1(AzureStorageOptions options) =>
            Files.Of.AzureBlobStorage(
                        options.AccountName,
                        options.Key,
                        options.ContainerName);

        IFileStorage GetLocalStorage1(LocalStorageOptions options) =>
            Files.Of.LocalDisk(options.Root);
    }
}

