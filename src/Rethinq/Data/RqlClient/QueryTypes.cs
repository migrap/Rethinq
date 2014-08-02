﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rethinq.Data.RqlClient {
    public static class QueryTypes {
        public static QueryType Start = new QueryType("START", 1);
        public static QueryType Continue = new QueryType("CONTINE", 2);
        public static QueryType Stop = new QueryType("STOP", 3);
        public static QueryType NoReplyWait = new QueryType("NOREPLY_WAIT", 4);
    }
}
