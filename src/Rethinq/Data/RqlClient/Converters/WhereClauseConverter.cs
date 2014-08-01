using Newtonsoft.Json;
using Remotion.Linq.Clauses;
using System;
using System.Linq.Expressions;

namespace Rethinq.Data.RqlClient.Converters {
    internal class WhereClauseConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(WhereClause).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if(null == value) {
                writer.WriteNull();
                return;
            }

            var where = value as WhereClause;
            var expression = where.Predicate;
            var function = VisitExpression(expression);

            serializer.Serialize(writer, function);
        }

        public override bool CanRead {
            get { return false; }
        }

        public override bool CanWrite {
            get { return true; }
        }

        private Term VisitExpression(Expression expression) {
            var function = new Term(x => x.Function);
            var parameters = new Term(x => x.MakeArray);

            parameters.Arguments.Add(2);
            function.Arguments.Add(parameters);

            var where = WhereExpressionVisitor.Walk(expression);
            function.Arguments.Add(where);
            return function;
        }
    }

    internal class WhereExpressionVisitor : ExpressionVisitor {
        private Term _term;

        public static Term Walk(Expression expression) {
            if(expression.NodeType == ExpressionType.Constant) {
                var term = new Term() {
                    Arguments = { (expression as ConstantExpression).Value }
                };
                return term;
            }

            var visitor = new WhereExpressionVisitor();
            visitor.Visit(expression);
            return visitor._term;
        }        

        protected override Expression VisitConstant(ConstantExpression node) {
            var term = new Term() {
                Arguments = { node.Value }
            };

            _term.Arguments.Add(term);

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            switch (node.NodeType) {
                case ExpressionType.Equal:
                    _term = ConvertBinaryExpressionToRqlTerm(node, x => x.Equals);
                    break;
            }
            return node;        
        }

        private Term ConvertBinaryExpressionToRqlTerm(BinaryExpression expression, Func<Term, Func<TermType>> term) {
            _term = new Term(term);
            _term.Arguments.Add(Walk(expression.Left));
            _term.Arguments.Add(Walk(expression.Right));            
            return _term;
        }

        protected override Expression VisitMember(MemberExpression node) {
            _term = new Term(x => x.GetField);
            _term.Arguments.Add(new Term(x => x.Var) {
                Arguments = { 2 }
            });

            var name = node.Member.Name;
            _term.Arguments.Add(name);

            return node;
        }
    }
}
