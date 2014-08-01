﻿using Newtonsoft.Json;
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
            var json = JsonConvert.SerializeObject(count, new RqlTermConverter());
        }
    }

    internal class Term {
        private TermType _term;
        private List<object> _arguments = new List<object>();
        private Dictionary<string, object> _optional = new Dictionary<string, object>();

        internal Term() {
        }

        internal Term(Action<IRqlTermBuilder> builder) {
            builder(new RqlTermBuilder(this));
        }

        internal Term(Func<Term, Func<TermType>> term)
            : this(term: term(null)()) {
        }

        internal Term(Term previous = null, Func<Term, Func<TermType>> term = null, params object[] arguments) 
            : this(previous: previous, term: term(previous)(), arguments: arguments) {
        }

        internal Term(Term previous = null, Func<Term, Func<TermType>> term = null, IEnumerable arguments = null)
            : this(previous: previous, term: term(previous)(), arguments: arguments) {
        }

        internal Term(Term previous = null, TermType term = null, IEnumerable arguments = null, IDictionary<string, object> optional = null) {
            _term = term;
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
