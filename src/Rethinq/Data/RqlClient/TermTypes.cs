namespace Rethinq.Data.RqlClient {
    internal static partial class TermTypes {
        public static readonly TermType Datum = new TermType("DATUM", 1);
        public static readonly TermType MakeArray = new TermType("MAKE_ARRAY", 2);
        public static readonly TermType Database = new TermType("DB", 14);
        public static readonly TermType Table = new TermType("TABLE", 15);
        public static readonly TermType Count = new TermType("COUNT", 43);
        public static readonly TermType Filter = new TermType("FILTER", 39);
        public static readonly TermType Function = new TermType("FUNC", 69);
        public static readonly TermType Var = new TermType("VAR", 10);
        public static readonly TermType Equals = new TermType("EQ", 17);
        public static readonly TermType GetField = new TermType("GET_FIELD", 31);
    }
}
