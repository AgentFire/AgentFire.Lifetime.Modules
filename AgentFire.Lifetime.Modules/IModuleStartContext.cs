using System;

namespace AgentFire.Lifetime.Modules
{
    public interface IModuleStartContext
    {
        void RequireDependency<T>() where T : IModule;
        void RequireDependency(Type moduleType);
    }
}
