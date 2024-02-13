using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoRegister;
using WebHost.Controllers;
using WebHost.Services;
using static WebHost.Controllers.TestController;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(
   builder => {
       //builder.RegisterType<ServiceA>().As<IServiceA>();
   });

var services = builder.Services;

services.AddControllers(config => {
}).AddJsonOptions(config => {
    config.JsonSerializerOptions.Converters.Add(new ServiceConfigJsonConverterSystemText());
    config.JsonSerializerOptions.Converters.Add(new PrimitiveWrapperJsonConverterSystemText<string>());
});

services.AddScoped<ChatHistoryAnalysis>();
services.AddScoped<IServiceA, ServiceA>();
services.AddScoped<ICompletionService, CompletionService>();
services.AddScoped<CompletionService>();

services.AddScoped<ITokenCountEstimator, TokenCountEstimator>();
services.AddScoped<IDeploymentProvider, DeploymentProvider>();

services.AddScoped<DefaultPromptBuilder>();
 
TypeNameCache.RegisterAssemblies(AppDomain.CurrentDomain.GetAssemblies());

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();
