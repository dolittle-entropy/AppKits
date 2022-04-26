using Common.Commands;
using Common.Models;

namespace Common.Validation
{
    public interface IBusinessValidator<TCommand>
        where TCommand : ICommand<IEntity>
    {
        Task<bool> Validate(TCommand command);
    }
}