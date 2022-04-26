using Common.Rejections;
using Common.Repositories;
using MongoDB.Driver;

namespace Common.GraphQL
{
    public class BaseGQLQuery
    {
        public const string CollectionName = "Rejections";
        IMongoCollection<Rejection> _rejections;

        public BaseGQLQuery(ICollectionService collectionService, string contextName)
        {
            _rejections = collectionService.GetMongoCollection<Rejection>($"{CollectionName}_{contextName}")!;
        }

        [UsePaging(MaxPageSize = 200), UseProjection, UseSorting, UseFiltering]
        public IExecutable<Rejection> GetRejections(string context, string area)
        {
            return _rejections!
                .Find(x => x.BoundedContext == context && x.SpecificArea == area)
                .AsExecutable();
        }

        public async Task<IEnumerable<FailedContext>> GetRejectedContexts()
        {
            return await _rejections.Aggregate()
                .Group(
                    foo => foo.BoundedContext,
                    bar => new FailedContext { Context = bar.Key, FailedCount = bar.Sum(a => 1) })
                .ToListAsync();
        }

        public async Task<IEnumerable<FailedArea>> GetRejectedAreas(string context)
        {
            var filter = Builders<Rejection>.Filter.Eq(x => x.BoundedContext, context);
            return await _rejections.Aggregate()
                .Match(filter)
                .Group(
                    foo => foo.SpecificArea,
                    bar => new FailedArea { Area = bar.Key, FailedCount = bar.Sum(a => 1) }
                ).ToListAsync();
        }
    }
}
