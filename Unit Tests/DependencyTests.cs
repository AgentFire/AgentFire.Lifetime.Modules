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
    public class DependencyTests
    {
        #region DefaultBehaviour

        private class D1 : ModuleBase { }

        [TestMethod]
        public async Task DefaultBehaviour()
        {
            ModuleManager m = new ModuleManager();

            Assert.IsNull(m.TryGetModule<D1>());

            await m.Start(default, typeof(D1));

            D1 one = m.TryGetModule<D1>();
            D1 two = m.TryGetModule<D1>();

            Assert.IsNotNull(one);
            Assert.IsNotNull(two);
            Assert.AreEqual(one, two);

            await m.Shutdown();

            Assert.IsNull(m.TryGetModule<D1>());

            await m.Start(default, typeof(D1));

            D1 three = m.TryGetModule<D1>();
            D1 four = m.TryGetModule<D1>();

            Assert.IsNotNull(three);
            Assert.IsNotNull(four);
            Assert.AreEqual(three, four);
            Assert.AreNotEqual(one, three);

            await m.Shutdown();
        }

        #endregion
        #region DependentReferenceTest

        private static readonly AsyncLocal<ModuleManager> _am = new AsyncLocal<ModuleManager>();

        private class DR1 : ModuleBase
        {
            protected override Task StartInternal(CancellationToken token)
            {
                Assert.IsNotNull(_am.Value.TryGetModule<DR2>());
                Assert.IsFalse(_am.Value.TryGetModule<DR2>().IsRunning);

                return base.StartInternal(token);
            }
        }

        [ModuleDependency(typeof(DR1))]
        private class DR2 : ModuleBase
        {
            protected override Task StartInternal(CancellationToken token)
            {
                Assert.IsNotNull(_am.Value.TryGetModule<DR1>());
                Assert.IsTrue(_am.Value.TryGetModule<DR1>().IsRunning);

                return base.StartInternal(token);
            }
        }


        [TestMethod]
        public async Task DependentReferenceTest()
        {
            _am.Value = new ModuleManager();

            await _am.Value.Start(default, typeof(DR1), typeof(DR2));

            await _am.Value.Shutdown();

            _am.Value = null;
        }

        #endregion
    }
}
