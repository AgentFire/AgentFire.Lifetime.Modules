using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AgentFire.Lifetime.Modules
{
    /// <summary>
    /// Provides a single-threaded way to start and stop your modules, which implement <see cref="IModule"/>.
    /// Only non-abstract modules with public parameterless constructors will be used.
    /// </summary>
    public sealed class ModuleManager
    {
        /// <summary>
        /// Starts all Auto-Start modules from a calling assembly.
        /// </summary>
        public void Start()
        {
            Start(Assembly.GetCallingAssembly());
        }
        /// <summary>
        /// Starts all Auto-Start modules from a specified assembly.
        /// </summary>
        public void Start(Assembly assemblyWithModules)
        {
            StartInternal(assemblyWithModules);
        }

        /// <summary>
        /// Stops all running modules.
        /// </summary>
        public void Stop()
        {
            StopInternal();
        }

        private readonly Dictionary<Type, ModuleStartContext> _allModules = new Dictionary<Type, ModuleStartContext>();

        public IEnumerable<IModule> GetRunningModules()
        {
            return _allModules.Values.Select(T => T.Module).Where(T => T.IsRunning);
        }

        private void StartInternal(Assembly assemblyWithModules)
        {
            var query = from type in assemblyWithModules.DefinedTypes
                        where !_allModules.ContainsKey(type)
                        where !type.IsAbstract
                        where typeof(IModule).IsAssignableFrom(type)
                        let c = type.GetConstructor(Type.EmptyTypes)
                        where c != null
                        let instance = (IModule)c.Invoke(null)
                        select instance;

            foreach (IModule module in query)
            {
                EnsureLoaded(module);

                if (module.AutoStart && !module.IsRunning)
                {
                    StartModule(module, new HashSet<IModule>());
                }
            }
        }

        private void EnsureLoaded(IModule module)
        {
            if (_allModules.ContainsKey(module.GetType()))
            {
                return;
            }

            ModuleStartContext context = new ModuleStartContext(module);
            module.Initialize(context);
            _allModules[module.GetType()] = context;

            foreach (Type moduleType in context.RequiredDependencies)
            {
                ModuleStartContext mc;

                if (!_allModules.TryGetValue(moduleType, out mc))
                {
                    IModule m = (IModule)Activator.CreateInstance(moduleType);
                    EnsureLoaded(m);
                }

                // No StackOverflow exception due to the actual adding them in the previous line.
            }
        }

        private void StartModule(IModule module, HashSet<IModule> hashSet)
        {
            if (hashSet.Contains(module))
            {
                throw new InvalidOperationException("Circular dependencies are not allowed.");
            }

            hashSet.Add(module);

            foreach (IModule dependency in _allModules[module.GetType()].RequiredDependencies.Select(T => _allModules[T].Module))
            {
                if (!dependency.IsRunning)
                {
                    StartModule(dependency, hashSet);
                }
            }

            module.Start();

            hashSet.Remove(module);
        }

        private void StopInternal()
        {
            foreach (IModule module in GetRunningModules())
            {
                module.Stop();
            }
        }

        public T TryGetModule<T>(bool onlyIfRunning = true) where T : IModule
        {
            ModuleStartContext c;
            T module = _allModules.TryGetValue(typeof(T), out c) ? (T)c.Module : default(T);

            if (onlyIfRunning && module != null && !module.IsRunning)
            {
                return default(T);
            }

            return module;
        }
    }
}
