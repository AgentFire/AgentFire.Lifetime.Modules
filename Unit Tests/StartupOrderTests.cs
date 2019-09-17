using AgentFire.Lifetime.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Unit_Tests
{
    [TestClass]
    public class StartupOrderTests
    {
        private static readonly Random _r = new Random();
        private static readonly AsyncLocal<List<string>> _testBoxUpStorage = new AsyncLocal<List<string>>();
        private static readonly AsyncLocal<List<string>> _testBoxDownStorage = new AsyncLocal<List<string>>();

        public abstract class ModuleTestBase : ModuleBase
        {
            protected override sealed async Task StartInternal(CancellationToken token)
            {
                await Task.Delay(_r.Next(0, 500));

                lock (_testBoxUpStorage.Value)
                {
                    _testBoxUpStorage.Value.Add(GetType().Name);
                }
            }
            protected override sealed async Task StopInternal(CancellationToken token)
            {
                await Task.Delay(_r.Next(0, 500));

                lock (_testBoxDownStorage.Value)
                {
                    _testBoxDownStorage.Value.Add(GetType().Name);
                }
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

        //[ModuleDependency(typeof(M5))]
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

        private readonly ModuleManager _manager = new ModuleManager();

        [TestMethod]
        public async Task OrderAndDependency()
        {
            _testBoxUpStorage.Value = new List<string>();
            _testBoxDownStorage.Value = new List<string>();

            Assert.IsFalse(_manager.IsRunning);
            await _manager.Start(default, typeof(M1), typeof(M2), typeof(M3), typeof(M4), typeof(M5), typeof(M6), typeof(M7), typeof(M8), typeof(M9));
            Assert.IsTrue(_manager.IsRunning);

            bool UpMoreThan<T1, T2>() => _testBoxUpStorage.Value.IndexOf(typeof(T1).Name) > _testBoxUpStorage.Value.IndexOf(typeof(T2).Name);

            Assert.IsTrue(UpMoreThan<M2, M1>());
            Assert.IsTrue(UpMoreThan<M3, M1>());
            Assert.IsTrue(UpMoreThan<M4, M2>());
            Assert.IsTrue(UpMoreThan<M4, M3>());
            Assert.IsTrue(UpMoreThan<M6, M4>());
            Assert.IsTrue(UpMoreThan<M7, M4>());
            Assert.IsTrue(UpMoreThan<M8, M4>());

            Assert.IsTrue(UpMoreThan<M9, M1>());
            Assert.IsTrue(UpMoreThan<M9, M5>());
            Assert.IsTrue(UpMoreThan<M9, M6>());
            Assert.IsTrue(UpMoreThan<M9, M7>());
            Assert.IsTrue(UpMoreThan<M9, M8>());

            await _manager.Shutdown();
            Assert.IsFalse(_manager.IsRunning);

            bool DownMoreThan<T1, T2>() => _testBoxDownStorage.Value.IndexOf(typeof(T1).Name) < _testBoxDownStorage.Value.IndexOf(typeof(T2).Name);

            Assert.IsTrue(DownMoreThan<M2, M1>());
            Assert.IsTrue(DownMoreThan<M3, M1>());
            Assert.IsTrue(DownMoreThan<M4, M2>());
            Assert.IsTrue(DownMoreThan<M4, M3>());
            Assert.IsTrue(DownMoreThan<M6, M4>());
            Assert.IsTrue(DownMoreThan<M7, M4>());
            Assert.IsTrue(DownMoreThan<M8, M4>());

            Assert.IsTrue(DownMoreThan<M9, M1>());
            Assert.IsTrue(DownMoreThan<M9, M5>());
            Assert.IsTrue(DownMoreThan<M9, M6>());
            Assert.IsTrue(DownMoreThan<M9, M7>());
            Assert.IsTrue(DownMoreThan<M9, M8>());

            _testBoxUpStorage.Value = null;
            _testBoxDownStorage.Value = null;
        }
    }
}
