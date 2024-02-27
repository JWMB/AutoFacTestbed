using AutoRegister;

namespace WebHost.Services
{
    public interface ICompletionService
    {
        Task<string> GetChatCompletions(string prompt, CompletionSettings settings);
    }
    public record CompletionSettings(int MaxTokens);

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
}
