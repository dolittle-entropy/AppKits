using Common.Models;

namespace Common.Commands
{
    public interface ICorrelatedCommand<out TEntity> : ICommand<TEntity>, IEntity
        where TEntity : IEntity
    {
        /// <summary>
        /// The name of the type of payload that we are issuing
        /// </summary>
        string? PayloadType { get; set; }

        /// <summary>
        /// The Correlation ID to be used towards an ERP Connector
        /// </summary>
        string? CorrelationId { get; set; }
    }
}