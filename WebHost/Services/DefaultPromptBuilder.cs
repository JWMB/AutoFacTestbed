using WebHost.Services;

namespace WebHost.Controllers
{
    public partial class TestController
    {
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
    }
}
