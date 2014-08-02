using Remotion.Linq;
using Rethinq.Data.RqlClient;
using System;

namespace Rethinq.Linq {
    internal static class RethinqQueryGenerator {
        public static Query GenerateQuery(QueryModel queryModel) {
            var visitor = new RethinqQueryModelVisitor();
            visitor.VisitQueryModel(queryModel);
            //return visitor.GetQuery();
            throw new NotImplementedException();
        }
    }
}
