using AgentFire.Lifetime.Modules.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
        public Task Start()
        {
            return Start(Assembly.GetCallingAssembly());
        }
        /// <summary>
        /// Starts all Auto-Start modules from a specified assembly.
        /// </summary>
        public Task Start(Assembly assemblyWithModules)
        {
            return Start(new[] { assemblyWithModules });
        }
        /// <summary>
        /// Starts all Auto-Start modules from a specified set of assemblies.
        /// </summary>
        public Task Start(params Assembly[] assembliesWithModules)
        {
            return StartInternal(assembliesWithModules);
        }
        /// <summary>
        /// Starts all Auto-Start modules from a specified set of assemblies.
        /// </summary>
        public Task Start(IEnumerable<Assembly> assembliesWithModules)
        {
            return StartInternal(assembliesWithModules);
        }

        /// <summary>
        /// Stops all running modules.
        /// </summary>
        public Task Shutdown()
        {
            return StopInternal();
        }

        private bool _isRunning = false;
        private readonly object _startStopLock = new object();

        private Dictionary<Type, IModule> _moduleDic = null;
        private DependencyGraph<IModule> _graph = null;

        private static async Task<(DependencyGraph<IModule> graph, Dictionary<Type, IModule> moduleDic)> LoadDependencyGraph(IEnumerable<Assembly> assembliesWithModules)
        {
            Dictionary<Type, IModule> dic = new Dictionary<Type, IModule>();

            IModule TryInstantiateAsModule(Type type)
            {
                lock (dic)
                {
                    if (dic.TryGetValue(type, out IModule m))
                    {
                        return m;
                    }

                    if (type.IsAbstract || !typeof(IModule).IsAssignableFrom(type))
                    {
                        return null;
                    }

                    ConstructorInfo c = type.GetConstructor(Type.EmptyTypes);

                    if (c == null)
                    {
                        return null;
                    }

                    m = (IModule)c.Invoke(null);
                    dic[type] = m;
                    return m;
                }
            }

            var query = from ass in assembliesWithModules
                        from type in ass.DefinedTypes
                        let i = TryInstantiateAsModule(type)
                        where i != null
                        let context = new DependencyResolverContext(i)
                        select i.ResolveDependencies(context).ContinueWith(_ => new
                        {
                            Module = i,
                            DependsOn = (from d in context.RequiredDependencies
                                         let di = TryInstantiateAsModule(d)
                                         where di != null
                                         select di).ToList()
                        });

            var qResult = await Task.WhenAll(query).ConfigureAwait(false);
            Dictionary<IModule, List<IModule>> graphDic = qResult.ToDictionary(T => T.Module, T => T.DependsOn);

            return (new DependencyGraph<IModule>(graphDic.Select(T => T.Key), T => graphDic[T]), dic);
        }

        private async Task StartInternal(IEnumerable<Assembly> assembliesWithModules)
        {
            lock (_startStopLock)
            {
                if (_isRunning)
                {
                    throw new InvalidOperationException("Already running.");
                }

                _isRunning = true;
            }

            var data = await LoadDependencyGraph(assembliesWithModules).ConfigureAwait(false);

            _graph = data.graph;
            _moduleDic = data.moduleDic;

            await UseGraph(_graph.Graph, true).ConfigureAwait(false);
        }

        private static Task UseGraph(IReadOnlyList<DependencyItem<IModule>> graph, bool isStartMode)
        {
            Dictionary<IModule, Task> _dic = new Dictionary<IModule, Task>();

            Task UseModule(DependencyItem<IModule> di)
            {
                // Double-check locking.

                if (!_dic.TryGetValue(di.Value, out Task t))
                {
                    lock (_dic)
                    {
                        if (!_dic.TryGetValue(di.Value, out t))
                        {
                            _dic[di.Value] = t = Task.Run(async () =>
                            {
                                var q = from d in di.DependsOn
                                        select UseModule(d);

                                await Task.WhenAll(q).ConfigureAwait(false);

                                if (isStartMode)
                                {
                                    await di.Value.Start().ConfigureAwait(false);
                                }
                                else
                                {
                                    await di.Value.Stop().ConfigureAwait(false);
                                }
                            });
                        }
                    }
                }

                return t;
            }

            var query = from di in graph
                        select UseModule(di);

            return Task.WhenAll(query);
        }

        private async Task StopInternal()
        {
            lock (_startStopLock)
            {
                if (!_isRunning)
                {
                    throw new InvalidOperationException("Not running.");
                }

                _isRunning = false;
            }

            await UseGraph(_graph.ReversedGraph, false).ConfigureAwait(false);
            _graph = null;
            _moduleDic = null;
        }

        public T TryGetModule<T>() where T : IModule => _moduleDic.TryGetValue(typeof(T), out IModule result) ? (T)result : default(T);
    }
}
