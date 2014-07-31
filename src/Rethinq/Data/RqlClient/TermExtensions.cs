namespace Rethinq.Data.RqlClient {
    internal static partial class TermExtensions {
        internal static Term Dataum(this RqlTerm self) {
            return Terms.Datum;
        }

        internal static Term Database(this RqlTerm self) {
            return Terms.Database;
        }

        internal static Term Table(this RqlTerm self) {
            return Terms.Table;
        }

        internal static Term Count(this RqlTerm self) {
            return Terms.Count;
        }

        internal static Term Filter(this RqlTerm self) {
            return Terms.Filter;
        }

        internal static Term Function(this RqlTerm self) {
            return Terms.Function;
        }

        internal static Term MakeArray(this RqlTerm self) {
            return Terms.MakeArray;
        }

        internal static Term Var(this RqlTerm self) {
            return Terms.Var;
        }

        internal static Term Equals(this RqlTerm self) {
            return Terms.Equals;
        }

        internal static Term GetField(this RqlTerm self) {
            return Terms.GetField;
        }
    }
}
