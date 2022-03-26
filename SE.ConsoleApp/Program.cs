namespace SE.ConsoleApp;

public static class Program
{
    private const string _23skidooFileName = "23skidoo.txt";

    private static async Task Main(string[] args)
    {
        try
        {
            var serviceProvider = NewServiceProvider();

            using var storageManager = serviceProvider.GetRequiredService<StorageManager>();
            var tempStorage = storageManager.GetFileStorage(StorageNames.TempStorage);

            // Write a file to temp storage
            await tempStorage.WriteText(_23skidooFileName, "23 skidoo!");

            // Copy the file from temp storage to a new folder in local storage
            await storageManager.CopyFileAsync(
                                    StorageNames.TempStorage,
                                    _23skidooFileName,
                                    StorageNames.LocalStorage,
                                    $"/newfolder5/{_23skidooFileName}");

            // Delete the file from temp storage
            await tempStorage.Rm(_23skidooFileName);

            // Copy files from local storage to a new folder in cloud storage
            await storageManager.CopyFolderAsync(
                                    StorageNames.LocalStorage,
                                    "/source/",
                                    StorageNames.Cloud1,
                                    true,
                                    "/newfolder23/");

            // Delete all files from the new folder on the cloud storage
            var cloudStorage1 = storageManager.GetFileStorage(StorageNames.Cloud1);
            await cloudStorage1.Rm("/newfolder23/", true);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"{exception.GetType()} - {exception.Message}");
            Console.WriteLine(exception.StackTrace ?? string.Empty);
        }
        finally
        {
            Console.WriteLine();
            Console.WriteLine("...");
            Console.ReadKey();
        }
    }

    private static IServiceProvider NewServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json")
                                    .AddJsonFile("appsettings.Development.json", true)
                                    .Build();

        return new ServiceCollection()
                    .AddSingleton(_ => {
                        var options = new List<IStorageOptions>();
                        options.AddRange(configuration.GetSection(nameof(AzureStorageOptions)).Get<IEnumerable<AzureStorageOptions>>());
                        options.AddRange(configuration.GetSection(nameof(LocalStorageOptions)).Get<IEnumerable<LocalStorageOptions>>());
                        return options;
                    })
                    .AddSingleton<StorageManager>()
                    .BuildServiceProvider();
    }
}