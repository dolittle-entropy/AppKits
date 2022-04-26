using Common.GraphQL;
using Common.Models;
using Common.Repositories;
using Serilog;

namespace Common.EventHandling
{
    /// <summary>
    /// Base class for EventHandlers that need the ability to perform notifications over GraphQL Subscriptions.
    /// Inherit this in EventHandlers that do not require Mutations, i.e. Facilities.
    /// Extends the base class <see cref="BaseEventHandler{TReadModel}"/>
    /// </summary>
    public class NotificationEventHandler<TReadModel> : BaseEventHandler<TReadModel>
        where TReadModel : class, IEntity, new()
    {
        readonly ILogger _log;
        readonly INotificationPublisher _notificationPublisher;

        public NotificationEventHandler(ILogger log, INotificationPublisher notificationPublisher, ICollectionService collectionService, string collectionName)
            : base(log, collectionService, collectionName)
        {
            _log = log.ForContext<NotificationEventHandler<TReadModel>>();
            _notificationPublisher = notificationPublisher;
        }

        protected async Task SendNotification(FrontendNotification notification, CancellationToken cancellationToken = default)
            => await _notificationPublisher.Publish(notification, cancellationToken).ConfigureAwait(false);

        protected async Task SendUserConfirmation<TEntity>(string userId, TEntity payload, string message)
        {
            var notification = new FrontendNotification()
                .OfType(FrontendNotificationType.Confirmation)
                .WithMessage(message)
                .WithUserId(userId)
                .WithPayload(payload);

            await _notificationPublisher.Publish(notification);
        }
        protected async Task SendCreateNotificationToFrontend<TEntity>(TEntity entity)
        {
            var notification = new FrontendNotification()
                .OfType(FrontendNotificationType.Confirmation)
                .WithMessage($"{typeof(TEntity).Name}Created ")
                .ScopedTo(FrontendNotificationScope.Everyone)                
                .WithPayload(entity);

            await _notificationPublisher.Publish(notification);
        }

        protected async Task SendErrorToUser<TEntity>(string userEmail, string errorContext, TEntity entity, string? reason)
        {
            var notification = new FrontendNotification()
                .WithUserId(userEmail)
                .OfType(FrontendNotificationType.Error)
                .ScopedTo(FrontendNotificationScope.User)
                .WithRecipient(userEmail)
                .WithMessage($"{errorContext}: {typeof(TEntity).Name} -- {reason}")
                .WithPayload(entity);

            await _notificationPublisher.Publish(notification);
        }

        protected async Task SendErrorToEveryone<TEntity>(TEntity entity, string errorDescription)
        {
            var notification = new FrontendNotification()
                .OfType(FrontendNotificationType.Error)
                .ScopedTo(FrontendNotificationScope.Everyone)
                .WithRecipient("Everyone")
                .WithMessage(errorDescription)
                .WithPayload(entity);

            await _notificationPublisher.Publish(notification);
        }

        protected async Task SendUpdateNotificationToFrontend<TEntity>(TEntity entity)
        {
            var notification = new FrontendNotification()
                .OfType(FrontendNotificationType.Confirmation)
                .ScopedTo(FrontendNotificationScope.Everyone)                
                .WithMessage($"{typeof(TEntity).Name}Updated ")                
                .WithPayload(entity);

            await _notificationPublisher.Publish(notification);
        }

        protected async Task SendDeleteNotificationToFrontend<TEntity>(TEntity entity)
        {
            var notification = new FrontendNotification()
                .OfType(FrontendNotificationType.Confirmation)
                .ScopedTo(FrontendNotificationScope.Everyone)                
                .WithMessage($"{typeof(TEntity).Name}Deleted")                
                .WithPayload(entity);

            await _notificationPublisher.Publish(notification);
        }
    }
}
