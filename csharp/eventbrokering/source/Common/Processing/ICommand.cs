﻿using Common.Models;

namespace Common.Processing
{
    public interface ICommand<out TEntity> where TEntity : IEntity
    {
        /// <summary>
        /// The id of the Tenant that this command was issued for
        /// </summary>
        Guid TenantId { get; set; }

        /// <summary>
        /// The payload of the command
        /// </summary>
        TEntity? Payload { get; }
    }
}
