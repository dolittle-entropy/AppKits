using Common.Models;
using MediatR;

namespace Common.Rejections
{
    public class Rejection : INotification, IEntity
    {
        public string? Id { get; init; }
        public bool Rejected { get; set; }
        public string? BrokerOffset { get; set; }
        public string? FromTenantId { get; set; }
        public string? BoundedContext { get; set; }
        public string? SpecificArea { get; set; }
        public string? RejectedBy { get; set; }
        public string? RejectionReason { get; set; }
        public object? FailingObject { get; set; }
        public bool? IsSynchronized { get; set; }

        public Rejection()
        {
            Id = Guid.NewGuid().ToString();
            Rejected = false;
            IsSynchronized = true;
        }
    }
}
