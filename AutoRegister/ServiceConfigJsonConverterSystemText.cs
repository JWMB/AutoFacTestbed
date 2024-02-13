using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AutoRegister
{
    public class ServiceConfigJsonConverterSystemText : JsonConverter<ServiceConfigBase>
    {
        public override ServiceConfigBase Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var doc = JsonDocument.ParseValue(ref reader);
            if (doc == null)
                throw new Exception("!");

            var obj = JsonObject.Create(doc.RootElement);
            if (obj == null)
                throw new Exception("!");

            object? instance = null;

            var typeNameToken = obj[ServiceConfigBase.TypePropertyName];
            if (typeNameToken != null)
            {
                var typeName = typeNameToken.GetValue<string>();
                obj.Remove(ServiceConfigBase.TypePropertyName);
                // ServiceConfigBase base class is Newtonsoft JObject...
                var json = JsonSerializer.Serialize(obj);
                var jObj = Newtonsoft.Json.Linq.JObject.Parse(json);
                instance = Activator.CreateInstance(typeToConvert, typeName, jObj);
            }
            else // backwards compatibility
            {
                var typeNameToken2 = obj["Name"];
                if (typeNameToken2 != null)
                {
                    var typeName = typeNameToken2.GetValue<string>();
                    obj.Remove("Name");
                    instance = Activator.CreateInstance(typeToConvert, typeName, obj["Config"]);
                }
                else
                {
                    throw new Exception("!");
                }
            }
            if (instance == null)
                throw new Exception("!");
            if (instance is ServiceConfigBase typed)
                return typed;

            throw new Exception("!");
        }

        public override void Write(Utf8JsonWriter writer, ServiceConfigBase value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
            //writer.WriteStringValue(Decimal.Round(value, 3).ToString());
        }

        public override bool CanConvert(Type typeToConvert) => ServiceConfigBase.GetTypeIsServiceConfig(typeToConvert);
    }
}
