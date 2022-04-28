using Common.Extensions;
using Common.Validation;

namespace Sample.Warehousing.Warehouses
{
    public class UpsertWarehouseFromM3BusinessValidator : IBusinessValidator<UpsertWarehouseFromM3>
    {
        static List<string> EXCLUDED_WAREHOUSES = new List<string> { "GOVT", "MAIN" };

        public Task<bool> Validate(UpsertWarehouseFromM3 command)
        {
            if(EXCLUDED_WAREHOUSES.Any(excluded => command.Payload!.Name!.Contains(excluded)))
            {
                command.Reject(
                    rejectedBy: nameof(UpsertWarehouseFromM3BusinessValidator),
                    fieldName: nameof(Warehouse.Name),
                    fieldValue: command.Payload!.Name!,
                    $"the {nameof(Warehouse)} is excluded from import in this context");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
    }
}
