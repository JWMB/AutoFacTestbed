using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoRegister;
using FakeItEasy;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Reflection;
using WebHost.Controllers;
using WebHost.Services;
using static WebHost.Controllers.TestController;

namespace WebHost.Tests
{
    public class UnitTest1
    {
        // Problem:
        // * same payload with two instances of same T of ServiceConfig<T> with different configs
        //   currently the config is registered as a service - how can they be separated? Using the property path as the key (KeyedService)?

        [Fact]
        public void UnderstandingAutofacKeyed()
        {
            var s1 = new ClassForKeyed { Value = 1 };
            var s2 = new ClassForKeyed { Value = 2 };

            var builder = new ContainerBuilder();
            builder.Register(c => s1).Named("key1", typeof(ClassForKeyed));
            builder.Register(c => s2).Named("key2", typeof(ClassForKeyed));

            var container = builder.Build();
            var serviceProvider = new AutofacServiceProvider(container);

            serviceProvider.GetService<ClassForKeyed>().ShouldBeNull();
            serviceProvider.GetKeyedService<ClassForKeyed>("key1")?.Value.ShouldBe(s1.Value);
            serviceProvider.GetKeyedService<ClassForKeyed>("key2")?.Value.ShouldBe(s2.Value);
        }

        public class ClassForKeyed
        {
            public int Value { get; set; }
        }

        [Fact]
        public async Task MultipleInstances()
        {
            var scopeActivator = new ScopeActivator();

            var sut = new TestController(scopeActivator);

            var dto = new DuplicatesDto
            {
                PluginA = new ServiceConfig<IChatHistoryAnalysis>(typeof(ChatHistoryAnalysis), new { Value = "A" }),
                PluginB = new ServiceConfig<IChatHistoryAnalysis>(typeof(ChatHistoryAnalysis), new { Value = "B" })
            };

            PrepareControllerContext(sut, nameof(sut.Duplicates), dto, services =>
            {
                ConfigureDefaultServices(services);
                TypeNameCache.RegisterType(typeof(ChatHistoryAnalysis));
            });

            var response = await sut.Duplicates(dto);

        }
         
        [Fact]
        public async Task Test0()
        {
            var chatHistoryAnalysis = A.Fake<IChatHistoryAnalysis>();
            var fakeRegistrations = new Dictionary<Type, object> { { typeof(IChatHistoryAnalysis), chatHistoryAnalysis } };
            var scopeActivator = new ScopeActivator(builder => new FakeSimpleServiceCollection(builder, fakeRegistrations));

            var sut = new TestController(scopeActivator);

            var dto = new SimpleDto {
                Plugin = new ServiceConfig<IChatHistoryAnalysis>(typeof(ChatHistoryAnalysis), new { Value = "myval" })
            };

            PrepareControllerContext(sut, nameof(sut.Simple), dto, services =>
            {
                ConfigureDefaultServices(services);
                TypeNameCache.RegisterType(typeof(ChatHistoryAnalysis));
            });

            var response = await sut.Simple(dto);

            // Assert
            response.ShouldNotBeNull();
            A.CallTo(() => chatHistoryAnalysis.Analyze(A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Test1()
        {
            // Arrange
            var sut = new TestController(new ScopeActivator());

            var tokenCountEstimator = A.Fake<ITokenCountEstimator>();

            var dto = new TestController.PromptDto
            {
                CompletionService = new ServiceConfig<ICompletionService>(nameof(CompletionService), new { ModelSelector = "gpt-4" }),
                SessionId = new SessionId("123"),
                Plugin = new ServiceConfig<IChatHistoryAnalysis>(nameof(ChatHistoryAnalysis), new { Value = "abc" }),
            };

            PrepareControllerContext(sut, nameof(sut.CreatePrompt), dto, services =>
            {
                ConfigureDefaultServices(services);
                services.AddSingleton<ITokenCountEstimator>(sp => tokenCountEstimator);

                // Used by ServiceConfig.GetConfigType() / GetConfig()
                // Could we use IServiceProvider instead? But we need types, not instances
                TypeNameCache.RegisterType(typeof(ChatHistoryAnalysis));
                TypeNameCache.RegisterType(typeof(CompletionService));
            });

            // Act

            var response = await sut.CreatePrompt(dto);

            // Assert
            response.ShouldNotBeNull();
            A.CallTo(() => tokenCountEstimator.Count(A<string>._)).MustHaveHappenedOnceExactly();
        }

        public class FakeSimpleServiceCollection(ContainerBuilder builder, Dictionary<Type, object> overrides) : ISimpleServiceCollection
        {
            private readonly ContainerBuilder builder = builder;
            public void Add(Type type, object implementation) =>
                builder.Register(c => overrides.TryGetValue(type, out var v) ? v : implementation).As(type);

            public void AddKeyed(Type type, object key, object implementation) => throw new NotImplementedException();
        }

        private void PrepareControllerContext(ControllerBase controller, string methodName, object payload, Action<IServiceCollection>? addServices)
        {
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = CreateHttpContext(addServices)
            };

            var method = controller.GetType().GetMethod(methodName);
            if (method?.GetCustomAttribute<AutoRegisterRequestContentAttribute>() != null)
                AutoRegisterRequestContentAttribute.UpdateRequestScope(controller.HttpContext, new Dictionary<string, object?> { { "", payload } });
        }

        private void ConfigureDefaultServices(IServiceCollection services)
        {
            services.AddScoped<ChatHistoryAnalysis>();
            services.AddScoped<IServiceA, ServiceA>();
            services.AddScoped<ICompletionService, CompletionService>();
            services.AddScoped<CompletionService>();

            services.AddScoped<ITokenCountEstimator, TokenCountEstimator>();
            services.AddScoped<IDeploymentProvider, DeploymentProvider>();

            services.AddScoped<DefaultPromptBuilder>();
        }

        private HttpContext CreateHttpContext(Action<IServiceCollection>? addServices)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            addServices?.Invoke(services);

            builder.Populate(services);
            var container = builder.Build();
            var serviceProvider = new AutofacServiceProvider(container);

            var httpContext = A.Fake<HttpContext>();
            var features = A.Fake<IFeatureCollection>();
            //var serviceProvidersFeature = A.Fake<IServiceProvidersFeature>();
            //A.CallTo(() => serviceProvidersFeature.RequestServices).Returns(serviceProvider);
            var serviceProvidersFeature = new MockServiceProvidersFeature(serviceProvider);
            
            //A.CallTo(() => features.GetEnumerator()).Returns((new[] { new KeyValuePair<Type, object>(typeof(IServiceProvidersFeature), serviceProvidersFeature) }).ToList().GetEnumerator());
            A.CallTo(() => features.Get<IServiceProvidersFeature>()).Returns(serviceProvidersFeature);

            A.CallTo(() => httpContext.Features).Returns(features);
            return httpContext;
        }

        public class MockServiceProvidersFeature : IServiceProvidersFeature
        {
            public MockServiceProvidersFeature(IServiceProvider requestServices)
            {
                RequestServices = requestServices;
            }
            public IServiceProvider RequestServices { get; set; }
        }
    }
}