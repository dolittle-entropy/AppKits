using Newtonsoft.Json;

namespace Common.GraphQL;

public record FrontendNotification
{
    public FrontendNotificationType NotificationType { get; set; }

    public FrontendNotificationScope Scope { get; set; }

    public string? Recipient { get; set; }

    public string? Message { get; set; }

    public string? Payload { get; set; }

    public string? PayloadType { get; set; }
}

public static class FrontendNotificationExtensions
{
    public static FrontendNotification OfType(this FrontendNotification notification, FrontendNotificationType frontendNotificationType)
    {
        notification.NotificationType = frontendNotificationType;

        return notification;
    }

    public static FrontendNotification WithRecipient(this FrontendNotification notification, string recipient)
    {
        notification.Recipient = recipient;
        return notification;
    }

    public static FrontendNotification ScopedTo(this FrontendNotification notification, FrontendNotificationScope scope)
    {
        notification.Scope = scope;
        return notification;
    }

    public static FrontendNotification WithMessage(this FrontendNotification notification, string message)
    {
        notification.Message = message;
        return notification;
    }

    public static FrontendNotification WithUserId(this FrontendNotification notification, string userId)
    {
        notification.Scope = FrontendNotificationScope.User;
        notification.Recipient = userId;

        return notification;
    }

    public static FrontendNotification WithPayload<TEntity>(this FrontendNotification notification, TEntity payload)
    {
        notification.PayloadType = typeof(TEntity).Name;
        notification.Payload = JsonConvert.SerializeObject(payload);

        return notification;
    }
}

public enum FrontendNotificationScope
{
    Unknown, User, Everyone
}

public enum FrontendNotificationType
{
    Unknown, Confirmation, Warning, Error, CallToAction
}