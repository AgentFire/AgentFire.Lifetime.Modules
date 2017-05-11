using System.Reflection;

namespace AgentFire.Lifetime.Modules
{
    public abstract class ModuleBase : IModule
    {
        public virtual bool AutoStart { get; protected set; } = true;
        public virtual bool IsRunning { get; protected set; } = false;

        public virtual void Initialize(IModuleStartContext context)
        {
            foreach (var attr in GetType().GetCustomAttributes<ModuleDependencyAttribute>(true))
            {
                context.RequireDependency(attr.DependsOn);
            }
        }

        protected ModuleBase()
        {
        }

        public virtual void Restart()
        {
            Stop();
            Start();
        }

        public virtual void Start()
        {
            if (IsRunning)
            {
                return;
            }

            StartInternal();
            IsRunning = true;
        }
        public virtual void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            StopInternal();
            IsRunning = false;
        }

        protected virtual void StartInternal() { }
        protected virtual void StopInternal() { }
    }
}
