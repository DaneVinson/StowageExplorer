namespace SE.Domain;

public class StorageManager : IDisposable
{
    private readonly Dictionary<StorageNames, IFileStorage> _fileStorages;

    public StorageManager(List<IStorageOptions> options)
    {
        if (options == null) { throw new ArgumentNullException(nameof(options)); }

        var names = options.Select(o => o.Name).ToArray();
        if (names.Length != names.Distinct().Count() ||
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

    public async Task CopyFileAsync(
        StorageNames source,
        string sourcePath,
        StorageNames target,
        string targetPath,
        WriteMode writeMode = WriteMode.Create,
        CancellationToken cancellationToken = default)
    {
        await using var sourceStream = await GetFileStorage(source).OpenRead(sourcePath, cancellationToken);
        await using var targetStream = await GetFileStorage(target).OpenWrite(targetPath, writeMode, cancellationToken);
        await sourceStream.CopyToAsync(targetStream, cancellationToken);
    }

    public async Task CopyFolderAsync(
        StorageNames source,
        string sourcePath,
        StorageNames target,
        bool recursive,
        string targetPrependPath = "",
        WriteMode writeMode = WriteMode.Create,
        CancellationToken cancellationToken = default)
    {
        var sourceStorage = GetFileStorage(source);
        var files = await sourceStorage.Ls(sourcePath, recursive, cancellationToken);

        await Task.WhenAll(files.Select(f => CopyFileAsync(
                                                    source,
                                                    f.Path,
                                                    target,
                                                    $"/{targetPrependPath}/{f.Path}",
                                                    writeMode,
                                                    cancellationToken)));
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
                { } type when type == typeof(AzureStorageOptions) => GetAzureStorage((AzureStorageOptions)option),
                { } type when type == typeof(LocalStorageOptions) => GetLocalStorage((LocalStorageOptions)option),
                _ => throw new NotSupportedException($"{option.GetType()} is not a supported option type")
            };
            fileStorages.Add(storageName, fileStorage);
        }
        return fileStorages;


        IFileStorage GetAzureStorage(AzureStorageOptions storageOptions) =>
            Files.Of.AzureBlobStorage(
                        storageOptions.AccountName,
                        storageOptions.Key,
                        storageOptions.ContainerName);

        IFileStorage GetLocalStorage(LocalStorageOptions storageOptions) =>
            Files.Of.LocalDisk(storageOptions.Root);
    }

    private bool Disposed { get; set; }
}
