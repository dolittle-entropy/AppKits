using Common.Constants;
using GraphQlGateway.Stitching;
using HotChocolate.Types.Relay;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;

namespace GraphQlGateway;

public static class StitchingExtensions
{
    public static IServiceCollection SetupSchemaStitching(this IServiceCollection services, IConfiguration configuration)
    {
        // Connect to the Redis instance
        var microservices = configuration.GetSection("Microservices")["redis"];
        services.AddSingleton(ConnectionMultiplexer.Connect(microservices));

        // Register all the services that the gateway needs to connect to
        services.AddDownstreamClients(configuration);

        services
            .AddGraphQLServer()
            // .AddAuthorization()
            .AddType<NodeType>()
            .AddDirectiveMergeHandler<MergeDirectivesHandler>()
            .AddTypeMergeHandler<NsgMergeTypesHandler>()

            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddSubscriptionType<Subscription>()

            .AddSorting()
            .AddFiltering()
            .AddInMemorySubscriptions()
            .AddDefaultTransactionScopeHandler()

            .AddRemoteSchemasFromRedis(BoundedContexts.RedisDatabaseName, sp => sp.GetRequiredService<ConnectionMultiplexer>())
            .ModifyOptions(options => options.SortFieldsByName = true)
            .InitializeOnStartup();

        return services;
    }

    static void AddDownstreamClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClientForContext(configuration, BoundedContexts.Orders);
        services.AddHttpClientForContext(configuration, BoundedContexts.Warehouses);
        services.AddHttpClientForContext(configuration, BoundedContexts.Products);
    }

    static void AddHttpClientForContext(this IServiceCollection services, IConfiguration configuration, string contextName)
    {
        var url = configuration.GetSection("Microservices")[contextName];

        services.AddHttpClient(contextName, c =>
        {
            c.BaseAddress = new Uri($"http://{url}");

        }).AddHeaderPropagation(c => c.Headers.Add("Authorization"));
    }
}