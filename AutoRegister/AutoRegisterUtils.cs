using Autofac;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace AutoRegister
{
    public class AutoRegisterUtils
    {
        public static void RegisterServiceConfigTargets(object? obj, IServiceProvider serviceProvider, ContainerBuilder builder)
        {
            if (obj == null)
                return;
            var toRegister = RecurseInstantiateServiceConfigTargets(obj, serviceProvider).ToList();
            foreach (var item in toRegister)
            {
                builder.Register(c => item.Instance).As(item.Type);
            }
        }

        public static IEnumerable<(Type Type, object Instance)> RecurseGetAutoregisterItems(object obj)
        {
            var found = new List<(Type, object)>();
            var detectors = new[] { GetIfServiceConfig, GetIfRegisterAttribute };

            Traverse(obj, null, (o, prop) =>
            {
                var continue_ = true;
                foreach (var item in detectors)
                    continue_ &= item(o, prop);
                return continue_;
            });

            return found;

            bool GetIfServiceConfig(object o, PropertyInfo? prop)
            {
                if (o is ServiceConfigBase confObj)
                {
                    if (o.GetType().IsGenericType)
                    {
                        var conf = confObj.GetConfig();
                        if (conf != null && confObj.GetConfigType() != null)
                            found.Add((confObj.GetConfigType()!, conf));
                    }
                    return false;
                }
                return true;
            }

            bool GetIfRegisterAttribute(object o, PropertyInfo? prop)
            {
                var attr = prop?.GetCustomAttribute<RegisterOnRequestContextAttribute>();
                if (attr != null)
                    found.Add((prop!.PropertyType, o));
                return true;
            }
        }

        private static IEnumerable<(Type Type, object Instance)> RecurseInstantiateServiceConfigTargets(object obj, IServiceProvider serviceProvider)
        {
            var found = new List<(Type, object)>();
            var detectors = new[] { GetAndInstantiateIfServiceConfig };

            Traverse(obj, null, (o, prop) =>
            {
                var continue_ = true;
                foreach (var item in detectors)
                    continue_ &= item(o, prop);
                return continue_;
            });

            return found;

            bool GetAndInstantiateIfServiceConfig(object o, PropertyInfo? prop)
            {
                if (o is ServiceConfigBase sc)
                {
                    if (o.GetType().IsGenericType && sc.TargetType != null)
                    {
                        var rootType = o.GetType().GetGenericArguments().Single();
                        var instance = serviceProvider.GetService(sc.TargetType);
                        if (instance != null)
                            found.Add((rootType, instance));
                    }
                    return false;
                }
                else if (o is JToken)
                    return false;
                return true;
            }
        }

        public static void Traverse(object obj, PropertyInfo? parentProp, Func<object, PropertyInfo?, bool> func)
        {
            // TODO: extremely naïve implementation, vulnerable to e.g. circular references
            if (func(obj, parentProp) == false)
                return;

            var type = obj.GetType();
            if (!(type.IsAbstract || type.IsPrimitive || type == typeof(string)))
            {
                if (type.IsGenericType && type.GetInterfaces().Contains(typeof(System.Collections.IEnumerable)))
                {
                    var enumerable = obj as System.Collections.IEnumerable;
                    if (enumerable != null)
                    {
                        // TODO: if it's e.g. a KeyValue, e.g. from Dictionary?
                        foreach (var item in enumerable)
                            Traverse(item, null, func);
                    }
                }
                else
                {
                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        var val = prop.GetValue(obj);
                        if (val == null)
                            continue;
                        Traverse(val, prop, func);
                    }
                }
            }
        }


        //private static IEnumerable<(Type Type, object Instance)> RecurseGetAutoregisterItemsZ(object obj)
        //{
        //    return Traverse(obj, null, (o, prop) => {
        //        if (o is ServiceConfigBase confObj)
        //        {
        //            if (o.GetType().IsGenericType)
        //            {
        //                var conf = confObj.GetConfig();
        //                if (conf != null && confObj.GetConfigType() != null)
        //                    return CreateResult(confObj.GetConfigType()!, conf);
        //            }
        //        }
        //        else if (o is JToken)
        //            return CreateResult(null, null, false);

        //        var attr = prop?.GetCustomAttribute<RegisterOnRequestContextAttribute>();
        //        if (attr != null)
        //            return CreateResult(prop!.PropertyType, obj);

        //        return CreateResult(null, null);
        //    }).Where(o => o.Item1 != null && o.Item2 != null)
        //        .Select(o => (o.Item1!, o.Item2!))
        //        .ToList();

        //    TraverseActionResult<(Type?, object?)> CreateResult(Type? type, object? obj, bool continue_ = true)
        //    {
        //        return new TraverseActionResult<(Type?, object?)>((type, obj), continue_);
        //    }
        //}

        //private static IEnumerable<(Type Type, object Instance)> RecurseInstantiateServiceConfigTargets(object obj, IServiceProvider serviceProvider)
        //{
        //    var ienum = Traverse(obj, null, (o, prop) => {
        //        if (o is ServiceConfigBase sc)
        //        {
        //            if (o.GetType().IsGenericType)
        //            {
        //                var rootType = o.GetType().GetGenericArguments().Single();
        //                var instance = sc.GetInstance(serviceProvider);
        //                return CreateResult(rootType, instance, false);
        //            }
        //        }
        //        else if (o is JToken)
        //            return CreateResult(null, null, false);

        //        return CreateResult(null, null);
        //    });

        //    return ienum
        //        .Where(o => o.Item1 != null && o.Item2 != null)
        //        .Select(o => (o.Item1!, o.Item2!))
        //        .ToList();

        //    TraverseActionResult<(Type?, object?)> CreateResult(Type? type, object? obj, bool continue_ = true)
        //    {
        //        return new TraverseActionResult<(Type?, object?)>((type, obj), continue_);
        //    }
        //}

        //public class TraverseActionResult<T>
        //{
        //    public TraverseActionResult(T? result, bool continue_ = true)
        //    {
        //        Result = result;
        //        Continue = continue_;
        //    }
        //    public T? Result { get; }
        //    public bool Continue { get; }
        //}

        //public static IEnumerable<TResult> Traverse<TResult>(object obj, PropertyInfo? parentProp, Func<object, PropertyInfo?, TraverseActionResult<TResult>> func)
        //{
        //    // TODO: extremely naïve implementation, vulnerable to e.g. circular references
        //    var result = func(obj, parentProp);
        //    if (result.Result != null)
        //        yield return result.Result;

        //    if (result.Continue == true)
        //    {
        //        var type = obj.GetType();
        //        if (!(type.IsAbstract || type.IsPrimitive || type == typeof(string)))
        //        {
        //            if (type.IsGenericType && type.GetInterfaces().Contains(typeof(System.Collections.IEnumerable)))
        //            {
        //                var enumerable = obj as System.Collections.IEnumerable;
        //                if (enumerable != null)
        //                {
        //                    foreach (var item in enumerable)
        //                    {
        //                        // TODO: if it's e.g. a KeyValue, e.g. from Dictionary?
        //                        foreach (var yielded in Traverse(item, null, func))
        //                            yield return yielded;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                foreach (var prop in obj.GetType().GetProperties())
        //                {
        //                    var val = prop.GetValue(obj);
        //                    if (val == null)
        //                        continue;
        //                    foreach (var item in Traverse(val, prop, func))
        //                        yield return item;
        //                }
        //            }
        //        }
        //    }
        //}


        //public static IEnumerable<(Type Type, object Instance)> RecurseGetAutoregisterItems(object obj)
        //{
        //    // TODO: can we find an autofac IIActivator that takes a ServiceProvider plus arguments, similar to the old TypeInstantiator?
        //    // If so we don't need to register ServiceConfigs
        //    // With the current pattern, having more than one instance of a certain ServiceConfig will be difficult...
        //    var serviceConfigProps = obj.GetType().GetProperties().Where(o => ServiceConfigBase.GetTypeIsServiceConfig(o.PropertyType))
        //        .ToList();
        //    foreach (var prop in serviceConfigProps)
        //    {
        //        var value = prop.GetValue(obj);
        //        if (value != null)
        //        {
        //            var confObj = value as ServiceConfigBase;
        //            if (confObj == null)
        //                continue;
        //            var conf = confObj.GetConfig();
        //            if (conf == null)
        //                continue;
        //            if (confObj.GetConfigType() != null)
        //                yield return (confObj.GetConfigType()!, conf);
        //        }
        //    }

        //    foreach (var prop in obj.GetType().GetProperties())
        //    {
        //        var type = prop.PropertyType;
        //        // TODO: better way to skip builtin types. Or rather, should we provide a list of types to scan?
        //        if (type.IsAbstract || type.IsPrimitive)
        //            continue;

        //        var val = prop.GetValue(obj);
        //        if (val != null)
        //        {
        //            var attr = prop.GetCustomAttribute<RegisterOnRequestContextAttribute>();
        //            if (attr != null)
        //            {
        //                yield return (prop.PropertyType, val);
        //            }

        //            if (type.IsGenericType && type.GetInterfaces().Contains(typeof(System.Collections.IEnumerable))) 
        //            {
        //                var enumerable = val as System.Collections.IEnumerable;
        //                if (enumerable != null)
        //                {
        //                    foreach (var item in enumerable)
        //                    {
        //                        // TODO: if it's e.g. a KeyValue, e.g. from Dictionary?
        //                        foreach (var yielded in RecurseGetAutoregisterItems(item))
        //                            yield return yielded;
        //                    }
        //                    continue;
        //                }
        //            }

        //            foreach (var yielded in RecurseGetAutoregisterItems(val))
        //                yield return yielded;
        //        }
        //    }
        //}
    }
}
