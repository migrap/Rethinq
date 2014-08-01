using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rethinq.Data.RqlClient.Converters {
    internal class RqlQueryConverter :JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(Query).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (null == value) {
                writer.WriteNull();
                return;
            }

            var query = value as Query;

            writer.WriteStartArray();
            writer.WriteValue(query.QueryType.Value);

            if (query.QueryType.Equals(x => x.Start)) {

                serializer.Serialize(writer, query.Term);

                if (query.Optional.Count > 0) {
                    serializer.Serialize(writer, query.Optional);
                }else {
                    writer.WriteStartObject();
                    writer.WriteEndObject();
                }

                //return;
            }

            writer.WriteEndArray();
            
        }

        public override bool CanRead {
            get { return false; }
        }

        public override bool CanWrite {
            get { return true; }
        }
    }
}
