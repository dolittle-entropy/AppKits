using HotChocolate.Stitching.Merge;
using System.Collections.Generic;
using System.Linq;

namespace GraphQlGateway.Stitching
{
    public class NsgMergeTypesHandler : ITypeMergeHandler
    {
        private readonly MergeTypeRuleDelegate _next;
        public NsgMergeTypesHandler(MergeTypeRuleDelegate next) => _next = next;

        private readonly string[] _dupeTypes = new[]
        {
            // If you're using Relay/GlobalIdentification on multiple downstream servers or have added NodeType to the gateway too
            "Node",
            // If you're using HC Authorization on multiple downstream servers or have added Authorization to the gateway too
            "ApplyPolicy",

            "MCO",

            "FrontendNotification",
            "FrontendNotificationScope",
            "FrontendNotificationType",
            "FrontendNotificationFilterInput",
            "FrontendNotificationTypeOperationFilterInput",
            "FrontendNotificationScopeOperationFilterInput",
            "ListFilterInputTypeOfTeamOperationLineFilterInput",
            "TeamOperationLineFilterInput",
            "StringOperationFilterInput",
            "TeamFilterInput",
            "ListStringOperationFilterInput",
        };

        public void Merge(ISchemaMergeContext context, IReadOnlyList<ITypeInfo> types)
        {
            // I believe this skips all downstream versions of the type,
            // leaving only the type added directly onto the gateway schema.
            // So you'll need `.AddType<Node>()` and `.AddAuthorization()` on the gateway
            // schema builder
            if (_dupeTypes.Contains(types.First().Definition.Name.Value))
            {
                return;
            }

            // Standard merge behaviour
            _next(context, types);
        }
    }
}
