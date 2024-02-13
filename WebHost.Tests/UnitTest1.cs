using AutoRegister;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using WebHost.Controllers;
using WebHost.Services;

namespace WebHost.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var sut = new TestController(A.Fake<ILogger<TestController>>());
            var response = await sut.Post(new TestController.ConfigDto
            {
                CompletionService = new ServiceConfig<ICompletionService>(nameof(CompletionService), new { ModelSelector = "gpt-4" }),
                SessionId = new SessionId("123"),
                Plugin = new ServiceConfig<IChatHistoryAnalysis>(nameof(ChatHistoryAnalysis), new { Value = "abc" }),
            });
            
        }
    }
}