using Newtonsoft.Json;
using System;
using System.Linq;

namespace Rethinq.Data.RqlClient.Converters {
    internal class RqlTermConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(RqlTerm).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (null == value) {
                writer.WriteNull();
                return;
            }

            var rql = value as RqlTerm;

            if (null == rql.Term) {
                serializer.Serialize(writer, rql.Arguments.First());
                return;
            }
            
            writer.WriteStartArray();

            writer.WriteValue((int)rql.Term);

            serializer.Serialize(writer, rql.Arguments);

            if (rql.Optional.Count > 0) {
                serializer.Serialize(writer, rql.Optional);
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