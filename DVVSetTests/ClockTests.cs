using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DVVSet;

namespace DVVSetTests
{
    [TestClass()]
    public class ClockTests : Dvvdotnet
    {
        [TestMethod()]
        public void NewWithHistoryTest()
        {
            var a = new Clock("v1");
            Assert.AreEqual(expected: ClockToString(a), actual: "[],[v1];");
            var a1 = Create(a, "a");
            Assert.AreEqual(expected: ClockToString(a1), actual: "[{a,1,[v1]}],[];");
            var b = NewWithHistory(Join(a1), "v2");
            Assert.AreEqual(expected: ClockToString(b), actual: "[{a,1,[]}],[v2];");
            var b1 = Update(b, a1, "b");
            Assert.AreEqual(expected: ClockToString(b1), actual: "[{a,1,[]}],[];[{b,1,[v2]}],[];");
        }

        [TestMethod()]
        public void UpdateTest()
        {
            var a0 = Create(new Clock("v1"), "a");
            Assert.AreEqual(ClockToString(a0), "[{a,1,[v1]}],[];");
            var a1 = Update(NewWithHistory(Join(a0), "v2"), a0, "a");
            Assert.AreEqual(ClockToString(a1), "[{a,2,[v2]}],[];");
            var a2 = Update(NewWithHistory(Join(a1), "v3"), a1, "b");
            Assert.AreEqual(ClockToString(a2), "[{a,2,[]}],[];[{b,1,[v3]}],[];");
            var a3 = Update(NewWithHistory(Join(a0), "v4"), a1, "b");
            Assert.AreEqual(ClockToString(a3), "[{a,2,[v2]}],[];[{b,1,[v4]}],[];");
            var a4 = Update(NewWithHistory(Join(a0), "v5"), a1, "a");
            Assert.AreEqual(ClockToString(a4), "[{a,3,[v5][v2]}],[];");     //little change to string because i don`t want change ClockToString :)
        }
    }
}