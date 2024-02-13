using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoRegister;
using static WebHost.Controllers.TestController;
using WebHost.Controllers;

namespace WebHost
{
    public static class ScopeActivator
    {
        public static object? CreateOnNewScope(Type type, AutofacServiceProvider asp, Action<IServiceProvider, ContainerBuilder> build)
        {
            using var childScope = asp.LifetimeScope.BeginLifetimeScope(builder =>
            {
                build(asp, builder);
            });
            using var sp = new AutofacServiceProvider(childScope);
            return sp.GetService(type);
        }

        public static object? CreateOnNewScope(Type type, HttpContext context, Action<IServiceProvider, ContainerBuilder> build)
        {
            var serviceProvider = AutoRegisterRequestContentAttribute.GetServiceProvidersFeature(context)?.RequestServices;
            if (serviceProvider is AutofacServiceProvider asp)
            {
                using var childScope = asp.LifetimeScope.BeginLifetimeScope(builder =>
                {
                    build(serviceProvider, builder);
                });
                using var sp = new AutofacServiceProvider(childScope);
                return sp.GetService(type);
            }
            return null;
        }

        public static T CreateOnNewScope<T>(HttpContext context, Action<IServiceProvider, ContainerBuilder> build) where T : class
        {
            var instance = CreateOnNewScope(typeof(T), context, build);
            if (instance == null)
                throw new Exception("Could not create instance");
            var typed = instance as T;
            if (typed == null)
                throw new Exception("Incorrect type");
            return typed;
        }

        public static AutofacServiceProvider CreateScopes(HttpContext context, IEnumerable<Action<IServiceProvider, ContainerBuilder>> scopeCreators)
        {
            var serviceProvider = AutoRegisterRequestContentAttribute.GetServiceProvidersFeature(context)?.RequestServices;
            if (serviceProvider is not AutofacServiceProvider asp)
                throw new Exception("not autofact");
            return CreateScopes(asp, scopeCreators);
        }

        public static AutofacServiceProvider CreateScopes(AutofacServiceProvider parent, IEnumerable<Action<IServiceProvider, ContainerBuilder>> scopeCreators)
        {
            var currentParent = parent;
            foreach (var item in scopeCreators)
            {
                var childScope = currentParent.LifetimeScope.BeginLifetimeScope(builder => item(currentParent, builder));
                currentParent = new AutofacServiceProvider(childScope);
            }
            return currentParent;
        }
    }
}

