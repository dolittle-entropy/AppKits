using FluentValidation;
using ValidateOrder = Common.Validation.IValidator<Sample.Orders.Orders.Order>;

namespace Sample.Orders.Orders
{
    public class OrderValidator : AbstractValidator<Order>, ValidateOrder
    {
        public OrderValidator()
        {
            RuleFor(x => x.Id).Must(id => id != Guid.Empty);
            RuleFor(x => x.IsSynchronized)
                .NotNull();

            RuleFor(x => x.CreatedBy)
                .NotNull()
                .NotEmpty()
                .MinimumLength(4)
                .MaximumLength(50);

            RuleFor(x => x.LastModifiedBy)
                .NotNull()
                .NotEmpty()
                .MinimumLength(4)
                .MaximumLength(50);

            RuleFor(x => x.Created)
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-3))
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));

            RuleFor(x => x.LastModified)
                .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-3))
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
        }

        public new bool Validate(Order? order)
        {
            if (order is null)
                return false;

            return base.Validate(order).IsValid;
        }
    }
}
