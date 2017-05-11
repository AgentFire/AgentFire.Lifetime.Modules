namespace AgentFire.Lifetime.Modules
{
    public interface IModule
    {
        void Initialize(IModuleStartContext context);

        void Start();
        void Stop();
        void Restart();

        bool IsRunning { get; }
        /// <summary>
        /// Does it start when <see cref="ModuleManager.Start()"/> is called?
        /// </summary>
        bool AutoStart { get; }
    }
}
