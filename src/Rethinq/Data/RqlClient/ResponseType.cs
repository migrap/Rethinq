using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rethinq.Data.RqlClient {

    //[DebuggerDisplay("{Name} - {Value}")]
    //internal class ResponseType : IEquatable<ResponseType> {
    //    private int _value;
    //    private string _name;

    //    internal ResponseType(string name, int value) {
    //        _name = name;
    //        _value = value;
    //    }

    //    public static implicit operator int (ResponseType value) {
    //        return value._value;
    //    }

    //    public string Name {
    //        get { return _name; }
    //    }

    //    public int Value {
    //        get { return _value; }
    //    }

    //    public override string ToString() {
    //        return (new { Name, Value }).ToString();
    //    }

    //    public override bool Equals(object obj) {
    //        return !ReferenceEquals(obj as ResponseType, null) && Equals(obj as ResponseType);
    //    }

    //    public bool Equals(ResponseType other) {
    //        if (null == other) {
    //            return false;
    //        }

    //        return _value == other.Value && _name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase);
    //    }

    //    public bool Equals(Func<Response, Func<ResponseType>> other) {
    //        return Equals(other((Response)null)());
    //    }
    //}
    public enum ResponseType {
        SUCCESS_ATOM = 1,
        SUCCESS_SEQUENCE = 2,
        SUCCESS_PARTIAL = 3,
        SUCCESS_FEED = 5,
        WAIT_COMPLETE = 4,
        CLIENT_ERROR = 16,
        COMPILE_ERROR = 17,
        RUNTIME_ERROR = 18
    }
}