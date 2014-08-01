namespace Rethinq.Data.RqlClient {
    internal static partial class TermExtensions {
        internal static TermType Dataum(this Term self) {
            return Terms.Datum;
        }

        internal static TermType Database(this Term self) {
            return Terms.Database;
        }

        internal static TermType Table(this Term self) {
            return Terms.Table;
        }

        internal static TermType Count(this Term self) {
            return Terms.Count;
        }

        internal static TermType Filter(this Term self) {
            return Terms.Filter;
        }

        internal static TermType Function(this Term self) {
            return Terms.Function;
        }

        internal static TermType MakeArray(this Term self) {
            return Terms.MakeArray;
        }

        internal static TermType Var(this Term self) {
            return Terms.Var;
        }

        internal static TermType Equals(this Term self) {
            return Terms.Equals;
        }

        internal static TermType GetField(this Term self) {
            return Terms.GetField;
        }
    }
}
