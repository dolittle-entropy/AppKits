using Common.Models;
using Common.Validation;

namespace Common.Commands
{
    public class DefaultBusinessValidator<TCommand> : IBusinessValidator<TCommand>
        where TCommand : ICommand<IEntity>
    {
        public Task<bool> Validate(TCommand command)
        {
            return Task.FromResult(true);
        }
    }
}