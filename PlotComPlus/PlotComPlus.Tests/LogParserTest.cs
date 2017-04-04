using System;
using System.IO;
using System.Collections.Generic;

using NUnit.Framework;
using PlotComPlus.Logging;
using PlotComPlus.ProcessFilters;


namespace PlotComPlus
{
    [TestFixture]
    public class LogParserTest
    {

        [Test]
        public void SmokeTest()
        {
            // Instantiating the objects.
            ILogger logger = new ConsoleLogger();

            SeriesFilter sf = new SeriesFilter();
            sf.AddDesiredSeries("%ProcessorTime", 
                "PlotComPlus.Series.DefaultSeries");
            sf.AddDesiredSeries("PrivateBytes", 
                "PlotComPlus.Series.MemorySeries");

            IProcessFilter pf = new ProcessFilter("session,userinfo");

            LogParser parser = new LogParser(pf, sf, logger);

            // Setting up the test log file.
            string path = Path.GetTempFileName();
            string[] content = new string[] {
                "Microsoft (R) Windows Script Host Version 5.6", 
                "Copyright (C) Microsoft Corporation 1996-2001. All rights reserved.",
                "",
                "Time,CN,PN(ID),%ProcessorTime,%UT,TC,PrivateBytes,WS",
                "4/3/2007 10:00:37 AM,.,Idle(0),100,0,8,0,16384",
                "4/3/2007 10:00:37 AM,.,System(4),0,0,110,28672,28672",
                "4/3/2007 10:00:37 AM,.,RBCWSSession(6520),23,0,31,30208000,37974016",
                ",,,,,,,",
                "4/3/2007 10:00:42 AM,.,RBCWSSession(6520),24,0,31,25071616,34078720",
                "4/3/2007 10:00:42 AM,.,RBCWSUserInfo(10496),11,0,32,13414400,21475328",
                "4/4/2007,.,RBCWSSession(6520),25,0,31,25214976,34222080"
            };
            File.WriteAllLines(path, content);

            // Now we run the smoke test of parsing 1 log file containing
            // 2 processes (RBCWSSession, RBCWSUserInfo) we are interested,
            // with the 2 metrics above that we are interested in.
            List<Process> processes = parser.ParseLogs(path);

            Assert.AreEqual(2, processes.Count);

            Process session = processes[0];
            Assert.AreEqual("RBCWSSession", session.ProcessName);
            Assert.AreEqual(3, session.TimeSeries.Count);

            IList<double> data = session.Get("%ProcessorTime");
            Assert.AreEqual(3, data.Count);
            Assert.AreEqual(true, data.Contains(23f));
            Assert.AreEqual(true, data.Contains(24f));
            Assert.AreEqual(true, data.Contains(25f));

            data = session.Get("PrivateBytes");
            Assert.AreEqual(3, data.Count);
            Assert.AreEqual(true, data.Contains(30.208d));
            Assert.AreEqual(true, data.Contains(25.071616d));
            Assert.AreEqual(true, data.Contains(25.214976d));

            Process userinfo = processes[1];
            Assert.AreEqual("RBCWSUserInfo", userinfo.ProcessName);
            Assert.AreEqual(1, userinfo.TimeSeries.Count);

            data = userinfo.Get("%ProcessorTime");
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(true, data.Contains(11d));

            data = userinfo.Get("PrivateBytes");
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(true, data.Contains(13.4144d));

            // Clean up.
            File.Delete(path);
        }
    }
}
