using System.Reflection;

namespace AutoRegister
{
    public class ObjectTraverser
    {
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
    }
}
