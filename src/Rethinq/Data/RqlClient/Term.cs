using Newtonsoft.Json;
using Rethinq.Data.RqlClient.Builders;
using Rethinq.Data.RqlClient.Converters;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rethinq.Data.RqlClient {
    public static partial class Extensions {
        public static void Sandbox() {
            var database = new Term(_ => _
                .Term(x => x.Database)
                .Arguments("test")
            );

            var table = new Term(_ => _
                .Previous(database)
                .Term(x => x.Table)
                .Arguments("people")
            );

            var count = new Term(_ => _
                .Previous(table)
                .Term(x => x.Count)
            );

            //[43,[[15,[[14,[\"test\"]],\"people\"]]]]
            var json = JsonConvert.SerializeObject(count, new TermConverter());
        }
    }

    internal class Term {
        private TermType _termtype;
        private List<object> _arguments = new List<object>();
        private Dictionary<string, object> _optional = new Dictionary<string, object>();

        internal Term() {
        }

        internal Term(Action<IRqlTermBuilder> builder) {
            builder(new RqlTermBuilder(this));
        }

        internal Term(Func<Term, Func<TermType>> termtype)
            : this(termtype: termtype(null)()) {
        }

        internal Term(Term previous = null, Func<Term, Func<TermType>> termtype = null, params object[] arguments) 
            : this(previous: previous, termtype: termtype(previous)(), arguments: arguments) {
        }

        internal Term(Term previous = null, Func<Term, Func<TermType>> termtype = null, IEnumerable arguments = null)
            : this(previous: previous, termtype: termtype(previous)(), arguments: arguments) {
        }

        internal Term(Term previous = null, TermType termtype = null, IEnumerable arguments = null, IDictionary<string, object> optional = null) {
            _termtype = termtype;

            if (null != previous && null != previous.TermType) {
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

        public TermType TermType {
            get { return _termtype; }
            set { _termtype = value; }
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
