using System;
using System.Threading;
using System.Threading.Tasks;

namespace AgentFire.Lifetime.Modules
{
    /// <summary>
    /// Has <see cref="LifetimeAsync"/> method which allows you to maintain the full cycle of your app.
    /// </summary>
    public abstract class LifetimeModuleBase : ModuleBase
    {
        private Task _lifetimeTask = null;
        private CancellationTokenSource _cancelSource;

        /// <summary>
        /// Don't forget to call this base method when overriding, since it launches the <see cref="LifetimeAsync"/> method and returns immediately after.
        /// </summary>
        protected override Task StartInternal(CancellationToken token)
        {
            _cancelSource = new CancellationTokenSource();

            _lifetimeTask = MaintainLifetime(_cancelSource.Token);

            return base.StartInternal(token);
        }

        /// <summary>
        /// Don't forget to call base method at the end of your method when (and only if) overriding.
        /// </summary>
        protected override async Task StopInternal(CancellationToken token)
        {
            _cancelSource.Cancel();

            try
            {
                await _lifetimeTask.ConfigureAwait(false);
            }
            finally
            {
                _cancelSource.Dispose();

                _lifetimeTask?.Dispose();
                _lifetimeTask = null;
            }
        }

        private async Task MaintainLifetime(CancellationToken token)
        {
            try
            {
                await LifetimeAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == token)
            {
                // The cancellation is handled.
            }
        }

        /// <summary>
        /// You can throw <see cref="OperationCanceledException"/> in this method, but you must include the <see cref="CancellationToken"/> parameter inside the exception object.
        /// </summary>
        protected abstract Task LifetimeAsync(CancellationToken token);
    }
}
