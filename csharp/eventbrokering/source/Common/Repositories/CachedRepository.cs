using Common.Commands;
using Common.Models;
using MongoDB.Driver;

namespace Common.Repositories;

public class CachedRepository<TEntity, TCommand> : Repository<TCommand>, ICachedRepository<TEntity, TCommand>
    where TCommand : class, ICorrelatedCommand<TEntity>, new()
    where TEntity : class, IEntity, new()
{
    public CachedRepository(IMongoCollection<TCommand> collection)
        : base(collection)
    {
    }

    public async Task<TCommand?> Pop(string correlationId, CancellationToken cancellationToken = default)
    {
        if (await GetOne(x => x.CorrelationId == correlationId, cancellationToken) is { } command)
        {
            if (await Delete(x => x.CorrelationId == correlationId, cancellationToken))
                return command;
        }
        return default;
    }

    public async Task<TCommand?> Peek(string correlationId, CancellationToken cancellationToken = default)
    {
        return await GetOne(x => x.CorrelationId == correlationId, cancellationToken);
    }

    public async Task<IEnumerable<TEntity>?> GetAllTypesOf(string entityType, CancellationToken cancellationToken = default)
    {
        var results = new List<TEntity>();
        var filter = Builders<TCommand>.Filter.Eq(x => x.PayloadType, entityType);
        if (await GetCollection.Find(filter).ToListAsync(cancellationToken) is List<TCommand> commands)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i].Payload is TEntity entity)
                    results.Add(entity);
            }
        }
        return results;
    }

    public async Task<bool> Push(TCommand command, CancellationToken cancellationToken = default)
        => await Upsert(x => x.CorrelationId! == command.CorrelationId, command, cancellationToken) != null;
}