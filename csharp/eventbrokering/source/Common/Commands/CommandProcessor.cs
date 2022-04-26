using Common.Exceptions;
using Common.Extensions;
using Common.GraphQL;
using Common.Helpers;
using Common.Models;
using Common.Rejections;
using Common.Repositories;
using Common.Validation;
using Dolittle.SDK;
using Dolittle.SDK.Aggregates;
using Dolittle.SDK.Tenancy;
using Serilog;

namespace Common.Commands;

public abstract class CommandProcessor<TEntity, TCommand> : CommandValidator<TEntity, TCommand>, ICommandProcessor<TCommand>
    where TEntity : IEntity
    where TCommand : ICommand<IEntity>
{
    readonly ILogger _log;
    readonly INotificationPublisher _notificationPublisher;

    public IDolittleClient DolittleClient { get; private set; }

    public string TenantId
        => DolittleClient.Tenants.FirstOrDefault() is Tenant tenant
            ? tenant.Id.ToString()
            : string.Empty;

    public CommandProcessor(
        IValidator<TEntity> entityValidator,
        IBusinessValidator<TCommand> businessValidator,
        IDolittleClient dolittleClient,
        INotificationPublisher notificationPublisher,
        ICollectionService collectionService)
        : base(entityValidator, businessValidator, collectionService)
    {
        _log = Log.ForContext<CommandProcessor<TEntity, TCommand>>();
        if (!dolittleClient.IsConnected)
            dolittleClient.Connect().Wait();

        DolittleClient = dolittleClient;

        _notificationPublisher = notificationPublisher;
    }

    public async Task SendToFrontend(FrontendNotification notification, CancellationToken cancellationToken = default)
        => await _notificationPublisher.Publish(notification, cancellationToken);

    public abstract Task<bool> Process(TCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform operation on Dolittle Aggregate. If it fails to execute within the provided retry policy,
    /// It will apply a Rejection to the command and write that into our rejection store.
    /// </summary>
    /// <returns>false only when writing to rejection fails. True when the operation was successful or the rejection was handled</returns>
    public async Task<bool> DoOnAggregate<TAggregate>(string? eventSourceId, TCommand command, Action<TAggregate> aggregateAction)
        where TAggregate : AggregateRoot
    {
        try
        {
            var asyncRetryPolicy = RetryPolicyBuilder.ForDolittleAggregate(this, _log)
                                   ?? throw new RetryPolicyBuilderFailed();

            return await asyncRetryPolicy.ExecuteAsync<bool>(async () =>
            {
                var tenantId = command.TenantId;
                if (tenantId == Guid.Empty)
                    tenantId = DolittleClient.Tenants.First().Id.Value;

                try
                {
                    await DolittleClient.PerformOnAggregate<TAggregate>(
                            eventSourceId: eventSourceId,
                            tenantId: tenantId,
                            action: equip => aggregateAction(equip))
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                return true;
            });
        }
        catch (Exception ex)
        {
            if (command is Rejection rejection)
            {
                PopulateCommandRejection(
                    command: command,
                    rejectedBy: nameof(CommandProcessor<TEntity, TCommand>));

                rejection.RejectionReason = ex.Message;
                rejection.FailingObject = command.ToString();

                return await WriteRejection(rejection);
            }
            return false;
        }
    }
}