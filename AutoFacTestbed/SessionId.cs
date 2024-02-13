using AutoRegister;

namespace AutoFacTestbed
{
    [PrimitiveWrapper]
    public class SessionId : PrimitiveWrapperBase<string>
    {
        public SessionId(string id) : base(id) { }
    }
}
