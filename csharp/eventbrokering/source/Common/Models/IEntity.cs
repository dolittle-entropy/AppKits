namespace Common.Models
{
    /// <summary>
    /// A business entity, something named that represents a concept in need of public Processing (and synchronization)
    /// </summary>
    public interface IEntity
    {
        bool? IsSynchronized { get; set; }
    }
}
