namespace WebHost.Services
{
    public interface IServiceA
    {
    }
    public class ServiceA : IServiceA
    {
        private readonly SessionId sessionId;

        public ServiceA(SessionId sessionId)
        {
            this.sessionId = sessionId;
        }

        public override string ToString() => $"{GetType().Name} - {sessionId}";
    }
}
