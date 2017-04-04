using System;
using PlotComPlus;
using NUnit.Framework;
using PlotComPlus.ProcessFilters;

namespace PlotComPlus
{
    [TestFixture]
    public class ProcessFilterTest
    {

        [Test]
        public void ContainsWithSinglePattern()
        {
            ProcessFilter f = new ProcessFilter("session");
            Assert.AreEqual(true, f.Contains("RBCWSSession"));
            Assert.AreEqual(true, f.Contains("some session server"));
            Assert.AreEqual(false, f.Contains("Quake"));
            Assert.AreEqual(false, f.Contains(""));
        }


        [Test]
        public void ContainsWithMultiplePatterns()
        {
            ProcessFilter f = new ProcessFilter("Session,UserInfo");
            Assert.AreEqual(true, f.Contains("RBCWSUserInfo"));
            Assert.AreEqual(true, f.Contains("Some Session Server"));
            Assert.AreEqual(true, f.Contains("Some Userinfo App"));
            Assert.AreEqual(false, f.Contains("Quake"));
            Assert.AreEqual(false, f.Contains(""));
        }
       
    }
}
