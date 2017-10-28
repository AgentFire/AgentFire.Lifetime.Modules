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
        /// Starts all Auto-Start modules from a specified set of assemblies.
        /// </summary>
        public void Start(params Assembly[] assembliesWithModules)
        {
            StartInternal(assembliesWithModules);
        }

        /// <summary>
        /// Stops all running modules.
        /// </summary>
        public void Stop()
        {
            StopInternal();
        }

        private readonly List<ModuleStartContext> _allModules = new List<ModuleStartContext>();

        public IEnumerable<IModule> GetRunningModules()
        {
            return _allModules.Select(T => T.Module).Where(T => T.IsRunning);
        }

        private void StartInternal(params Assembly[] assembliesWithModules)
        {
            var query = from ass in assembliesWithModules
                        from type in ass.DefinedTypes
                        where _allModules.All(T => T.ModuleType != type)
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
            if (_allModules.Any(T => T.Module == module))
            {
                return;
            }

            ModuleStartContext context = new ModuleStartContext(module);
            module.Initialize(context);
            _allModules.Add(context);

            foreach (Type moduleType in context.RequiredDependencies)
            {
                ModuleStartContext mc = _allModules.Where(T => T.ModuleType == moduleType).SingleOrDefault();

                if (mc == null)
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
                throw new InvalidOperationException("I've detected a circular module dependency. How dare you.");
            }

            hashSet.Add(module);

            foreach (IModule dependency in _allModules.Where(T => T.Module == module).Single().RequiredDependencies.Select(T => _allModules.Where(W => W.ModuleType == T).Single().Module))
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
            foreach (IModule module in GetRunningModules().Reverse())
            {
                module.Stop();//
            }
        }

        public T TryGetModule<T>(bool onlyIfRunning = true) where T : IModule
        {
            ModuleStartContext c;
            T module = _allModules.Where(W => W.ModuleType == typeof(T)).Select(W => (T)W.Module).SingleOrDefault();

            if (onlyIfRunning && module != null && !module.IsRunning)
            {
                return default(T);
            }

            return module;
        }
    }
}
