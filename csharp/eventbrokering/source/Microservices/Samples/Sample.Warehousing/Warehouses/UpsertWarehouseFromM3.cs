using Common.Commands;
using Common.Rejections;

namespace Sample.Warehousing.Warehouses
{
    public class UpsertWarehouseFromM3 : Rejection, ICommand<Warehouse>
    {
        public Guid TenantId { get; set; }

        public string? IssuedBy { get; set; }

        public Warehouse? Payload { get; set; }
    }
}
