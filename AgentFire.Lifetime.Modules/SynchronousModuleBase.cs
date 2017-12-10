using System.Threading.Tasks;

namespace AgentFire.Lifetime.Modules
{
    public abstract class SynchronousModuleBase : ModuleBase
    {
        protected override sealed Task StartInternal()
        {
            StartSync();
            return base.StartInternal();
        }
        protected override sealed Task StopInternal()
        {
            StopSync();
            return base.StopInternal();
        }

        protected virtual void StartSync() { }
        protected virtual void StopSync() { }
    }
}
