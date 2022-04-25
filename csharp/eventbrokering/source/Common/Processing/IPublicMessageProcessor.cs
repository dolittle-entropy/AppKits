using Common.Models;

namespace Common.Processing
{
    public interface IPublicMessageProcessor
    {
        /// <summary>
        /// The MessageType for which this processor was made, i.e. 'UpsertOrder'
        /// </summary>
        string ForMessageType { get; }

        Task<bool> ProcessMessage(PublicMessage publicMessage);
    }

    public interface IPublicMessageProcessor<TCommand> : IPublicMessageProcessor, ICommandProcessor<TCommand>
        where TCommand : ICommand<IEntity>
    {
    }
}
