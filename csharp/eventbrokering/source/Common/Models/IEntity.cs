namespace Common.Models
{
    /// <summary>
    /// A business entity, something named that represents a concept in need of public Processing (and synchronization)
    /// </summary>
    public interface IEntity
    {
        public string? CreatedBy { get; set; }
        public DateTime Created { get; set; }

        public string? LastModifiedBy { get; set; }
        public DateTime LastModified { get; set; }

        bool? IsSynchronized { get; set; }
    }
}
