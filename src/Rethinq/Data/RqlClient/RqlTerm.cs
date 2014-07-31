using Newtonsoft.Json;
using Rethinq.Data.RqlClient.Builders;
using Rethinq.Data.RqlClient.Converters;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rethinq.Data.RqlClient {
    public static partial class Extensions {
        public static void Sandbox() {
            var database = new RqlTerm(_ => _
                .Term(x => x.Database)
                .Arguments("test")
            );

            var table = new RqlTerm(_ => _
                .Previous(database)
                .Term(x => x.Table)
                .Arguments("people")
            );

            var count = new RqlTerm(_ => _
                .Previous(table)
                .Term(x => x.Count)
            );

            //[43,[[15,[[14,[\"test\"]],\"people\"]]]]
            var json = JsonConvert.SerializeObject(count, new RqlTermConverter());
        }
    }

    internal class RqlTerm {
        private Term _term;
        private List<object> _arguments = new List<object>();
        private Dictionary<string, object> _optional = new Dictionary<string, object>();

        internal RqlTerm() {
        }

        internal RqlTerm(Action<IRqlTermBuilder> builder) {
            builder(new RqlTermBuilder(this));
        }

        internal RqlTerm(Func<RqlTerm, Func<Term>> term)
            : this(term: term(null)()) {
        }

        internal RqlTerm(RqlTerm previous = null, Func<RqlTerm, Func<Term>> term = null, params object[] arguments) 
            : this(previous: previous, term: term(previous)(), arguments: arguments) {
        }

        internal RqlTerm(RqlTerm previous = null, Func<RqlTerm, Func<Term>> term = null, IEnumerable arguments = null)
            : this(previous: previous, term: term(previous)(), arguments: arguments) {
        }

        internal RqlTerm(RqlTerm previous = null, Term term = null, IEnumerable arguments = null, IDictionary<string, object> optional = null) {
            _term = term;
            if (null != previous && null != previous.Term) {
                _arguments.Add(previous);
            }

            if (null != arguments) {
                foreach(var argument in arguments) {
                    _arguments.Add(argument);
                }                
            }

            if (null != optional) {
                foreach (var item in optional) {
                    _optional[item.Key] = item.Value;
                }
            }
        }

        public Term Term {
            get { return _term; }
            set { _term = value; }
        }

        public List<object> Arguments {
            get { return _arguments; }
            set { _arguments = value; }
        }

        public Dictionary<string, object> Optional {
            get { return _optional; }
            set { _optional = value; }
        }
    }
}
