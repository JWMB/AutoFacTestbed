using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoFacTestbed;
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using static AutoFacTestbed.SomeController;

var serviceCollection = new ServiceCollection();

ConfigureServices(serviceCollection);
var builder = new ContainerBuilder();
builder.Populate(serviceCollection);
ConfigureAutofacServices(builder);
var container = builder.Build();
var serviceProvider = new AutofacServiceProvider(container);

TypeNameCache.RegisterAssemblies(AppDomain.CurrentDomain.GetAssemblies());

using (var scope = container.BeginLifetimeScope(
  builder => {
      // Do this at beginning of a request
      var requestBodyString =
      """
      {
        "SessionId": "abc",
        "Plugin": {
           "$type": "SomePluginA",
           "Value": "my value"
        }
      }
      """;

      var requestBody = JsonConvert.DeserializeObject<ConfigDto>(requestBodyString, new PrimitiveWrapperJsonConverter(), new ServiceConfigJsonConverter());
      if (requestBody != null)
          foreach (var item in AutoRegisterUtils.RecurseGetAutoregisterItems(requestBody))
              builder.Register(c => item.Instance).As(item.Type);
  }))
{
    var sessionId = scope.Resolve<SessionId>();
    var service = scope.Resolve<IServiceA>();
    if (((ServiceA)service).SessionId != sessionId || sessionId.ToString() != "abc")
        throw new Exception("!");

    var plugin = scope.Resolve<ISomePlugin>();
    if (plugin.ToString() != "my value")
        throw new Exception("!");
}

void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IServiceA, ServiceA>();
    services.AddTransient<IServiceB, ServiceB>();
    services.AddTransient<ISomePlugin, SomePluginA>();
}

void ConfigureAutofacServices(ContainerBuilder builder)
{
}
