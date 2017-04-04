using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace PlotComPlus
{

    [TestFixture]
    public class ProcessTest
    {

        [Test]
        public void SmokeTest()
        {
            Process p = new Process("RBCWSSession(1234)", "test_log.txt");

            Assert.AreEqual("RBCWSSession", p.ProcessName);
            Assert.AreEqual(1234, p.ProcessId);
            Assert.AreEqual("test_log.txt", p.LogFileName);

            // During runtime, the value will be converted from string to
            // double by Series.Transform().
            p.Set("PrivateBytes", 1000);
            p.Set("PrivateBytes", 2000);
            p.Set("PrivateBytes", 3000);

            IList<double> data = p.Get("PrivateBytes");
            Assert.AreEqual(3, data.Count);
            Assert.AreEqual(true, data.Contains(1000f));
            Assert.AreEqual(true, data.Contains(2000f));
            Assert.AreEqual(true, data.Contains(3000f));
        }

        [Test]
        public void AbnormalCase()
        {
            Process p = new Process("()", "test_log.txt");

            Assert.AreEqual("", p.ProcessName);
            Assert.AreEqual(0, p.ProcessId);
            Assert.AreEqual("test_log.txt", p.LogFileName);
        }
    }
}
