using Newtonsoft.Json;

namespace AutoRegister
{
    public class PrimitiveWrapperJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) =>
            Activator.CreateInstance(objectType, new[] { reader.Value });

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) =>
            objectType.GetCustomAttributes(true).OfType<PrimitiveWrapperAttribute>().Any();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PrimitiveWrapperAttribute : Attribute { }
}
