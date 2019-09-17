using System;

namespace AgentFire.Lifetime.Modules
{
    /// <summary>
    /// Dependency resolver for modules.
    /// </summary>
    public interface IModuleResolver
    {
        /// <summary>
        /// Returns an instance or null.
        /// </summary>
        T TryGet<T>() where T : IModule;

        /// <summary>
        /// Returns an instance or null.
        /// </summary>
        IModule TryGet(Type type);

        /// <summary>
        /// Removes all cached instances, if any.
        /// </summary>
        void Reset();
    }
}
