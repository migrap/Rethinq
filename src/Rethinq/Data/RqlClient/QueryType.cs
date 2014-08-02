using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rethinq.Data.RqlClient {
    [DebuggerDisplay("{Name} - {Value}")]
    public class QueryType : IEquatable<QueryType> {
        private int _value;
        private string _name;

        internal QueryType(string name, int value) {
            _name = name;
            _value = value;
        }

        public static implicit operator int (QueryType value) {
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
            return !ReferenceEquals(obj as QueryType, null) && Equals(obj as QueryType);
        }

        public bool Equals(QueryType other) {
            if(null == other) {
                return false;
            }

            return _value == other.Value && _name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool Equals(Func<Query, Func<QueryType>> other) {
            return Equals(other((Query)null)());
        }
    }
}
