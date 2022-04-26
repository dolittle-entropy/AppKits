using Common.Messaging;
using Dolittle.SDK;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Configuration
{
    public static class ConfigureAppExtensions
    {
        public static ILogger CreateMicroserviceLogger(bool enableDolittleDiagnostics = false)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug();

            if (enableDolittleDiagnostics)
            {
                loggerConfiguration.MinimumLevel.Override("Dolittle", LogEventLevel.Debug);
            }
            else
            {
                loggerConfiguration.MinimumLevel.Override("Dolittle", LogEventLevel.Error);
            }
            loggerConfiguration
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.Debug()
                .WriteTo.Console();
            return loggerConfiguration.CreateLogger();
        }

        public static void StartMicroserviceContext(this IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration, bool enableAuthentication, bool enableMessageBrokering = true)
        {
            Log.Information("Waiting for Dolittle client");
            var client = app.ApplicationServices.GetRequiredService<IDolittleClient>();
            client.Connected.Wait();
            Log.Information($"Dolittle Client is connected: {client.IsConnected}");

            var runtimeConfig = configuration.GetSection("DOLITTLE")?.GetSection("RUNTIME");
            if (runtimeConfig != null)
            {
                var dolittleHost = runtimeConfig["HOST"] ?? "unknown";
                var dolittlePort = runtimeConfig["PORT"] ?? "default";

                Log.Information($"Dolittle Runtime Host: {dolittleHost}");
                Log.Information($"Dolittle Runtime Port: {dolittlePort}");
            }

            app.UseCors("AllowAllHeaders");
            app.UseForwardedHeaders(env);
            app.UseWebSockets();
            app.UseRouting();

            if (enableAuthentication)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }
            app.UseHealthChecks("/health");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var basePath = Environment.GetEnvironmentVariable("MicroserviceName") ?? string.Empty;
            var schemaPath = "/schema";
            if (!string.IsNullOrEmpty(basePath))
            {
                schemaPath = $"/{basePath}/schema";
            }

            app.UseEndpoints(endpoints =>
            {
                if (enableAuthentication)
                {
                    endpoints.MapGraphQL($"/{basePath}").RequireAuthorization();
                }
                else
                {
                    endpoints.MapGraphQL($"/{basePath}");
                }
                endpoints.MapGraphQLSchema($"{schemaPath}");
            });

            Log.Information($"GraphQL Endpoint: {basePath}");
            Log.Information($"GraphQL Schema Endpoint: {schemaPath}");

            if (enableMessageBrokering)
                app.ApplicationServices.GetRequiredService<IPublicMessageHandler>().StartProcessing();
        }

        static IApplicationBuilder UseForwardedHeaders(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            var forwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,

            };
            forwardedHeaderOptions.KnownNetworks.Clear();
            forwardedHeaderOptions.KnownProxies.Clear();

            return app.UseForwardedHeaders(forwardedHeaderOptions);
        }
    }
}
