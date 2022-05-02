using HotChocolate.Stitching.Merge;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQlGateway.Stitching
{
    /// <summary>
    /// <para>This merge handler deals with duplicate directives when schemas are stitched.</para>
    /// <para>It currently handles dupes of: authorize.</para>
    /// </summary>
    public class MergeDirectivesHandler : IDirectiveMergeHandler
    {
        private readonly MergeDirectiveRuleDelegate _next;

        public MergeDirectivesHandler(MergeDirectiveRuleDelegate next) => _next = next;

        public void Merge(ISchemaMergeContext context, IReadOnlyList<IDirectiveTypeInfo> directives)
        {
            // If you're using HC Authorization on multiple downstream servers
            if (!directives.First().Definition.Name.Value.Equals("authorize", StringComparison.InvariantCultureIgnoreCase))
            {
                _next(context, directives);
            }
        }
    }
}
