using Autofac;
using AutoRegister;
using Microsoft.AspNetCore.Mvc;
using WebHost.Services;

namespace WebHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public partial class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [AutoRegisterRequestContent]
        public async Task<object> Post(ConfigDto dto)
        {
            using var leafSP = ScopeActivator.CreateScopes(HttpContext, new Action<IServiceProvider, ContainerBuilder>[]
            {
                (sp, b) => b.Register(ctx => new DeploymentGroup("Azure")),
                (sp, b) => AutoRegisterUtils.RegisterServiceConfigTargets(dto, sp, b),
            });
            var promptBuilder = leafSP.GetService<DefaultPromptBuilder>();

            //var promptBuilder = ScopeActivator.CreateOnNewScope<DefaultPromptBuilder>(HttpContext, (sp, builder) => {
            //    AutoRegisterUtils.RegisterServiceConfigTargets(dto, serviceProvider, builder);
            //});
            return $"promptBuilder={promptBuilder}\nPrompt={(promptBuilder == null ? "N/A" : await promptBuilder.GetPrompt("hello"))}";
        }

        public class DefaultPromptBuilder(IServiceA serviceA, SessionId sessionId, IChatHistoryAnalysis chatHistoryPlugin, ICompletionService completionService)
        {
            public override string ToString()
            {
                return $"ServiceA:{serviceA}, sessionId:{sessionId} chatHistoryPlugin:{chatHistoryPlugin} completionService:{completionService}";
            }
            public async Task<string> GetPrompt(string userInput)
            {
                return $"{await completionService.GetChatCompletions(userInput, new CompletionSettings(200))}";
            }
        }

        public class ConfigDto
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


    public interface ICompletionService
    {
        Task<string> GetChatCompletions(string prompt, CompletionSettings settings);
    }
    public class CompletionService : ICompletionService
    {
        private readonly Config config;
        private readonly IDeploymentProvider deploymentProvider;
        private readonly ITokenCountEstimator tokenCountEstimator;

        public record Config(string ModelSelector)
        {
        }

        public CompletionService([ServiceConfigParameter] Config config, IDeploymentProvider deploymentProvider, ITokenCountEstimator tokenCountEstimator)
        {
            this.config = config;
            this.deploymentProvider = deploymentProvider;
            this.tokenCountEstimator = tokenCountEstimator;
        }

        public Task<string> GetChatCompletions(string prompt, CompletionSettings settings)
        {
            var deployment = deploymentProvider.GetDeployment(config.ModelSelector, tokenCountEstimator.Count(prompt) + settings.MaxTokens);
            return Task.FromResult($"deployment={deployment} prompt={prompt}");
        }
    }

    public record CompletionSettings(int MaxTokens);
    public interface ITokenCountEstimator
    {
        int Count(string text);
    }
    public class TokenCountEstimator : ITokenCountEstimator
    {
        public int Count(string text) => text.Length;
    }
    public interface IDeploymentProvider
    {
        string GetDeployment(string modelSelector, int minContextSize);
    }

    public class DeploymentGroup : PrimitiveWrapperBase<string>
    {
        public DeploymentGroup(string id) : base(id) { }
    }

    public class DeploymentProvider : IDeploymentProvider
    {
        private readonly DeploymentGroup deploymentGroup;

        public DeploymentProvider(DeploymentGroup deploymentGroup)
        {
            this.deploymentGroup = deploymentGroup;
        }

        public string GetDeployment(string modelSelector, int minContextSize)
        {
            return $"{deploymentGroup}";
        }
    }
}
