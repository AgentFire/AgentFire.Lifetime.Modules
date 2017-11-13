using System;
using System.Collections.Generic;

namespace AgentFire.Lifetime.Modules
{
    internal sealed class DependencyResolverContext : IDependencyResolverContext
    {
        public Type ModuleType { get; }
        public IModule Module { get; }
        public IReadOnlyCollection<Type> RequiredDependencies => _requiredDependencies;

        private readonly HashSet<Type> _requiredDependencies = new HashSet<Type>();

        public DependencyResolverContext(IModule module)
        {
            Module = module;
            ModuleType = Module.GetType();
        }

        void IDependencyResolverContext.RequireDependency<T>()
        {
            _requiredDependencies.Add(typeof(T));
        }
        void IDependencyResolverContext.RequireDependency(Type moduleType)
        {
            if (moduleType == null)
            {
                throw new ArgumentNullException(nameof(moduleType));
            }

            if (!typeof(IModule).IsAssignableFrom(moduleType))
            {
                throw new ArgumentException($"Type must implement {nameof(IModule)} interface.", nameof(moduleType));
            }

            if (moduleType == Module.GetType())
            {
                return;
            }

            _requiredDependencies.Add(moduleType);
        }
    }
}
