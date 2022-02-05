namespace SE.ConsoleApp;

public class Program
{
    private const string _23skidooFileName = "23skidoo.txt";

    private static async Task Main(string[] args)
    {
        try
        {
            var serviceProvider = NewServiceProvider();

            using var storageManager = serviceProvider.GetRequiredService<StorageManager>();

            var localStorage = storageManager.GetFileStorage(StorageNames.LocalStorage);
            var tempStorage = storageManager.GetFileStorage(StorageNames.TempStorage);
            var cloudStorage1 = storageManager.GetFileStorage(StorageNames.Cloud1);

            // Write a file to temp storage
            await tempStorage.WriteText(_23skidooFileName, "23 skidoo!");

            // Copy the file from temp storage to a new folder in local storage
            await StorageManager.CopyFileAsync(
                                    tempStorage,
                                    _23skidooFileName,
                                    localStorage,
                                    $"/newfolder5/{_23skidooFileName}");

            // Delete the file from temp storage
            await tempStorage.Rm(_23skidooFileName);

            // Copy files from local storage to a new folder in cloud storage
            await StorageManager.CopyFolderAsync(
                                    localStorage,
                                    "/source/",
                                    true,
                                    cloudStorage1,
                                    "/newfolder23/");
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
                        options.AddRange(configuration.GetSection("AzureStorage").Get<IEnumerable<AzureStorageOptions>>());
                        options.AddRange(configuration.GetSection("LocalStorage").Get<IEnumerable<LocalStorageOptions>>());
                        return options;
                    })
                    .AddSingleton<StorageManager>()
                    .BuildServiceProvider();
    }
}