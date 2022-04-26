using Common.Models;
using Common.Rejections;
using Common.Repositories;
using Common.Validation;
using Common.Constants;
using Serilog;
using Common.Exceptions;

namespace Common.Commands
{
    /// <summary>
    /// Offers Validation of Commands and Entities and produces rejections when they fail to pass validation
    /// </summary>
    public class CommandValidator<TEntity, TCommand>
        where TEntity : IEntity
        where TCommand : ICommand<IEntity>
    {
        readonly IValidator<TEntity> _entityValidator;
        readonly IBusinessValidator<TCommand> _businessValidator;

        IRepository<Rejection>? _rejections;
        ICollectionService _collectionService;

        public CommandValidator(IValidator<TEntity> entityValidator, IBusinessValidator<TCommand> businessValidator, ICollectionService collectionService)
        {
            _entityValidator   = entityValidator;
            _businessValidator = businessValidator;
            _collectionService = collectionService;
        }

        public async Task<bool> CommandIsValid(TCommand? command, CancellationToken cancellationToken = default)
        {            
            try
            {
                if (command is null)
                    return false;

                if (command is ICorrelatedCommand<TEntity> trans)
                {
                    if (!Constants.Validation.User.IsValidUserId(trans.IssuedBy))
                        return false;                    
                }

                if (command?.Payload is { } payload)
                {
                    if (!_entityValidator.Validate((TEntity?)payload))
                        return false;
                }

                if (!await _businessValidator.Validate(command!))
                {
                    if (command is Rejection rejectable)
                    {
                        if (rejectable.Rejected)
                        {
                            PopulateCommandRejection(command, _businessValidator.GetType().Name);
                            await WriteRejection(rejectable, cancellationToken);
                        }
                    }
                    return false;
                }
            }
            catch (ValidationFailed ex)
            {
                if (command is Rejection rejectable)
                {
                    PopulateCommandRejection(command, _entityValidator.GetType().Name);
                    rejectable.RejectionReason = ex.Message;
                    rejectable.FailingObject = command.Payload;

                    await WriteRejection(rejectable, cancellationToken);
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"UNKNOWN ERROR: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
            return true;
        }

        public void PopulateCommandRejection(TCommand command, string rejectedBy)
        {
            if (command is Rejection rejectable)
            {

                var boundedContext = "Unknown";
                var specificArea = "Unknown";
                var sourceBits = command.Payload?.GetType()?.FullName?.Split('.') ?? null;
                if (sourceBits != null)
                {
                    boundedContext = sourceBits[0].Equals(CollectionNames.CustomerPrefix, StringComparison.InvariantCultureIgnoreCase) ? sourceBits[1] : sourceBits[0];
                    specificArea   = sourceBits[0].Equals(CollectionNames.CustomerPrefix, StringComparison.InvariantCultureIgnoreCase) ? sourceBits[2] : sourceBits[1];
                }
                rejectable.Rejected = true;                
                rejectable.RejectedBy = rejectedBy;
                rejectable.FromTenantId = command.TenantId.ToString();
                rejectable.BoundedContext = boundedContext;
                rejectable.SpecificArea = specificArea;
                rejectable.FailingObject = command.Payload;
                rejectable.IsSynchronized = true;
            }
        }

        public async Task<bool> WriteRejection(Rejection rejection, CancellationToken cancellationToken = default)
        {
            if (_rejections is null)
            {
                var tableName = ResolveRejectionTableName(rejection);
                _rejections = _collectionService.GetRepository<Rejection>(tableName);
            }
            return await _rejections.Upsert(r => r.Id == rejection.Id, rejection, cancellationToken).ConfigureAwait(false)
                is Rejection
                ? true
                : false;
        }

        static string ResolveRejectionTableName(Rejection rejection)
        {
            var contextName = rejection.BoundedContext?.Replace("Context", "").ToLower() ?? "unknown";
            return $"{CollectionNames.Rejections}_{contextName}";
        }
    }
}
