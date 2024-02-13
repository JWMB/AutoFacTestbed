using System.Reflection;

namespace AutoRegister
{
    public static class TypeNameCache
    {
        private static List<Assembly> registeredAssemblies = new();
        private static Dictionary<string, Type?> cache = new();
        public static Type? FindByName(string typeName)
        {
            if (cache.TryGetValue(typeName, out var cacheHit))
                return cacheHit;
            var found = registeredAssemblies.Select(o => o.GetTypes().SingleOrDefault(t => t.Name == typeName))
                .OfType<Type>().FirstOrDefault();
                
            cache.Add(typeName, found);
            return found;
        }
        public static void RegisterAssemblies(params Assembly[] assemblies)
        {
            foreach (var item in cache.Where(o => o.Value == null).ToList())
                cache.Remove(item.Key);
            var nonregistered = assemblies.Except(registeredAssemblies);
            registeredAssemblies.AddRange(nonregistered);
        }
    }
}
