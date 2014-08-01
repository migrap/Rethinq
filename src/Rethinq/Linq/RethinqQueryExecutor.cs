using Newtonsoft.Json;
using Remotion.Linq;
using Rethinq.Data.RqlClient.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rethinq.Linq {
    internal class RethinqQueryExecutor : IQueryExecutor {
        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel) {
            throw new NotImplementedException();
        }

        public T ExecuteScalar<T>(QueryModel queryModel) {
            var query = RethinqQueryGenerator.GenerateQuery(queryModel);
            var json = JsonConvert.SerializeObject(query, new RqlTermConverter(), new WhereClauseConverter());
            //[COUNT,[[FILTER,[[TABLE,[[DB,[test]],"people"]],[FUNC,[[MAKE_ARRAY,[MAKE_ARRAY]],[EQ,[[GET_FIELD,[[VAR,[MAKE_ARRAY]],"Id"]],0]]]]]]]]
            //[43,[[39,[[15,[[14,[\"test\"]],\"people\"]],[69,[[2,[2]],[17,[[31,[[10,[2]],\"Id\"]],0]]]]]]]]
            //[43,[[39,[[15,[[14,[\"test\"]],\"people\"]]]]]]
            var buffer = Encoding.UTF8.GetBytes(json);

            Globals.Connetion.SendAsync(buffer, 0, buffer.Length);
            return default(T);

        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty) {
            throw new NotImplementedException();
        }
    }
}