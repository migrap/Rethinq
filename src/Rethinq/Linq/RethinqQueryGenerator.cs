using Remotion.Linq;
using Rethinq.Data.RqlClient;

namespace Rethinq.Linq {
    internal static class RethinqQueryGenerator {
        public static RqlTerm GenerateQuery(QueryModel queryModel) {
            var visitor = new RethinqQueryModelVisitor();
            visitor.VisitQueryModel(queryModel);
            return visitor.GetQuery();
        }
    }
}
