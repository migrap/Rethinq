using System.Diagnostics;

namespace Rethinq.Data.RqlClient {
    [DebuggerDisplay("{Name} - {Value}")]
    public class TermType {
        private int _value;
        private string _name;

        internal TermType(string name, int value) {
            _name = name;
            _value = value;
        }

        public static implicit operator int (TermType value) {
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
