using System.Collections.Generic;

namespace Rethinq.Data.RqlClient.Builders {
    internal class RqlTermBuilder : IRqlTermBuilder {
        private RqlTerm _term;

        public RqlTermBuilder(RqlTerm term) {
            _term = term;
        }

        IRqlTermBuilder IRqlTermBuilder.Arguments(params object[] collection) {
            foreach (var item in collection) {
                _term.Arguments.Add(item);
            }
            return this;
        }

        IRqlTermBuilder IRqlTermBuilder.Optional(IDictionary<string, object> collection) {
            foreach (var item in collection) {
                _term.Optional.Add(item.Key, item.Value);
            }
            return this;
        }

        IRqlTermBuilder IRqlTermBuilder.Term(Term value) {
            _term.Term = value;
            return this;
        }

        IRqlTermBuilder IRqlTermBuilder.Previous(RqlTerm value) {
            _term.Arguments.Add(value);
            return this;
        }
    }
}
