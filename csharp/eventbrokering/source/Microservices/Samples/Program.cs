using Configuration;
using Dolittle.SDK;
using Lamar.Microsoft.DependencyInjection;
using Sample.Orders;
using Serilog;
using System.Reflection;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = ConfigureAppExtensions.CreateMicroserviceLogger(enableDolittleDiagnostics: false);
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseLamar()
            .UseSerilog()
            .UseDolittle()

            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureAppConfiguration(config => config
                        .AddUserSecrets(Assembly.GetEntryAssembly(), optional: true)
                        .AddEnvironmentVariables()
                );
                webBuilder.UseStartup<Startup>();
            });
}