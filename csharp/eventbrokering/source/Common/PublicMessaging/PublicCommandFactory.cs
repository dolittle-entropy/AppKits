using Common.Commands;
using Common.Models;

namespace Common.PublicMessaging;

public static class PublicCommandFactory
{
    const string CompanyName = "Demo Company AS";
    const string ApplicationName = "Demo 1.0";
    const string ApplicationId = "com.dolittle/enterprise/demo/1.0/";

    public static PublicCommand CreateInsertCommand(M3Command command, object caller)
        => CreateCommand($"Insert{command.EntityName!}", command, caller);

    public static PublicCommand CreateUpdateCommand(M3Command command, object caller)
        => CreateCommand($"Update{command.EntityName!}", command, caller);

    public static PublicCommand CreateDeleteCommand(M3Command command, object caller)
        => CreateCommand($"Delete{command.EntityName!}", command, caller);

    public static PublicCommand CreateUpsertCommand(M3Command command, object caller)
        => CreateCommand($"Upsert{command.EntityName!}", command, caller);

    public static PublicCommand CreateCustomCommand(M3Command command, object caller)
        => CreateCommand(command.EntityName!, command, caller);

    public static M3Command AsM3Command<TEntity, TCommand>(this TCommand command, string? payloadType = null)
        where TCommand : ICorrelatedCommand<TEntity>
        where TEntity : IEntity
    {
        return new M3Command
        {
            CorrelationId = command.CorrelationId,
            EntityName = payloadType ?? command.PayloadType,
            Payload = command.Payload,
            TenantId = command.TenantId.ToString()
        };
    }

    private static PublicCommand CreateCommand(string commandName, M3Command command, object? callingObject)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Payload is null)
            throw new ArgumentNullException(nameof(command.Payload));

        if (callingObject is null)
            throw new ArgumentNullException(nameof(callingObject));

        var nameArray = callingObject.GetType().FullName!.Split('.');
        var microservice = string.Join('.', nameArray[..2]);
        var source = nameArray[^1];

        return new PublicCommand
        {
            Metadata = new Metadata
            {
                MessageType = commandName,
                CorrelationId = command.CorrelationId,
                Tenant = new TenantInformation(CompanyName, Guid.Parse(command.TenantId!)),
                Application = new ApplicationInformation(ApplicationName),
                Microservice = new MicroserviceInformation(microservice!),
                SourceName = $"{ApplicationId}/{source}",
                Extended = new DolittleSourceInformation
                {
                    EventHandler = command.EventHandler,
                    SequenceNumber = command.FromSequenceNumber
                }
            },
            TraceId = Guid.NewGuid(),
            Payload = command.Payload
        };
    }
}