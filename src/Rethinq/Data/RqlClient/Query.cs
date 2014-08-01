using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rethinq.Data.RqlClient {
    internal class Query {
        [ThreadStatic]
        private static Token _token;
        private Dictionary<string, object> _optional = new Dictionary<string, object>();

        internal static Token GetToken() {
            return _token ?? (_token = new Token((ushort)Thread.CurrentThread.ManagedThreadId));
        }

        public ulong Token { get; private set; }

        public QueryType QueryType { get; private set; }

        public Term Term { get; private set; }

        public Dictionary<string, object> Optional {
            get { return _optional; }
            set { _optional = value; }
        }

        public Query(Term term, Func<Query,Func<QueryType>> query) {
            Token = GetToken().Oxidize();
            Term = term;
            QueryType = query(this)();
        }
    }
}
