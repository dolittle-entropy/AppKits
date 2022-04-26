using System;

namespace Common.EventHandling
{
    public class ValueChangeEvent<TId, TValue>
    {
        public ValueChangeEvent(TId id, TValue oldValue, TValue newValue, string? issuedBy)
        {
            Id = id;
            OldValue = oldValue;
            NewValue = newValue;
            IssuedBy = issuedBy ?? throw new ArgumentNullException(nameof(issuedBy));
        }
        public TId Id { get; set; }
        public TValue OldValue { get; set; }
        public TValue NewValue { get; set; }
        public string? IssuedBy { get; set; }
    }
}
