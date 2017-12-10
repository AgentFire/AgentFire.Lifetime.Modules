using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFire.Lifetime.Modules.Test
{
    public abstract class ModuleTestBase : ModuleBase
    {
        protected override sealed async Task StartInternal()
        {
            await Task.Delay(600);
            Console.WriteLine($"{GetType().Name} started.");
        }
        protected override sealed async Task StopInternal()
        {
            await Task.Delay(600);
            Console.WriteLine($"{GetType().Name} stopped.");
        }
    }

    //[ModuleDependency(typeof(M3))]
    public sealed class M1 : ModuleTestBase { }

    [ModuleDependency(typeof(M1))]
    public sealed class M2 : ModuleTestBase { }

    [ModuleDependency(typeof(M1))]
    public sealed class M3 : ModuleTestBase { }

    [ModuleDependency(typeof(M2))]
    [ModuleDependency(typeof(M3))]
    public sealed class M4 : ModuleTestBase { }

    [ModuleDependency(typeof(M5))]
    public sealed class M5 : ModuleTestBase { }

    [ModuleDependency(typeof(M4))]
    public sealed class M6 : ModuleTestBase { }

    [ModuleDependency(typeof(M4))]
    public sealed class M7 : ModuleTestBase { }

    [ModuleDependency(typeof(M4))]
    public sealed class M8 : ModuleTestBase { }

    [ModuleDependency(typeof(M6))]
    [ModuleDependency(typeof(M7))]
    [ModuleDependency(typeof(M8))]
    [ModuleDependency(typeof(M1))]
    [ModuleDependency(typeof(M5))]
    public sealed class M9 : ModuleTestBase { }

    [ModuleDependency(typeof(S2))]
    public sealed class S1 : ModuleTestBase { }
    //[ModuleDependency(typeof(S1))] // Circular dependency deadlock test. Should just hang there.
    public sealed class S2 : ModuleTestBase { }
    
    public static class Program
    {
        public static ModuleManager m = new ModuleManager();

        internal static async Task Main(string[] args)
        {
            await m.Start();

            Console.WriteLine("Any key to stop.");
            Console.ReadKey(true);
            var m4 = m.TryGetModule<M4>();

            await m.Shutdown();

            Console.WriteLine("Any key to exit?..");
            Console.ReadKey(true);
        }
    }

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
}
