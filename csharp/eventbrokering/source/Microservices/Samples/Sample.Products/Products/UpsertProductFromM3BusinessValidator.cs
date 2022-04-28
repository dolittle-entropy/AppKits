using Common.Extensions;
using Common.Validation;

namespace Sample.Products.Products
{
    public class UpsertProductFromM3BusinessValidator : IBusinessValidator<UpsertProductFromM3>
    {
        static readonly List<string> IGNORED_SKUS = new List<string>{ "AP", "DP", "PH", "YT" };

        public async Task<bool> Validate(UpsertProductFromM3 command)
        {
            await Task.CompletedTask;
            if (IGNORED_SKUS.Any(ignored => command.Payload!.SKU!.StartsWith(ignored)))
            {
                command.Reject(
                    rejectedBy: nameof(UpsertProductFromM3BusinessValidator),
                    fieldName: nameof(UpsertProductFromM3.Payload.SKU),
                    fieldValue: nameof(UpsertProductFromM3.Payload.SKU),
                    customMessage: "Product belongs to an ignored SKU category");
            }
            return true;
        }        
    }
}
