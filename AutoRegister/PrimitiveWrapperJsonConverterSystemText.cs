using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoRegister
{
    public class PrimitiveWrapperJsonConverterSystemText<T> : JsonConverter<PrimitiveWrapperBase<T>> where T : notnull, IComparable<T>
    {
        public override PrimitiveWrapperBase<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString(); // TODO: depending on T
            var instance = Activator.CreateInstance(typeToConvert, new[] { str });
            if (instance == null)
                throw new Exception("!");
            var typed = instance as PrimitiveWrapperBase<T>;
            if (typed == null)
                throw new Exception("!");
            return typed;
        }

        public override void Write(Utf8JsonWriter writer, PrimitiveWrapperBase<T> value, JsonSerializerOptions options) =>
            throw new NotImplementedException();

        public override bool CanConvert(Type objectType) =>
            objectType.GetCustomAttributes(true).OfType<PrimitiveWrapperAttribute>().Any();
    }
}
