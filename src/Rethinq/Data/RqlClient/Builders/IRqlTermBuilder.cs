using System.Collections.Generic;

namespace Rethinq.Data.RqlClient.Builders {
    internal interface IRqlTermBuilder {
        IRqlTermBuilder Term(TermType value);
        IRqlTermBuilder Arguments(params object[] collection);
        IRqlTermBuilder Optional(IDictionary<string, object> collection);
        IRqlTermBuilder Previous(Term value);
    }
}
