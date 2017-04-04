using System;
using System.Collections.Generic;
using NUnit.Framework;


namespace PlotComPlus
{
    [TestFixture]
    public class ConvertorTest
    {
        [Test]
        public void TimestampsToTicksOK()
        {
            List<string> timestamps = new List<string>();
            timestamps.Add("4/4/2007 3:32:05 PM");
            timestamps.Add("4/4/2007 3:32:06 PM");
            timestamps.Add("4/4/2007 3:33 PM");
            timestamps.Add("4/4/2007 4:00:01 PM");
            timestamps.Add("4/5/2007");

            double[] actual = Convertor.TimestampsToTicks(timestamps);
            
            // Round up to 3 decimals.
            double[] expected = new double[] {
                0, 
                0.017,
                0.917,
                27.933,
                507.917
            };

            for (int i = 0; i < actual.Length; i++)
            {
                Assert.AreEqual(expected[i], Math.Round(actual[i], 3));
            }
        }

    }
}
