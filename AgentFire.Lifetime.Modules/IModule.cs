using System.Threading.Tasks;

namespace AgentFire.Lifetime.Modules
{
    public interface IModule
    {
        Task ResolveDependencies(IDependencyResolverContext context);

        Task Start();
        Task Stop();
    }
}
