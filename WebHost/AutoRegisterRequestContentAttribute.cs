using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoRegister;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebHost
{
    public class AutoRegisterRequestContentAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context) { }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.Method == HttpMethods.Post)
            {
                UpdateRequestScope(context.HttpContext, context.ActionArguments);
            }
        }

        public static void UpdateRequestScope(HttpContext context, IDictionary<string, object?> actionArguments)
        {
            var serviceProvidersFeature = GetServiceProvidersFeature(context);
            if (serviceProvidersFeature == null)
                return;

            var serviceProvider = serviceProvidersFeature.RequestServices;
            if (serviceProvider is AutofacServiceProvider asp)
            {
                //suggestion in https://stackoverflow.com/a/38881836 isn't valid (asp.LifetimeScope.ComponentRegistry.Register)

                // TODO: childScope disposal?
                // TODO: only for the body? context.ActionDescriptor.EndpointMetadata / Properties / MethodInfo
                var childScope = CreateScope(asp, actionArguments);
                serviceProvidersFeature.RequestServices = new AutofacServiceProvider(childScope);
            }

        }

        private static ILifetimeScope CreateScope(AutofacServiceProvider asp, IDictionary<string, object?> actionArguments)
        {
            return asp.LifetimeScope.BeginLifetimeScope(builder =>
            {
                foreach (var actionArg in actionArguments)
                {
                    if (actionArg.Value == null)
                        continue;
                    foreach (var item in AutoRegisterUtils.RecurseGetAutoregisterItems(actionArg.Value))
                        builder.Register(c => item.Instance).As(item.Type); // What is .ExternallyOwned()?
                }
            });
        }

        public static IServiceProvidersFeature? GetServiceProvidersFeature(HttpContext context)
        {
            return context.Features.Get<IServiceProvidersFeature>();
            //return context.Features
            //    .Select(o => o.Value)
            //    .OfType<IServiceProvidersFeature>()
            //    .FirstOrDefault();
        }
    }
}
