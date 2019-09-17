using System;

namespace AgentFire.Lifetime.Modules
{
    /// <summary>
    /// Used to require dependencies.
    /// </summary>
    public interface IDependencyResolverContext
    {
        /// <summary>
        /// Your dependencies here.
        /// </summary>
        void RequireDependency<T>() where T : IModule;

        /// <summary>
        /// Your dependencies here.
        /// </summary>
        void RequireDependency(Type moduleType);
    }
}
