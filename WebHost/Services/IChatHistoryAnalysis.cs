using AutoRegister;

namespace WebHost.Services
{
    public interface IChatHistoryAnalysis { }
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
    }
}
