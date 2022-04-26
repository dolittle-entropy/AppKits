using Brokers;
using Common.Commands;
using Common.GraphQL;
using Common.Messaging;
using Common.Models;
using Common.Processing;
using Common.Rejections;
using Common.Repositories;
using Common.Validation;
using Dolittle.SDK;
using HotChocolate.Execution.Configuration;
using Lamar;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Serilog;
using Serilog.Extensions.Logging;
using StackExchange.Redis;

namespace Configuration;

public static class ConfigurationExtensions
{
    private static IConfiguration? _configuration;

    public static ServiceRegistry ConfigureMicroservice(this ServiceRegistry services,
        IConfiguration configuration,
        bool enableAuthentication,
        bool enableMessageBroker)
    {
        _configuration = configuration;

        services.AddHealthChecks();
        services.AddOptions();

        services.ConfigureCors();

        if (enableAuthentication)
            services.ConfigureAuthentication(configuration);

        services.ConfigureLogging();

        if (enableMessageBroker)
            services.ConfigureMessaging();

        return services;
    }

    public static IRequestExecutorBuilder ConfigureGraphQL(this ServiceRegistry services, bool enableAuthentication, bool enableRedisGateway = true)
    {
        // Add a connection to our Redis server
        if(enableRedisGateway)
        {
            var redisConnectionString = _configuration!.GetSection("Microservices")["redis"];
            Log.Information($"Redis instance in use: {redisConnectionString}");
            services.AddSingleton(ConnectionMultiplexer.Connect(redisConnectionString));
        }

        var builder = services.AddGraphQLServer();

        if (enableAuthentication)
        {
            builder.AddAuthorization();
        }

        if(enableRedisGateway)
        {
            builder.AddRedisSubscriptions(sp => sp.GetRequiredService<ConnectionMultiplexer>());
        }
        else
        {
            builder.AddInMemorySubscriptions();
        }

        return builder
            // MongoDb
            .AddMongoDbFiltering()
            .AddMongoDbSorting()
            .AddMongoDbProjections()
            .AddMongoDbPagingProviders()

            .AddDefaultTransactionScopeHandler()            

            .AddProjections()
            .AddSorting()
            .AddFiltering();
    }

    static ServiceRegistry ConfigureCors(this ServiceRegistry services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllHeaders", builder => builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()
            );
        });
        return services;
    }

    static ServiceRegistry ConfigureAuthentication(this ServiceRegistry services, IConfiguration configuration)
    {
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(configuration.GetSection("AzureAd"));

        services.AddAuthorization();
        services.AddControllers(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });

        return services;
    }

    static ServiceRegistry ConfigureLogging(this ServiceRegistry services)
    {
        // Make ILogger available to anyone needing it
        services.AddLogging(x => x.AddSerilog());
        services.AddSingleton(Log.Logger);
        services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(Log.Logger));

        return services;
    }

    static ServiceRegistry ConfigureMessaging(this ServiceRegistry services)
    {
        services.For(typeof(IValidator<>)).Use(typeof(DefaultValidator<>)).Singleton();
        services.For(typeof(IBusinessValidator<>)).Use(typeof(DefaultBusinessValidator<>)).Singleton();
        services.For<INotificationPublisher>().Use<FrontendNotificationPublisher>();

        services.Scan(x =>
        {
            x.AssembliesAndExecutablesFromApplicationBaseDirectory();
            x.ConnectImplementationsToTypesClosing(typeof(IValidator<>), ServiceLifetime.Singleton);
            x.ConnectImplementationsToTypesClosing(typeof(ICorrelatedCommand<>), ServiceLifetime.Singleton);
            x.ConnectImplementationsToTypesClosing(typeof(ICommand<>), ServiceLifetime.Singleton);
            x.ConnectImplementationsToTypesClosing(typeof(IBusinessValidator<>), ServiceLifetime.Singleton);
            x.AddAllTypesOf(typeof(IEntity));
            x.ConnectImplementationsToTypesClosing(typeof(ICommandProcessor<>));
            x.AddAllTypesOf<IPublicMessageProcessor>(ServiceLifetime.Singleton).NameBy(p => p.FullName);
            x.AddAllTypesOf<IReceiptProcessor>(ServiceLifetime.Singleton).NameBy(p => p.FullName);
        });

        // services.AddSingleton<INotificationHandler<PublicMessage>, PublicMessageHandler>();
        services.AddSingleton<RejectionsHandler, RejectionsHandler>();

        // We only want a single mediator
        services.AddSingleton<IMessageConsumer, KafkaConsumer>();
        services.AddSingleton<IReceiptConsumer, KafkaReceiptConsumer>();
        services.AddSingleton<IMessagePublisher, KafkaPublisher>();

        services
            .For<ICollectionService>()
            .Use(container =>
            {
                var client = container.GetInstance<IDolittleClient>();
                var tenant = client.Tenants.First();
                var resources = client.Resources.ForTenant(tenant.Id);
                return new CollectionService(resources);
            }).Singleton();

        services
            .For<IPublicMessageHandler>()
            .Use(container =>
            {
                var publicMessageDictionary = container
                    .GetAllInstances<IPublicMessageProcessor>()
                    .ToDictionary(proc => proc.ForMessageType);

                var receiptDictionary = container
                    .GetAllInstances<IReceiptProcessor>()
                    .ToDictionary(proc => proc.ForReceiptType);

                return new PublicMessageHandler(
                    container.GetInstance<IMessageConsumer>(),
                    container.GetInstance<IReceiptConsumer>(),
                    container.GetInstance<IDolittleClient>().Tenants.Select(t => t.Id.Value).ToList(),                    
                    publicMessageDictionary,
                    receiptDictionary);
            })
            .Singleton();

        return services;
    }
}