using AutoRegister;

namespace AutoFacTestbed
{
    public interface IServiceA
    { }

    public class ServiceA : IServiceA //(SessionId sessionId) : IServiceA
    {
        private SessionId sessionId;
        public ServiceA(SessionId sessionId)
        {
            this.sessionId = sessionId;
        }
        public SessionId SessionId => sessionId;
    }

    public interface IServiceB
    { }

    public class ServiceB(IServiceA serviceA) : IServiceB
    {
        public IServiceA ServiceA => serviceA;
    }

    public interface ISomePlugin { }
    public class SomePluginA : ISomePlugin
    {
        private readonly Config config;

        public class Config
        {
            public string Value { get; set; } = string.Empty;
        }

        public SomePluginA([ServiceConfigParameter] Config config)
        {
            this.config = config;
        }

        public override string ToString() => config.Value;
    }

    public class SomeController
    {
        public void Post(ConfigDto dto)
        {
        }

        public class ConfigDto //(SessionId SessionId)
        {
            [RegisterOnRequestContext]
            public SessionId? SessionId { get; set; }
            public ServiceConfig<ISomePlugin>? Plugin { get; set;}
        }
    }
}
