using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgentFire.Lifetime.Modules.Test
{
    [ModuleDependency(typeof(M3))]
    public sealed class M1 : ModuleBase
    {
        protected override void StartInternal()
        {
            Console.WriteLine("M1 (of M3) start");
        }
        public override void Stop()
        {
            Console.WriteLine("M1 (of M3) stop");
        }
    }

    [ModuleDependency(typeof(M4))]
    public sealed class M2 : ModuleBase
    {
        protected override void StartInternal()
        {
            Console.WriteLine("M2 (of M4) start");
        }
        public override void Stop()
        {
            Console.WriteLine("M2 (of M4) stop");
        }
    }
    [ModuleDependency(typeof(M2))]
    public sealed class M3 : ModuleBase
    {
        protected override void StartInternal()
        {
            Console.WriteLine("M3 (of M2) start");
        }
        public override void Stop()
        {
            Console.WriteLine("M3 (of M2) stop");
        }
    }

    [ModuleDependency(typeof(M4))]
    public sealed class M4 : ModuleBase
    {
        protected override void StartInternal()
        {
            Console.WriteLine("M4 (of M4) start");
        }
        public override void Stop()
        {
            Console.WriteLine("M4 (of M4) stop");
        }

        public override bool IsRunning => true;

        //internal M4() { }
    }

    public static class Program
    {
        internal static void Main(string[] args)
        {
            ModuleManager m = new ModuleManager();

            m.Start();

            var m4 = m.TryGetModule<M4>();

            m.Stop();
        }
    }
}
