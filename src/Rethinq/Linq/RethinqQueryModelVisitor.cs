using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Rethinq.Data.RqlClient;
using Rethinq.Data.RqlClient.Builders;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Rethinq.Linq {
    internal class RethinqQueryModelVisitor : QueryModelVisitorBase {
        private Term _term = new Term(_ => _
            .Term(x => x.Database)
            .Arguments("test")
        );

        public Query GetQuery() {
            return new Query(_term, x => x.Start);
        }

        public override void VisitQueryModel(QueryModel queryModel) {
            //queryModel.SelectClause.Accept(this, queryModel);
            queryModel.MainFromClause.Accept(this, queryModel);

            VisitBodyClauses(queryModel.BodyClauses, queryModel);
            VisitResultOperators(queryModel.ResultOperators, queryModel);
        }

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel) {
            var table = GetTable(fromClause.ItemType).ToLower();
            _term = new Term(_ => _
                .Previous(_term)
                .Term(x => x.Table)
                .Arguments(table)
            );

            base.VisitMainFromClause(fromClause, queryModel);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index) {
            if(resultOperator is CountResultOperator) {
                _term = new Term(_term, x => x.Count);
            }
            base.VisitResultOperator(resultOperator, queryModel, index);
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index) {
            _term = new Term(_ => _
                .Previous(_term)
                .Term(x => x.Filter)
                .Arguments(whereClause)
            );
        }

        protected override void VisitBodyClauses(ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel) {
            base.VisitBodyClauses(bodyClauses, queryModel);
        }

        private static string GetTable(Type type) {
            return type.Name;
        }
    }
}