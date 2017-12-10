using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AgentFire.Lifetime.Modules
{
    public abstract class ModuleBase : IModule
    {
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// Base method iterates over <see cref="ModuleDependencyAttribute"/> attributes in current type and calls <see cref="IDependencyResolverContext.RequireDependency"/> for them.
        /// </summary>
        public virtual Task ResolveDependencies(IDependencyResolverContext context)
        {
            foreach (var attr in GetType().GetCustomAttributes<ModuleDependencyAttribute>(true))
            {
                context.RequireDependency(attr.DependsOn);
            }

            return Task.CompletedTask;
        }

        protected ModuleBase() { }

        private readonly object _startStopLock = new object();
        private bool _isStartingOrStopping = false;

        protected virtual Task StartInternal() => Task.CompletedTask;
        protected virtual Task StopInternal() => Task.CompletedTask;

        public async Task Start()
        {
            lock (_startStopLock)
            {
                if (IsRunning && _isStartingOrStopping)
                {
                    throw new InvalidOperationException("Your previous call on this method has not yet finished.");
                }

                _isStartingOrStopping = true;
            }

            string typename = GetType().Name;
            await StartInternal().ConfigureAwait(false);

            IsRunning = true;
            _isStartingOrStopping = false;
        }
        public async Task Stop()
        {
            lock (_startStopLock)
            {
                if (!IsRunning && _isStartingOrStopping)
                {
                    throw new InvalidOperationException("Your previous call on this method has not yet finished.");
                }

                _isStartingOrStopping = true;
            }

            await StopInternal().ConfigureAwait(false);

            IsRunning = false;
            _isStartingOrStopping = false;
        }
    }
}
