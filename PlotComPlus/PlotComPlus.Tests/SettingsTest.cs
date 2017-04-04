using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;


namespace PlotComPlus
{

    [TestFixture]
    public class SettingsTest
    {
        [Test]
        public void ExtractDesiredSeriesOK()
        {
            NameValueCollection appSettings = new NameValueCollection();
            appSettings["Column.%ProcessorTime"] = "DefaultSeries";
            appSettings["Column.PrivateBytes"] = "MemorySeries";
            appSettings["Some Other Key"] = "Some Other Value";

            List<string[]> actual = Settings.DesiredSeries(appSettings);
            Assert.AreEqual(2, actual.Count);
            
            string[] processorTime = actual[0];
            Assert.AreEqual("%ProcessorTime", processorTime[0]);
            Assert.AreEqual("PlotComPlus.Series.DefaultSeries", processorTime[1]);

            string[] privateBytes = actual[1];
            Assert.AreEqual("PrivateBytes", privateBytes[0]);
            Assert.AreEqual("PlotComPlus.Series.MemorySeries", privateBytes[1]);
        }

    }
}
