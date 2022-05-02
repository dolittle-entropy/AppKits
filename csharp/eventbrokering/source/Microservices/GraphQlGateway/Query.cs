using HotChocolate;
using System.Security.Claims;

namespace GraphQlGateway
{
    [GraphQLName(nameof(Query)), GraphQLDescription("Direct Queries to the Proxy Service")]
    public class Query
    {
        public View GetView(ClaimsPrincipal claimsPrincipal)
            => new View
            {
                Version = "EventBrokering Sample v1.0",
            };
    }
}
