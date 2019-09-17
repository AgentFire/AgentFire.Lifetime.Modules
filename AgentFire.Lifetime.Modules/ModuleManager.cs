using AgentFire.Lifetime.Modules.Components;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// <remarks>
    /// Exceptions thrown inside Start or Stop methods may leave your Modules in an undetermined states. Get rid of them in the dev stage.
    /// </remarks>
    public sealed class ModuleManager
    {
        /// <summary>
        /// Allows consumer to resolve <see cref="IModule"/> instances in their custom ways.
        /// </summary>
        public IModuleResolver ModuleResolver { get; set; } = new DefaultModuleResolver();

        /// <summary>
        /// Starts all Auto-Start modules from a calling assembly.
        /// </summary>
        public Task Start(CancellationToken token = default)
        {
            return Start(Assembly.GetCallingAssembly(), token);
        }
        /// <summary>
        /// Starts all Auto-Start modules from a calling assembly.
        /// </summary>
        public Task Start(CancellationToken token = default, params Type[] modules)
        {
            return StartWithModules(modules, token);
        }
        /// <summary>
        /// Starts all Auto-Start modules from a specified assembly.
        /// </summary>
        public Task Start(Assembly assemblyWithModules, CancellationToken token = default)
        {
            return Start(new[] { assemblyWithModules }, token);
        }
        /// <summary>
        /// Starts all Auto-Start modules from a specified set of assemblies.
        /// </summary>
        public Task Start(CancellationToken token = default, params Assembly[] assembliesWithModules)
        {
            return StartWithAssemblies(assembliesWithModules, token);
        }
        /// <summary>
        /// Starts all Auto-Start modules from a specified set of assemblies.
        /// </summary>
        public Task Start(IEnumerable<Assembly> assembliesWithModules, CancellationToken token = default)
        {
            return StartWithAssemblies(assembliesWithModules, token);
        }

        /// <summary>
        /// Stops all running modules.
        /// </summary>
        public Task Shutdown(CancellationToken token = default)
        {
            return StopInternal(token);
        }

        /// <summary>
        /// Indicates whether the manager is in Running state."
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// Indicates where the manager is starting.
        /// </summary>
        public bool IsStarting { get; private set; } = false;

        /// <summary>
        /// Indicates where the manager is stopping.
        /// </summary>
        public bool IsStopping { get; private set; } = false;

        private DependencyGraph<IModule> _graph = null;

        /// <exception cref="NonAcyclicGraphException" />
        /// <exception cref="KeyNotFoundException" />
        private async Task<DependencyGraph<IModule>> LoadDependencyGraph(IEnumerable<Type> moduleTypes, CancellationToken token)
        {
            Dictionary<Type, IModule> dic = new Dictionary<Type, IModule>();

            var query = from type in moduleTypes
                        let i = ModuleResolver.TryGet(type)
                        where i != null
                        let context = new DependencyResolverContext(i)
                        select i.ResolveDependencies(context, token).ContinueWith(_ => new
                        {
                            Module = i,
                            DependsOn = (from d in context.RequiredDependencies
                                         let di = ModuleResolver.TryGet(d)
                                         where di != null
                                         select di).ToList()
                        });

            var qResult = await Task.WhenAll(query).ConfigureAwait(false);

            Dictionary<IModule, List<IModule>> graphDic = qResult.ToDictionary(T => T.Module, T => T.DependsOn);

            return new DependencyGraph<IModule>(graphDic.Select(T => T.Key), T => graphDic[T]);
        }

        private Task StartWithAssemblies(IEnumerable<Assembly> assembliesWithModules, CancellationToken token)
        {
            return StartWithModules(assembliesWithModules.SelectMany(T => T.DefinedTypes), token);
        }
        private async Task StartWithModules(IEnumerable<Type> moduleTypes, CancellationToken token)
        {
            IsStarting = true;

            try
            {
                try
                {
                    _graph = await LoadDependencyGraph(moduleTypes, token).ConfigureAwait(false);
                }
                catch (NonAcyclicGraphException ex)
                {
                    throw new InvalidOperationException("Your dependency graph is circular (cyclic).", ex);
                }
                catch (KeyNotFoundException ex)
                {
                    throw new InvalidOperationException("Your dependency graph is not complete.", ex);
                }

                await UseGraph(_graph.Forward, T => T.Start(token)).ConfigureAwait(false);
            }
            catch
            {
                ModuleResolver.Reset();
                throw;
            }
            finally
            {
                IsStarting = false;
            }

            IsRunning = true;
        }

        private static Task UseGraph(IReadOnlyCollection<DependencyItem<IModule>> acyclicGraph, Func<IModule, Task> useAction)
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

                                await useAction(di.Value).ConfigureAwait(false);
                            });
                        }
                    }
                }

                return t;
            }

            var query = from di in acyclicGraph
                        select UseModule(di);

            return Task.WhenAll(query);
        }

        private async Task StopInternal(CancellationToken token)
        {
            IsStopping = true;

            try
            {
                await UseGraph(_graph.Backward, T => T.Stop(token)).ConfigureAwait(false);
            }
            finally
            {
                ModuleResolver.Reset();
                _graph = null;

                IsStopping = false;
                IsRunning = false;
            }
        }

        /// <summary>
        /// Returns a module instance, or null. Can be run from within your Start methods.
        /// </summary>
        public T TryGetModule<T>() where T : IModule
        {
            if (!IsStarting && !IsRunning && !IsStopping)
            {
                return default;
            }

            return ModuleResolver.TryGet<T>();
        }
    }
}
