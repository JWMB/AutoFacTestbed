using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoRegister
{
    public class ServiceConfigJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => ServiceConfigBase.GetTypeIsServiceConfig(objectType);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var obj = serializer.Deserialize<JObject>(reader);
            if (obj != null)
            {
                var typeNameToken = obj[ServiceConfigBase.TypePropertyName];
                if (typeNameToken != null)
                {
                    var typeName = typeNameToken.Value<string>();
                    obj.Remove(ServiceConfigBase.TypePropertyName);
                    var instance = Activator.CreateInstance(objectType, typeName, obj);
                    return instance;
                }
                else // backwards compatibility
                {
                    var typeNameToken2 = obj["Name"];
                    if (typeNameToken2 != null)
                    {
                        var typeName = typeNameToken2.Value<string>();
                        obj.Remove("Name");
                        var instance = Activator.CreateInstance(objectType, typeName, obj["Config"]);
                        return instance;
                    }
                }
            }
            return null;
        }

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
