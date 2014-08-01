using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rethinq.Data.RqlClient {
    internal class RqlQuery {
        [ThreadStatic]
        private static Token _token;
        private Dictionary<string, object> _optional = new Dictionary<string, object>();

        internal static Token GetToken() {
            return _token ?? (_token = new Token((ushort)Thread.CurrentThread.ManagedThreadId));
        }

        public ulong Token { get; private set; }

        public Query Query { get; private set; }

        public RqlTerm Term { get; private set; }

        public Dictionary<string, object> Optional {
            get { return _optional; }
            set { _optional = value; }
        }

        public RqlQuery(RqlTerm term, Func<RqlQuery,Func<Query>> query) {
            Token = GetToken().Oxidize();
            Term = term;
            Query = query(this)();
        }
    }



    [DebuggerDisplay("{Name} - {Value}")]
    internal class Query :IEquatable<Query> {
        private int _value;
        private string _name;

        internal Query(string name, int value) {
            _name = name;
            _value = value;
        }

        public static implicit operator int (Query value) {
            return value._value;
        }

        public string Name {
            get { return _name; }
        }

        public int Value {
            get { return _value; }
        }

        public override string ToString() {
            return (new { Name, Value }).ToString();
        }

        public override bool Equals(object obj) {
            return !ReferenceEquals(obj as Query, null) && Equals(obj as Query);
        }

        public bool Equals(Query other) {
            if(null == other) {
                return false;
            }

            return _value == other.Value && _name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool Equals(Func<RqlQuery, Func<Query>> other) {
            return Equals(other((RqlQuery)null)());
        }
    }

    internal static class Querys {
        public static Query Start = new Query("START", 1);
        public static Query Continue = new Query("CONTINE", 2);
        public static Query Stop = new Query("STOP", 3);
        public static Query NoReplyWait = new Query("NOREPLY_WAIT", 4);
    }

    public static partial class Extensions {
        internal static Query Start(this RqlQuery self) {
            return Querys.Start;
        }

        internal static Query Continue(this RqlQuery self) {
            return Querys.Continue;
        }

        internal static Query Stop(this RqlQuery self) {
            return Querys.Stop;
        }

        internal static Query NoReplyWait(this RqlQuery self) {
            return Querys.NoReplyWait;
        }
    }
}
