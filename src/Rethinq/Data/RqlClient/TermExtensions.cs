namespace Rethinq.Data.RqlClient {
    internal static partial class TermExtensions {
        internal static TermType Dataum(this Term self) {
            return TermTypes.Datum;
        }

        internal static TermType Database(this Term self) {
            return TermTypes.Database;
        }

        internal static TermType Table(this Term self) {
            return TermTypes.Table;
        }

        internal static TermType Count(this Term self) {
            return TermTypes.Count;
        }

        internal static TermType Filter(this Term self) {
            return TermTypes.Filter;
        }

        internal static TermType Function(this Term self) {
            return TermTypes.Function;
        }

        internal static TermType MakeArray(this Term self) {
            return TermTypes.MakeArray;
        }

        internal static TermType Var(this Term self) {
            return TermTypes.Var;
        }

        internal static TermType Equals(this Term self) {
            return TermTypes.Equals;
        }

        internal static TermType GetField(this Term self) {
            return TermTypes.GetField;
        }
    }
}
