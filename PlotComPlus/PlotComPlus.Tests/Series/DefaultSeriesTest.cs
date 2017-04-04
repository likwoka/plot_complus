using System;
using NUnit.Framework;
using PlotComPlus.Series;


namespace PlotComPlus
{

    [TestFixture]
    [Platform("NET-2.0")]
    public class DefaultSeriesTest
    {
        [Test]
        public void Transform()
        {
            DefaultSeries s = new DefaultSeries("%ProcessorTime", 3);
            Assert.AreEqual(10f, s.Transform("10"));
            Assert.AreEqual(0f, s.Transform(""));
            Assert.AreEqual(0f, s.Transform(null));
        }
    }
}
