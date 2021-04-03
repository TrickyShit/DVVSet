using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DVVSet;

namespace DVVSetTests
{
    [TestClass()]
    public class ClockTests:Dvvdotnet
    {
        [TestMethod()]
        public void NewWithHistoryTest()
        {
            var a = ( new Clock("v1"));
            Assert.AreEqual(ClockToString(a), "[][v1]");
            var a1 = Create(a, "a");
            Assert.AreEqual(ClockToString(a1), "[{a,1,[v1]}]");
            var b = NewWithHistory(Join(a1), "v2");
            Assert.AreEqual(ClockToString(b), "[{a,1,[]}][v2]");
            var b1 = Update(b, a1, "b");
            Assert.AreEqual(ClockToString(b1), "[{a,1,[]}][{b,1,[v2]}]");
        }
    }
}