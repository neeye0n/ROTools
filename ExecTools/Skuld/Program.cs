using Microsoft.Extensions.DependencyInjection;
using Skuld.App;
using Skuld.Config;
using Skuld.Diagnostics;
using Skuld.Sources;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

var services = new ServiceCollection();
ConfigureServices(services);
await using var provider = services.BuildServiceProvider();

var app = provider.GetRequiredService<AppRunner>();
await app.RunAsync();

if (!Console.IsInputRedirected)
{
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}

static void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IAppNotifier, SpectreAppNotifier>();

    services.AddHttpClient<RemoteSourceProvider>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(20);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Skuld/1.0");
    });

    services.AddSingleton<ISourceProvider, LocalFileSourceProvider>();
    services.AddSingleton<ISourceProvider>(sp => sp.GetRequiredService<RemoteSourceProvider>());

    services.AddSingleton<ConfigService>();
    services.AddSingleton<CategoryDataFetcher>();
    services.AddSingleton<AppRunner>();
}