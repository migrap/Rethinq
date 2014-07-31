using System.Collections.Generic;

namespace Rethinq.Data.RqlClient.Builders {
    internal interface IRqlTermBuilder {
        IRqlTermBuilder Term(Term value);
        IRqlTermBuilder Arguments(params object[] collection);
        IRqlTermBuilder Optional(IDictionary<string, object> collection);
        IRqlTermBuilder Previous(RqlTerm value);
    }
}
