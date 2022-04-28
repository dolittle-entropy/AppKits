using FluentValidation;
using ValidateWarehouse = Common.Validation.IValidator<Sample.Warehousing.Warehouses.Warehouse>;

namespace Sample.Warehousing.Warehouses
{
    public class WarehouseValidator : AbstractValidator<Warehouse>, ValidateWarehouse
    {
        public WarehouseValidator()
        {
            RuleFor(x => x.Id).Must(id => id != Guid.Empty);
            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty()
                .Length(min: 3, max: 50);

            RuleFor(x => x.Description)
                .NotNull()
                .Length(min: 0, max: 255);

            RuleFor(x => x.CreatedBy)
                .NotNull()
                .NotEmpty()
                .Length(min: 3, max: 50);

            RuleFor(x => x.LastModifiedBy)
                .NotNull()
                .NotEmpty()
                .Length(min: 3, max: 50);

            RuleFor(x => x.Created)
                .InclusiveBetween(
                    from: DateTime.UtcNow.AddYears(-3),
                    to: DateTime.UtcNow.AddHours(24));

            RuleFor(x => x.LastModified)
                .InclusiveBetween(
                    from: DateTime.UtcNow.AddYears(-3),
                    to: DateTime.UtcNow.AddHours(24));
        }

        public new bool Validate(Warehouse? entity)
        {
            if (entity is null)
                return false;

            return base.Validate(entity).IsValid;
        }
    }
}
