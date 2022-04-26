using Common.Commands;
using Common.Models;

namespace Common.Repositories
{
    public interface ICachedRepository<TEntity, TCommand> : IRepository<TCommand>
        where TCommand : class, ICorrelatedCommand<TEntity>, new()
        where TEntity : class, IEntity, new()
    {
        Task<bool> Push(TCommand command, CancellationToken cancellationToken = default);

        Task<TCommand?> Pop(string correlationId, CancellationToken cancellationToken = default);

        Task<TCommand?> Peek(string correlationId, CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>?> GetAllTypesOf(string entityType, CancellationToken cancellationToken = default);
    }
}
