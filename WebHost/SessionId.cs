using AutoRegister;

namespace WebHost
{
    [PrimitiveWrapper]
    public class SessionId : PrimitiveWrapperBase<string>
    {
        public SessionId(string id) : base(id) { }
    }
}
