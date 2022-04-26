using Dolittle.SDK;
using Dolittle.SDK.Aggregates;
using Dolittle.SDK.Events;
using Dolittle.SDK.Projections;
using Dolittle.SDK.Resources;
using Dolittle.SDK.Tenancy;
using MediatR;
using MongoDB.Driver;

namespace Common.Extensions
{
    public static class DolittleServicesExtensions
    {
        private static IMediator? _mediator { get; set; }

        public static IMongoCollection<T>? AsMongoCollection<T>(this IResources resources, string collectionName)
            => resources.MongoDB.GetDatabase() is { } db
            ? db.GetCollection<T>(collectionName)
            : default;

        public static IDolittleClient CreateForMicroserviceId(this IDolittleClient dolittleClient, string microserviceId, IMediator mediator)
        {
            _mediator = mediator;
            return DolittleClient.Setup().Connect().Result;
        }

        public static async Task<TProjection?> GetProjection<TProjection>(this IDolittleClient client, TenantId tenantId, Key key) where TProjection : class, new()
            => await client.Projections.ForTenant(tenantId).Get<TProjection>(key) is { } projection
            ? projection
            : default;

        public static async Task<IEnumerable<TProjection>?> GetProjections<TProjection>(this IDolittleClient client, TenantId tenantId) where TProjection : class, new()
            => await client.GetProjections<TProjection>(tenantId) is { } projections
            ? projections
            : default;

        public static IAggregateRootOperations<TAggregate>? GetAggregateRoot<TAggregate>(this IDolittleClient client, EventSourceId eventSourceId, TenantId tenantId)
            where TAggregate : AggregateRoot
            => client.Aggregates is { } builder
            ? builder.ForTenant(tenantId).Get<TAggregate>(eventSourceId)
            : null;

        public static async Task PerformOnAggregate<TAggregate>(this IDolittleClient client, EventSourceId eventSourceId, TenantId tenantId, Action<TAggregate> action)
            where TAggregate : AggregateRoot
        {
            if (client.GetAggregateRoot<TAggregate>(eventSourceId, tenantId) is { } root)
            {
                await root.Perform(action);
            }
        }
    }
}
