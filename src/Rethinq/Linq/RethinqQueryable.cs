using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;
using System.Linq;
using System.Linq.Expressions;

namespace Rethinq.Linq {
    internal class RethinqQueryable<T> : QueryableBase<T> {
        public RethinqQueryable(IQueryParser parser, IQueryExecutor executor)
            :base(new DefaultQueryProvider(typeof(RethinqQueryable<>), parser, executor)) {
        }

        public RethinqQueryable(IQueryProvider provider, Expression expression)
            : base(provider, expression) {
        }
    }
}
