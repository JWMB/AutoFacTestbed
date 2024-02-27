using Autofac;
using AutoRegister;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WebHost.Services;

namespace WebHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public partial class TestController : ControllerBase
    {
        private readonly IScopeActivator scopeActivator;

        public TestController(IScopeActivator scopeActivator)
        {
            this.scopeActivator = scopeActivator;
        }

        [HttpPost]
        [AutoRegisterRequestContent]
        public async Task<string> Simple(SimpleDto dto)
        {
            using var serviceProvider = scopeActivator.CreateScopesX(HttpContext, new Action<IServiceProvider, ISimpleServiceCollection>[]
            {
                (sp, b) => AutoRegisterUtils.RegisterServiceConfigTargetsX(dto, sp, b),
            });
            var chatHistoryAnalysis = serviceProvider.GetRequiredService<IChatHistoryAnalysis>();

            return $"{await chatHistoryAnalysis.Analyze("some text")}";
        }
        public class SimpleDto
        {
            public ServiceConfig<IChatHistoryAnalysis>? Plugin { get; set; }
        }


        [HttpPost]
        [AutoRegisterRequestContent]
        public async Task<string> Duplicates(DuplicatesDto dto)
        {
            using var serviceProvider = scopeActivator.CreateScopesX(HttpContext, new Action<IServiceProvider, ISimpleServiceCollection>[]
            {
                (sp, b) => AutoRegisterUtils.RegisterServiceConfigTargetsX(dto, sp, b),
            });
            var a = serviceProvider.GetRequiredKeyedService<IChatHistoryAnalysis>(nameof(dto.PluginA));
            var b = serviceProvider.GetRequiredKeyedService<IChatHistoryAnalysis>(nameof(dto.PluginB));

            return $"{await a.Analyze("some text")} / {await b.Analyze("some text")}";
        }

        public class DuplicatesDto
        {
            public ServiceConfig<IChatHistoryAnalysis>? PluginA { get; set; }
            public ServiceConfig<IChatHistoryAnalysis>? PluginB { get; set; }
        }
        //public class ServiceXA
        //{
        //    public ServiceXA([FromKeyedServices("A")] ICompletionService completionService)
        //    {
        //    }
        //}
        //public class ServiceXB
        //{
        //    public ServiceXB([FromKeyedServices("B")] ICompletionService completionService)
        //    {
        //    }
        //}


        [HttpPost]
        [AutoRegisterRequestContent]
        public async Task<object> CreatePrompt(PromptDto dto)
        {
            using var serviceProvider = scopeActivator.CreateScopes(HttpContext, new Action<IServiceProvider, ContainerBuilder>[]
            {
                (sp, b) => b.Register(ctx => new DeploymentGroup("Azure")),
                (sp, b) => AutoRegisterUtils.RegisterServiceConfigTargets(dto, sp, b),
            });
            var promptBuilder = serviceProvider.GetRequiredService<DefaultPromptBuilder>();

            return $"promptBuilder={promptBuilder}\nPrompt={await promptBuilder.GetPrompt("hello")}";
        }

        public class PromptDto
        {
            [RegisterOnRequestContext]
            public SessionId? SessionId { get; set; }
            public ServiceConfig<IChatHistoryAnalysis>? Plugin { get; set; }
            public ServiceConfig<ICompletionService>? CompletionService { get; set; }

            public static string ExampleJson =
"""
{
    "SessionId": "abc",
    "Plugin": {
        "$type": "ChatHistoryAnalysis",
        "Value": "my value"
    },
    "CompletionService": {
        "$type": "CompletionService",
        "ModelSelector": "gpt-4-1106"
    }
}
""";
        }
    }
}
