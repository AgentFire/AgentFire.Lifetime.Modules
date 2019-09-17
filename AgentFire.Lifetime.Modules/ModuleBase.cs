using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AgentFire.Lifetime.Modules
{
    /// <summary>
    /// Base class for <see cref="IModule"/> entity.
    /// </summary>
    public abstract class ModuleBase : IModule
    {
        /// <summary>
        /// True if the module is started.
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// Base method iterates over <see cref="ModuleDependencyAttribute"/> attributes in current type and calls <see cref="IDependencyResolverContext.RequireDependency"/> for them.
        /// </summary>
        public virtual Task ResolveDependencies(IDependencyResolverContext context, CancellationToken token = default)
        {
            foreach (var attr in GetType().GetCustomAttributes<ModuleDependencyAttribute>(true))
            {
                context.RequireDependency(attr.DependsOn);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// The only constructor.
        /// </summary>
        protected ModuleBase() { }

        /// <summary>
        /// Your startup implementation. Base implementation returns <see cref="Task.CompletedTask"/>
        /// </summary>
        protected virtual Task StartInternal(CancellationToken token) => Task.CompletedTask;

        /// <summary>
        /// Your stop implementation. Base implementation returns <see cref="Task.CompletedTask"/>
        /// </summary>
        protected virtual Task StopInternal(CancellationToken token) => Task.CompletedTask;

        /// <summary>
        /// Starts the module up
        /// </summary>
        public async Task Start(CancellationToken token = default)
        {
            await StartInternal(token).ConfigureAwait(false);

            IsRunning = true;
        }

        /// <summary>
        /// Stops the module.
        /// </summary>
        public async Task Stop(CancellationToken token = default)
        {
            await StopInternal(token).ConfigureAwait(false);

            IsRunning = false;
        }
    }
}
