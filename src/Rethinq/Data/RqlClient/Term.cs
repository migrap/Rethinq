using System.Diagnostics;

namespace Rethinq.Data.RqlClient {
    [DebuggerDisplay("{Name} - {Value}")]
    internal class Term {
        private int _value;
        private string _name;

        internal Term(string name, int value) {
            _name = name;
            _value = value;
        }

        public static implicit operator int (Term value) {
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
    }
}
