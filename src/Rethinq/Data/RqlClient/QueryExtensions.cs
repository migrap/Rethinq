using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rethinq.Data.RqlClient {
    public static partial class Extensions {
        internal static QueryType Start(this Query self) {
            return QueryTypes.Start;
        }

        internal static QueryType Continue(this Query self) {
            return QueryTypes.Continue;
        }

        internal static QueryType Stop(this Query self) {
            return QueryTypes.Stop;
        }

        internal static QueryType NoReplyWait(this Query self) {
            return QueryTypes.NoReplyWait;
        }
    }
}
