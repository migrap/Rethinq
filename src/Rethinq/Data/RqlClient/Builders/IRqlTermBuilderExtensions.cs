using System;

namespace Rethinq.Data.RqlClient.Builders {
    internal static partial class IRqlTermBuilderExtensions {
        internal static IRqlTermBuilder Term(this IRqlTermBuilder self, Func<Term, Func<TermType>> term) {
            return self.Term(term((Term)null)());
        }
    }
}