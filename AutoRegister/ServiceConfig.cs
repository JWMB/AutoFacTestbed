using Newtonsoft.Json.Linq;
using System.Reflection;

namespace AutoRegister
{
    public abstract class ServiceConfigBase : JObject
    {
        public const string TypePropertyName = "$type";

        public ServiceConfigBase() { }
        public ServiceConfigBase(string typeName, object? config = null)
        {
            this[TypePropertyName] = typeName;
            if (config != null)
                Merge(JObject.FromObject(config));
        }

        public ServiceConfigBase(Type type, object? config = null)
        {
            this[TypePropertyName] = type.Name;
            if (config != null)
                Merge(JObject.FromObject(config));
        }

        public bool HasType => TypeName != null;
        public string? TypeName => this[TypePropertyName]?.Value<string>();

        public Type? TargetType => TypeName == null ? null : TypeNameCache.FindByName(TypeName);

        public Type? GetConfigType()
        {
            var type = TargetType;
            if (type == null)
                return null;
            var configType = GetConfigTypeForServiceType(type);
            return configType;
        }
        public object? GetConfig()
        {
            var configType = GetConfigType();
            return configType == null ? null : ToObject(configType);
        }

        public TConfig? GetTypedConfig<TConfig>() where TConfig : class
        {
            var config = GetConfig();
            return config == null ? null : (TConfig)config;
        }

        //public object? GetInstance(IServiceProvider? serviceProvider)
        //{
        //    if (serviceProvider == null || TargetType == null)
        //        return null;
        //    return serviceProvider.GetService(TargetType);
        //}

        public static bool GetTypeIsServiceConfig(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ServiceConfig<>);

        private static bool GetIsConfigParameter(ParameterInfo param) =>
            param.GetCustomAttribute<ServiceConfigParameterAttribute>() != null;

        private static Type? GetConfigTypeForServiceType(Type type)
        {
            var constructorInfo = type.GetConstructors()
                .Select(p => new { Constr = p, ConfigParams = p.GetParameters().Where(GetIsConfigParameter).ToList() })
                .Where(o => o.ConfigParams.Any())
                .FirstOrDefault();
            return constructorInfo == null ? null : constructorInfo.ConfigParams.First().ParameterType;
        }

    }

    public class ServiceConfig<T> : ServiceConfigBase where T : class // JObject 
    {
        public ServiceConfig() { }

        public ServiceConfig(string typeName, object? config = null) : base(typeName, config) { }

        public ServiceConfig(Type type, object? config = null) : base(type, config) { }

        //public T? GetTypedInstance(IServiceProvider? serviceProvider)
        //{
        //    if (serviceProvider == null || TargetType == null)
        //        return null;
        //    // TODO: type check should be done earlier
        //    if (typeof(T).IsAssignableFrom(TargetType) == false)
        //        return null; // throw?
        //    return (T?)GetInstance(serviceProvider);
        //}
    }
}
