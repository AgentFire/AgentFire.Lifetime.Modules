using System;
using System.Threading.Tasks;

namespace AgentFire.Lifetime.Modules
{
    /// <summary>
    /// Has <see cref="LifetimeAsync"/> method which allows you to maintain the full cycle of your app.
    /// </summary>
    public abstract class LifetimeModuleBase : ModuleBase
    {
        private Task _lifetimeTask = null;

        /// <summary>
        /// Don't forget to call this base method when overriding, since it launches the <see cref="LifetimeAsync"/> method.
        /// </summary>
        protected override Task StartInternal()
        {
            _lifetimeTask = MaintainLifetime();
            return base.StartInternal();
        }
        /// <summary>
        /// Don't forget this base method when overriding (in the end), since it finishes its task when the <see cref="LifetimeAsync"/> method finishes.
        /// </summary>
        protected override Task StopInternal() => _lifetimeTask ?? Task.CompletedTask;

        private async Task MaintainLifetime()
        {
            await Task.Yield();

            try
            {
                await LifetimeAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // User cancellation is OK.   
            }
        }

        /// <summary>
        /// You can throw <see cref="OperationCanceledException"/> in this method.
        /// </summary>
        protected abstract Task LifetimeAsync();
    }
}
