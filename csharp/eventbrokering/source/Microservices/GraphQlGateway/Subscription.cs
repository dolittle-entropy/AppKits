using Common.GraphQL;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;

namespace GraphQlGateway
{
    public class Subscription
    {
        [Subscribe]
        [UseFiltering]
        public FrontendNotification NotificationFromBackend([EventMessage] FrontendNotification notification) => notification;
    }
}