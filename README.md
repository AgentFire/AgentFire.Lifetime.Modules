# AgentFire.Lifetime.Modules
Provides a way for an app to use module-based (Start/Stop) singleton instances.

Your modules:

    class Foo : ModuleBase
    {
        protected override async Task StartInternal()
        {
            await Task.Delay(1000);
            Console.WriteLine("Foo started.");
        }
        protected override Task StopInternal()
        {
            Console.WriteLine("Foo stopped.");
            return Task.CompletedTask;
        }
    }

    [ModuleDependency(typeof(Foo))]
    class Bar : ModuleBase
    {
        protected override Task StartInternal()
        {
            Console.WriteLine("Bar started.");
            return Task.CompletedTask;
        }
        protected override Task StopInternal()
        {
            Console.WriteLine("Bar stopped.");
            return Task.CompletedTask;
        }
    }
    
Usage:

    ModuleManager m = new ModuleManager();
    
    await m.Start();
    
    // After the start call the system will find any modules that you have (default - calling assembly),
    // sort them in a dependency graph and then fire Start() on all of them
    // in the most optimal (also async) way, respecting all the dependencies you've specified.
    
    // This tool is StackOverflow-free and in case of a circular dependency; it will instead dead-lock :)
    // Don't do circular dependencies, thank you.
    
    await m.Stop();
    
    // Guaranteed result:
    // Foo started.
    // Bar started.
    // Bar stopped.
    // Foo stopped.
