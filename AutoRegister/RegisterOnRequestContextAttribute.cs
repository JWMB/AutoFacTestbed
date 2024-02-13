namespace AutoRegister
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RegisterOnRequestContextAttribute : Attribute
    {
        public Type? RegisterAs { get; set; }
    }
}
