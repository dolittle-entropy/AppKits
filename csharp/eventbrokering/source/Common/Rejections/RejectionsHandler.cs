using Common.Constants;
using Common.Extensions;
using Common.Repositories;
using Dolittle.SDK;
using Serilog;

namespace Common.Rejections
{

    public class RejectionsHandler
    {
        readonly ILogger _log;
        readonly ICollectionService _collectionService;

        public RejectionsHandler(ILogger log, IDolittleClient dolittleClient, ICollectionService collectionService)
        {
            _log = log.ForContext<RejectionsHandler>();
            _collectionService = collectionService;
        }

        public async Task Handle(Rejection rejection, CancellationToken cancellationToken)
        {
            if (Guid.TryParse(rejection.FromTenantId, out Guid tenantId))
            {
                var collectionName = ResolveRejectionTableName(rejection);
                var repo = _collectionService.GetRepository<Rejection>(collectionName);

                await repo.Upsert(x => x.Id == rejection.Id, rejection).ConfigureAwait(false);
            }
            _log.Leave(this, $"{rejection.RejectedBy}.{rejection.SpecificArea} REJECTED: {rejection.RejectionReason}");
        }

        static string ResolveRejectionTableName(Rejection evt)
        {
            var contextName = evt.BoundedContext?.Replace("Context", "").ToLower() ?? "unknown";
            return $"{CollectionNames.Rejections}_{contextName}";
        }
    }
}

