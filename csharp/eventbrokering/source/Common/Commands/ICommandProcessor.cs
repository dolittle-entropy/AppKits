using Common.GraphQL;
using Common.Models;

namespace Common.Commands
{
    public interface ICommandProcessor<TCommand>
        where TCommand : ICommand<IEntity>
    {
        Task<bool> Process(TCommand command, CancellationToken cancellationToken = default);

        Task SendToFrontend(FrontendNotification notification, CancellationToken cancellationToken = default);
    }
}
