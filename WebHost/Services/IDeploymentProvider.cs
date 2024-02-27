using AutoRegister;

namespace WebHost.Services
{
    public interface IDeploymentProvider
    {
        string GetDeployment(string modelSelector, int minContextSize);
    }

    public class DeploymentProvider : IDeploymentProvider
    {
        private readonly DeploymentGroup deploymentGroup;

        public DeploymentProvider(DeploymentGroup deploymentGroup)
        {
            this.deploymentGroup = deploymentGroup;
        }

        public string GetDeployment(string modelSelector, int minContextSize)
        {
            return $"{deploymentGroup}";
        }
    }

    public class DeploymentGroup : PrimitiveWrapperBase<string>
    {
        public DeploymentGroup(string id) : base(id) { }
    }

}
