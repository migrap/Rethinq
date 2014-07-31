using System;

namespace Rethinq.Data.RqlClient.Builders {
    internal static partial class IRqlTermBuilderExtensions {
        internal static IRqlTermBuilder Term(this IRqlTermBuilder self, Func<RqlTerm, Func<Term>> term) {
            return self.Term(term((RqlTerm)null)());
        }
    }
}