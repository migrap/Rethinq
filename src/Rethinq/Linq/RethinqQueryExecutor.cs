using Newtonsoft.Json;
using Remotion.Linq;
using Rethinq.Data.RqlClient.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using Rethinq.Data.RqlClient;

namespace Rethinq.Linq {
    internal class RethinqQueryExecutor : IQueryExecutor {
        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel) {
            throw new NotImplementedException();
        }

        public T ExecuteScalar<T>(QueryModel queryModel) {
            var query = RethinqQueryGenerator.GenerateQuery(queryModel);
            var json = JsonConvert.SerializeObject(query, new QueryConverter(), new TermConverter(), new WhereClauseConverter());
            var buffer = Encoding.UTF8.GetBytes(json);


            var length = BitConverter.GetBytes(buffer.Length).ToLittleEndian();
            var token = BitConverter.GetBytes(query.Token).ToLittleEndian();

            Globals.Connetion.SendAsync(token, 0, token.Length);
            Globals.Connetion.SendAsync(length, 0, length.Length);
            Globals.Connetion.SendAsync(buffer, 0, buffer.Length);
            return default(T);

        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty) {
            throw new NotImplementedException();
        }
    }
}