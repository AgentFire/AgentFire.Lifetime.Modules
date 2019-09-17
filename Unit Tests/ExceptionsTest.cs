using AgentFire.Lifetime.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Unit_Tests
{
    [TestClass]
    public class ExceptionsTest
    {
        private readonly ModuleManager _manager = new ModuleManager();

        #region SameDependency

        [ModuleDependency(typeof(S1))]
        private sealed class S1 : ModuleBase { }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SameDependency()
        {
            await _manager.Start(default, typeof(S1));
        }

        #endregion
        #region NotAModule

        [ModuleDependency(typeof(int))]
        private sealed class F1 : ModuleBase { }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task NotAModule()
        {
            await _manager.Start(default, typeof(F1));
        }

        #endregion
        #region CircularDependency

        [ModuleDependency(typeof(C2))]
        private sealed class C1 : ModuleBase { }

        [ModuleDependency(typeof(C1))]
        private sealed class C2 : ModuleBase { }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CircularDependency1()
        {
            await _manager.Start(default, typeof(C1), typeof(C2));
        }

        [ModuleDependency(typeof(CD3))]
        private sealed class CD1 : ModuleBase { }

        [ModuleDependency(typeof(CD1))]
        private sealed class CD2 : ModuleBase { }

        [ModuleDependency(typeof(CD2))]
        private sealed class CD3 : ModuleBase { }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CircularDependency2()
        {
            await _manager.Start(default, typeof(CD1), typeof(CD2), typeof(CD3));
        }

        #endregion

        #region ExceptionPropagation

        private sealed class P1 : ModuleBase
        {
            protected override async Task StartInternal(CancellationToken token)
            {
                await base.StartInternal(token);
                throw new NotImplementedException();
            }
        }

        private sealed class P2 : ModuleBase
        {
            protected override async Task StopInternal(CancellationToken token)
            {
                await base.StopInternal(token);
                throw new NotImplementedException();
            }
        }

        private sealed class P3 : LifetimeModuleBase
        {
            protected override Task LifetimeAsync(CancellationToken token)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public async Task ExceptionPropagation1()
        {
            Assert.IsFalse(_manager.IsRunning);

            await _manager.Start(default, typeof(P1));

            Assert.IsFalse(_manager.IsRunning);
        }

        [TestMethod]
        public async Task ExceptionPropagation2()
        {
            await _manager.Start(default, typeof(P2));

            Assert.IsTrue(_manager.IsRunning);

            await Assert.ThrowsExceptionAsync<NotImplementedException>(() => _manager.Shutdown());

            Assert.IsFalse(_manager.IsRunning);
        }

        [TestMethod]
        public async Task ExceptionPropagation3()
        {
            await _manager.Start(default, typeof(P3));

            Assert.IsTrue(_manager.IsRunning);

            await Assert.ThrowsExceptionAsync<NotImplementedException>(() => _manager.Shutdown());

            Assert.IsFalse(_manager.IsRunning);
        }

        #endregion
    }
}
