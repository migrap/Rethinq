using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rethinq.Data.RqlClient {
    public static partial class Extensions {
        internal static QueryType Start(this Query self) {
            return Querys.Start;
        }

        internal static QueryType Continue(this Query self) {
            return Querys.Continue;
        }

        internal static QueryType Stop(this Query self) {
            return Querys.Stop;
        }

        internal static QueryType NoReplyWait(this Query self) {
            return Querys.NoReplyWait;
        }
    }
}
