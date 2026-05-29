using ItemDescTableModder.Services;
using ItemDescTableModder.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ItemDescTableModder
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string executableDirectory = Environment.CurrentDirectory;

            var services = new ServiceCollection();

            // Initial Config and Logger setup
            services
                .AddSingleton(executableDirectory)
                .AddLogging(configure =>
                {
                    configure.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "[HH:mm:ss] ";
                    });
                })
                .AddSingleton<IConfigLoader, ConfigLoader>();

            // Creating tempProvider to load config
            var tempProvider = services.BuildServiceProvider();
            var configLoader = tempProvider.GetRequiredService<IConfigLoader>();
            var config = configLoader.Load();

            // Setup DI
            services.AddHttpClient("ItemDescTableModder", client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5);
                client.BaseAddress = new Uri(config.ResourceUrl);
            }).RemoveAllLoggers();

            services
                .RemoveAll<IConfigLoader>()
                .AddSingleton(Options.Create(config))
                .AddSingleton<IApp, App>()
                .AddScoped<ILuaTableHandler, LuaTableHandler>()
                .AddScoped<ILuaTableModifier, LuaTableModifier>()
                .AddScoped<ILuaTableSerializer, LuaTableSerializer>();

            var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
            // Get service
            var app = provider.GetRequiredService<IApp>();
            var logger = provider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Check if a file was provided as an argument (dragged onto the exe)
                if (args.Length > 0)
                {
                    foreach (var file in args)
                    {
                        if (File.Exists(file))
                        {
                            logger.LogInformation("Processing file: {filePath}", Path.GetFileName(file));
                            await app.ProcessFile(file);
                        }
                    }

                    logger.LogInformation("Processing completed.");
                }
                else
                {
                    logger.LogWarning("No file provided or file does not exist.");
                    logger.LogWarning("Please drag a file onto the application to process it.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing the file.");
            }

            // Keep console window open
            logger.LogInformation("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

