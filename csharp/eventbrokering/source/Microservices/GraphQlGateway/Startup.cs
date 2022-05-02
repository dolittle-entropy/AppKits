using Common.GraphQL;
using GraphQlGateway.Services;
using HotChocolate.Subscriptions;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace GraphQlGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureContainer(ServiceRegistry services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders", builder => builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(origin => true)
                    .AllowCredentials()
                );
            });
            ConfigureAuthentication(services);
            services.AddHealthChecks();

            var apiKey = Environment.GetEnvironmentVariable("API_KEY") ?? string.Empty;

            // Configure GraphQL to act as a Gateway for all downstream services
            services.SetupSchemaStitching(Configuration);

            services.For<INotificationPublisher>().Use(sp => 
            {                
                var eventSender = sp.GetInstance<ITopicEventSender>();
                return new FrontendNotificationPublisher(Log.Logger, eventSender);
            });
            services.AddHostedService<NotificationClient>();
        }

        void ConfigureAuthentication(ServiceRegistry services)
        {

            //services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            //    .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
            //    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes: new[] { "user.read" })
            //    .AddInMemoryTokenCaches();

            //services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("AllowAllHeaders");
            app.UseHeaderPropagation();

            var pathBase = Configuration["PathBase"] ?? string.Empty;
            app.UsePathBase(pathBase);

            // app.UseMiddleware<ApiKeyMiddleware>();
            app.UseRouting();
            //app.UseAuthentication();
            //app.UseAuthorization();
            app.UseHealthChecks("/health");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseWebSockets();
            var basePath = Environment.GetEnvironmentVariable("MicroserviceName") ?? string.Empty;
            var schemaPath = "/schema";
            if (!string.IsNullOrEmpty(basePath))
            {
                schemaPath = $"/{basePath}/schema";
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL($"/{basePath}"); // .RequireAuthorization();
                endpoints.MapGraphQLSchema($"{schemaPath}");
            });
        }
    }
}
