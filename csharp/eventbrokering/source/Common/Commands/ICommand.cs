using Common.Models;

namespace Common.Commands
{

    public interface ICommand<out TEntity>
        where TEntity : IEntity
    {
        /// <summary>
        /// The id of the Tenant that this command was issued for
        /// </summary>
        Guid TenantId { get; set; }

        /// <summary>
        /// User or system that initiated the command
        /// </summary>
        string? IssuedBy { get; set; }

        /// <summary>
        /// The payload of the command
        /// </summary>
        TEntity? Payload { get; }
    }
}