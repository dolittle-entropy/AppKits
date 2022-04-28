using Common.Models;

namespace Sample.Orders.Orders
{
    public record Order : IEntity
    {
        public Guid Id { get; init; }
        public DateTime Created { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime LastModified{ get; set; }        
        public bool? IsSynchronized { get; set; }
    }
}
