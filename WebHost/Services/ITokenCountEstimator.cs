namespace WebHost.Services
{
    public interface ITokenCountEstimator
    {
        int Count(string text);
    }

    public class TokenCountEstimator : ITokenCountEstimator
    {
        public int Count(string text) => text.Length;
    }

}
