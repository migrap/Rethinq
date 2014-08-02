using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rethinq.Data.RqlClient {
    public class Query {
        
        private Dictionary<string, object> _optional = new Dictionary<string, object>();

        public Query(ulong token, Term term, Func<Query, Func<QueryType>> querytype) {
            Token = token;
            Term = term;
            QueryType = querytype(this)();
        }

        public ulong Token { get; private set; }

        public QueryType QueryType { get; private set; }

        public Term Term { get; private set; }

        public Dictionary<string, object> Optional {
            get { return _optional; }
            set { _optional = value; }
        }
    }
}
