using System.Threading;
using System.Threading.Tasks;

namespace AgentFire.Lifetime.Modules
{
    /// <summary>
    /// Defines a module.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Allows to define your dependencies on other modules.
        /// </summary>
        Task ResolveDependencies(IDependencyResolverContext context, CancellationToken token);

        /// <summary>
        /// Starts the module up.
        /// </summary>
        Task Start(CancellationToken token);

        /// <summary>
        /// Stops the module.
        /// </summary>
        Task Stop(CancellationToken token);
    }
}
