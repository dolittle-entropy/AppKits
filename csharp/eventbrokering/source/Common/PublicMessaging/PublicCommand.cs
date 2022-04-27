using Common.Models;
using System;

namespace Common.PublicMessaging;

public record PublicCommand
{
    public Metadata? Metadata { get; set; }

    public Guid TraceId { get; set; }

    public object? Payload { get; set; }
}