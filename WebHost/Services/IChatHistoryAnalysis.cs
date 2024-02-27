using AutoRegister;

namespace WebHost.Services
{
    public interface IChatHistoryAnalysis
    {
        Task<string> Analyze(string text);
    }

    public class ChatHistoryAnalysis : IChatHistoryAnalysis
    {
        private readonly Config config;

        public class Config
        {
            public string Value { get; set; } = string.Empty;
        }

        public ChatHistoryAnalysis([ServiceConfigParameter] Config config)
        {
            this.config = config;
        }

        public override string ToString() => config.Value;

        public Task<string> Analyze(string text)
        {
            return Task.FromResult($"analyzed: {text} with {config.Value}");
        }
    }
}
