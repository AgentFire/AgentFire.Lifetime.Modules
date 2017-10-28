using System;
using System.Collections.Generic;

namespace AgentFire.Lifetime.Modules
{
    internal sealed class ModuleStartContext : IModuleStartContext
    {
        internal Type ModuleType { get; }
        internal IModule Module { get; }
        public HashSet<Type> RequiredDependencies { get; } = new HashSet<Type>();

        public ModuleStartContext(IModule module)
        {
            Module = module;
            ModuleType = Module.GetType();
        }

        public void RequireDependency<T>() where T : IModule
        {
            RequiredDependencies.Add(typeof(T));
        }
        public void RequireDependency(Type moduleType)
        {
            if (!typeof(IModule).IsAssignableFrom(moduleType))
            {
                throw new ArgumentException($"Type must implement {nameof(IModule)} interface.", nameof(moduleType));
            }

            RequiredDependencies.Add(moduleType);
        }
    }
}
