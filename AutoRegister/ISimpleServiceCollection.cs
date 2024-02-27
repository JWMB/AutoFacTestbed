namespace AutoRegister
{
    public interface ISimpleServiceCollection
    {
        //void Add<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
        //    where TService : class
        //    where TImplementation : class, TService;
        //void Add<TService, TImplementation>(TImplementation implementation)
        //    where TService : class
        //    where TImplementation : class, TService;
        void Add(Type type, object implementation);
        void AddKeyed(Type type, object key, object implementation);

        public void Add<TService, TImplementation>(TImplementation implementation)
            where TService : class
            where TImplementation : class, TService
        {
            Add(typeof(TService), implementation);
        }
    }
}

