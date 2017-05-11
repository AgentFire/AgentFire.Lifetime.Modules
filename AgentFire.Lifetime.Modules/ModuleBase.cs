using System.Reflection;

namespace AgentFire.Lifetime.Modules
{
    public abstract class ModuleBase : IModule
    {
        public virtual bool AutoStart => true;
        public virtual bool IsRunning { get; private set; } = false;

        public virtual void Initialize(IModuleStartContext context)
        {
            foreach (var attr in GetType().GetCustomAttributes<ModuleDependencyAttribute>())
            {
                context.RequireDependency(attr.DependsOn);
            }
        }

        protected ModuleBase()
        {
        }

        public virtual void Restart()
        {
            if (IsRunning)
            {
                Stop();
            }

            Start();
        }

        public virtual void Start()
        {
            StartInternal();
            IsRunning = true;
        }

        public virtual void Stop()
        {
            StopInternal();
            IsRunning = false;
        }

        protected virtual void StartInternal() { }
        protected virtual void StopInternal() { }
    }
}
