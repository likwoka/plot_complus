using System;
using NUnit.Framework;
using PlotComPlus.Series;


namespace PlotComPlus
{
    [TestFixture]
    [Platform("NET-2.0")]
    public class MemorySeriesTest
    {

        [Test]
        public void Transform()
        {
            MemorySeries s = new MemorySeries("PrivateBytes", 6);
            Assert.AreEqual(12.345678d, s.Transform("12345678")); // 12 MB.
            Assert.AreEqual(0f, s.Transform("0"));
            Assert.AreEqual(0f, s.Transform(null));
        }

    }
}
