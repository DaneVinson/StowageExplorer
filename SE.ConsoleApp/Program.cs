namespace SE.ConsoleApp;

public class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            var serviceProvider = NewServiceProvider();

            var factory = serviceProvider.GetRequiredService<StorageManager>();
            var fileStorage = factory.GetFileStorage(StorageNames.LocalStorage);
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