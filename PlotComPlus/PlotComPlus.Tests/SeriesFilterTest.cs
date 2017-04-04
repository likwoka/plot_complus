using System;
using System.Collections;
using System.Collections.Generic;
using PlotComPlus.Series;
using PlotComPlus.Logging;
using NUnit.Framework;


namespace PlotComPlus
{

    [TestFixture]
    public class SeriesFilterTest
    {
        [SetUp]
        public void SetUp()
        {
            _filter = new SeriesFilter();
            _filter.AddDesiredSeries("%ProcessorTime", 
                "PlotComPlus.Series.DefaultSeries");
            _filter.AddDesiredSeries("PrivateBytes", 
                "PlotComPlus.Series.MemorySeries");
        }


        [Test]
        public void InitializeOK()
        {
            string header = "Time,CN,PN,%ProcessorTime,%UT,TC,PrivateBytes,WS";
            _filter.InitializeWithHeader(header);
            Assert.AreEqual(2, _filter.Series.Count);

            ISeries s = _filter.Series[0];
            Assert.AreEqual("%ProcessorTime", s.Name);
            Assert.AreEqual(3, s.Position);

            s = _filter.Series[1];
            Assert.AreEqual("PrivateBytes", s.Name);
            Assert.AreEqual(6, s.Position);
        }


        [Test]
        [ExpectedException(typeof(SeriesNotFoundException))]
        public void InitializeFailed()
        {
            string header = "Time,,,,,,";
            _filter.InitializeWithHeader(header);
        }


        private SeriesFilter _filter;
    }
}
