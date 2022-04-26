using Common.Models;
using Common.Validation;

namespace Common.Commands
{
    public class DefaultValidator<TEntity> : IValidator<TEntity>
        where TEntity : IEntity
    {
        public bool Validate(TEntity? entity)
        {
            return true;
        }
    }
}
