using CoRuntime;
using Microsoft.Extensions.Hosting;

namespace ServiceApp;

class Program
{
    static async Task Main(string[] args)
    {
        var appBuilder = Host.CreateApplicationBuilder(args);
        RuntimeBootstrap.Startup(new RuntimeHandler(), appBuilder.Services, appBuilder.Configuration, args);
        var host = appBuilder.Build();
        await host.RunAsync();
    }
}