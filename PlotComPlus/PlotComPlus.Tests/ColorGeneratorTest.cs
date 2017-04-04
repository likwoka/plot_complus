using System;
using System.Drawing;
using NUnit.Framework;


namespace PlotComPlus
{
    [TestFixture]
    public class ColorGeneratorTest
    {
        [Test]
        public void NextIsUnique()
        {
            ColorGenerator g = new ColorGenerator();
            Color c1 = g.Next();
            Color c2 = g.Next();
            Color c3 = g.Next();
            Console.WriteLine("{0} {1} {2}", c1, c2, c3);
            Assert.AreNotEqual(c1, c2);
            Assert.AreNotEqual(c2, c3);
            Assert.AreNotEqual(c1, c3);
        }
    }
}
