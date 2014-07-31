namespace Rethinq.Data.RqlClient {
    internal static partial class Terms {
        public static readonly Term Datum = new Term("DATUM", 1);
        public static readonly Term MakeArray = new Term("MAKE_ARRAY", 2);
        public static readonly Term Database = new Term("DB", 14);
        public static readonly Term Table = new Term("TABLE", 15);
        public static readonly Term Count = new Term("COUNT", 43);
        public static readonly Term Filter = new Term("FILTER", 39);
        public static readonly Term Function = new Term("FUNC", 69);
        public static readonly Term Var = new Term("VAR", 10);
        public static readonly Term Equals = new Term("EQ", 17);
        public static readonly Term GetField = new Term("GET_FIELD", 31);
    }
}
