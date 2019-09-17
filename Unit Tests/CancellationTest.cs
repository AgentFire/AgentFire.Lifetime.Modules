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
    public class CancellationTest
    {
        private readonly ModuleManager _manager = new ModuleManager();

        private sealed class C1 : ModuleBase
        {
            protected override async Task StartInternal(CancellationToken token)
            {
                await Task.Delay(200, token);
                Assert.Fail("Shown't have come here.");
            }
        }

        [TestMethod]
        public async Task CancelStartup()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Task t = _manager.Start(cts.Token, typeof(C1));

                await Task.Delay(100);
                cts.Cancel();

                await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => t);

                Assert.IsFalse(_manager.IsRunning);
            }
        }

        private sealed class C2 : ModuleBase
        {
            protected override async Task StopInternal(CancellationToken token)
            {
                await Task.Delay(200, token);
                Assert.Fail("Shown't have come here.");
            }
        }

        [TestMethod]
        public async Task CancelShutdown()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                await _manager.Start(cts.Token, typeof(C2));

                Task t = _manager.Shutdown(cts.Token);

                await Task.Delay(100);
                cts.Cancel();

                await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => t);

                Assert.IsFalse(_manager.IsRunning);
            }
        }
    }
}
