using System;

namespace AgentFire.Lifetime.Modules
{
    public interface IDependencyResolverContext
    {
        void RequireDependency<T>() where T : IModule;
        void RequireDependency(Type moduleType);
    }
}
