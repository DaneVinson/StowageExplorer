namespace SE.Domain;

public class StorageManager : IDisposable
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

    ~StorageManager()
    {
        Dispose(false);
    }

    public static async Task CopyFileAsync(
        IFileStorage sourceStorage,
        string sourcePath,
        IFileStorage targetStorage, 
        string targetPath, 
        WriteMode writeMode = WriteMode.Create)
    {
        using (var sourceStream = await sourceStorage.OpenRead(sourcePath))
        using (var targetStream = await targetStorage.OpenWrite(targetPath, writeMode))
        {
            await sourceStream.CopyToAsync(targetStream);
        }
    }

    public static async Task CopyFolderAsync(
        IFileStorage sourceStorage,
        string sourcePath,
        bool recursive,
        IFileStorage targetStorage,
        string targetPrependPath = "",
        WriteMode writeMode = WriteMode.Create)
    {
        var files = await sourceStorage.Ls(sourcePath, recursive);
        var tasks = new List<Task>();
        var streams = new List<Stream>();
        try
        {
            foreach (var file in files)
            {
                var sourceStream = await sourceStorage.OpenRead(file.Path);
                var targetStream = await targetStorage.OpenWrite($"/{targetPrependPath}/{file.Path}", WriteMode.Create);
                streams.AddRange(new[] { sourceStream, targetStream });
                tasks.Add(sourceStream.CopyToAsync(targetStream));
            }

            await Task.WhenAll(tasks);
        }
        finally
        {
            streams.ForEach(s => s.Dispose());
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (Disposed) { return; }

        if (disposing) 
        {
            foreach (var storage in _fileStorages.Values)
            {
                storage.Dispose();
            }
            _fileStorages.Clear();
        }
        Disposed = true;
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
                Type type when type == typeof(AzureStorageOptions) => GetAzureStorage((AzureStorageOptions)option),
                Type type when type == typeof(LocalStorageOptions) => GetLocalStorage((LocalStorageOptions)option),
                _ => throw new NotSupportedException($"{option.GetType()} is not a supported option type")
            };
            fileStorages.Add(storageName, fileStorage);
        }
        return fileStorages;


        IFileStorage GetAzureStorage(AzureStorageOptions options) =>
            Files.Of.AzureBlobStorage(
                        options.AccountName,
                        options.Key,
                        options.ContainerName);

        IFileStorage GetLocalStorage(LocalStorageOptions options) =>
            Files.Of.LocalDisk(options.Root);
    }

    private bool Disposed { get; set; }
}

