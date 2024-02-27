using Autofac;
using AutoRegister;

namespace WebHost
{
    public class ContainerBuilderSimpleServiceCollection(ContainerBuilder builder) : ISimpleServiceCollection
    {
        private readonly ContainerBuilder builder = builder;
        public void Add(Type type, object implementation) => builder.Register(c => implementation).As(type);

        public void AddKeyed(Type type, object key, object implementation) => builder.Register(c => implementation).Named($"{key}", type);
    }
}

