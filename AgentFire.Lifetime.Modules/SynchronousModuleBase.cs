using System.Threading;
using System.Threading.Tasks;

namespace AgentFire.Lifetime.Modules
{
    /// <summary>
    /// Non async base module class.
    /// </summary>
    public abstract class SynchronousModuleBase : ModuleBase
    {
        /// <summary>
        /// Calls <see cref="StartSync"/>.
        /// </summary>
        protected override sealed Task StartInternal(CancellationToken token)
        {
            StartSync(token);
            return base.StartInternal(token);
        }

        /// <summary>
        /// Calls <see cref="StopSync"/>.
        /// </summary>
        protected override sealed Task StopInternal(CancellationToken token)
        {
            StopSync(token);
            return base.StopInternal(token);
        }

        /// <summary>
        /// Your synchronous start method.
        /// </summary>
        protected virtual void StartSync(CancellationToken token) { }

        /// <summary>
        /// Your synchronous stop method.
        /// </summary>
        protected virtual void StopSync(CancellationToken token) { }
    }
}
