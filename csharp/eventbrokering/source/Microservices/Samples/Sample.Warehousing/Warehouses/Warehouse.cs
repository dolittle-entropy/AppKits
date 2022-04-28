using Common.Models;

namespace Sample.Warehousing.Warehouses
{
    public record Warehouse : IEntity
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime LastModified { get; set; }
        public bool? IsSynchronized { get; set; } = false;
    }
}
