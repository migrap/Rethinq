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
            var json = JsonConvert.SerializeObject(query, new RqlQueryConverter(), new RqlTermConverter(), new WhereClauseConverter());
            var buffer = Encoding.UTF8.GetBytes(json);

            Globals.Connetion.SendAsync(buffer, 0, buffer.Length);
            return default(T);

        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty) {
            throw new NotImplementedException();
        }
    }
}