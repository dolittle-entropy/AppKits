using Common.Models;

namespace Common.Validation
{
    public interface IValidator<TEntity>
        where TEntity : IEntity
    {
        bool Validate(TEntity? entity);
    }
}
