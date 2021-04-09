using Microsoft.VisualStudio.TestTools.UnitTesting;
using DVVSet;
using System.Collections.Generic;

namespace DVVSetTests
{
    [TestClass()]
    public class ClockTests : Dvvdotnet
    {
        [TestMethod()]
        public void NewWithHistoryTest()
        {
            var a = new Clock("v1");
            var a1 = Create(a, "a");
            var b = new Clock(Join(a1), "v2");
            var b1 = Update(b, a1, "b");
            Assert.AreEqual(ClockToString(a), "[],[v1];");
            Assert.AreEqual(ClockToString(a1), "[{a,1,[v1]}],[];");
            Assert.AreEqual(ClockToString(b), "[{a,1,[]}],[v2];");
            Assert.AreEqual(ClockToString(b1), "[{a,1,[]}],[{b,1,[v2]}],[];");
        }

        [TestMethod()]
        public void UpdateTest()
        {
            var a0 = Create(new Clock("v1"), "a");
            var a1 = Update(new Clock(Join(a0), "v2"), a0, "a");
            var a2 = Update(new Clock(Join(a1), "v3"), a1, "b");
            var a3 = Update(new Clock(Join(a0), "v4"), a1, "b");
            var a4 = Update(new Clock(Join(a0), "v5"), a1, "a");
            Assert.AreEqual(ClockToString(a0), "[{a,1,[v1]}],[];");
            Assert.AreEqual(ClockToString(a1), "[{a,2,[v2]}],[];");
            Assert.AreEqual(ClockToString(a2), "[{a,2,[]}],[{b,1,[v3]}],[];");
            Assert.AreEqual(ClockToString(a3), "[{a,2,[v2]}],[{b,1,[v4]}],[];");
            Assert.AreEqual(ClockToString(a4), "[{a,3,[v5][v2]}],[];");     //little change to string because i don`t want change ClockToString :)
        }

        [TestMethod()]
        public void SyncTest()
        {
            var temp = new Clock();
            var x = Create(temp, "x");              //{[{x,1,[]}],[] as Erlang
            var a = Create(new Clock("v1"), "a");   //{[{a,1,[v1]}],[]
            var y = Create(new Clock("v2"), "b");   
            var a1 = Create(new Clock(Join(a), "v2"), "a");
            var a3 = Create(new Clock(Join(a1), "v3"), "b");
            var a4 = Create(new Clock(Join(a1), "v3"), "c");
            //F   = fun (L,R) -> L>R end;
            var w = Create(temp, "a");           //W = {[{a,1,[]}],[]}
            var z = Create(temp, "a");           //z = {[{a,2,[v2][v1]}],[]};
            z.Entries.Values[0].Counter = 2;
            z.Entries.Values[0].Values.Add("v2");
            z.Entries.Values[0].Values.Add("v1");
            Assert.AreEqual(ClockToString(SyncClocks(w, z)), "[{a,2,[v2]}],[];");
            Assert.AreEqual(ClockToString(SyncClocks(z, w)), "[{a,2,[v2]}],[];");
            Assert.AreEqual(ClockToString(SyncClocks(a4, a3)), "[{a,2,[]}],[{b,1,[v3]}],[{c,1,[v3]}],[];");
            Assert.AreEqual(ClockToString(SyncClocks(a3, a4)), "[{a,2,[]}],[{b,1,[v3]}],[{c,1,[v3]}],[];");
            Assert.AreEqual(ClockToString(SyncClocks(a, a1)), ClockToString(SyncClocks(a1, a)));
            Assert.AreEqual(ClockToString(SyncClocks(x, a)), "[{a,1,[v1]}],[{x,1,[]}],[];");
            Assert.AreEqual(ClockToString(SyncClocks(x, a)), ClockToString(SyncClocks(a, x)));
            Assert.AreEqual(ClockToString(SyncClocks(x, a)), ClockToString(SyncClocks(a, x)));
            Assert.AreEqual(ClockToString(SyncClocks(a, y)), "[{a,1,[v1]}],[{b,1,[v2]}],[];");
            Assert.AreEqual(ClockToString(SyncClocks(y, a)), ClockToString(SyncClocks(a, y)));
            Assert.AreEqual(ClockToString(SyncClocks(y, a)), ClockToString(SyncClocks(a, y)));
            Assert.AreEqual(ClockToString(SyncClocks(a, x)), ClockToString(SyncClocks(x, a)));
        }

        [TestMethod()]
        public void SyncupdateTest()
        {
            var a0 = Create(new Clock(new List<string>{"v1"}), "a");            // Mary writes v1 w / o VV
            var vv1 = Join(a0);                                                 // Peter reads v1 with version vector(VV)
            var a1 = Update(new Clock(new List<string> {"v2"}), a0, "a");       // Mary writes v2 w / o VV
            var a2 = Update(new Clock(vv1,new List<string> {"v3"}), a1, "a");   // Peter writes v3 with VV from v1
            Assert.AreEqual(ClockToString(vv1), "[{a,1,[]}],[];");
            Assert.AreEqual(ClockToString(a0), "[{a,1,[v1]}],[];");
            Assert.AreEqual(ClockToString(a1), "[{a,2,[v2][v1]}],[];");
            // now A2 should only have v2 and v3, since v3 was causally newer than v1
            Assert.AreEqual(ClockToString(a2), "[{a,3,[v3][v2]}],[];");
        }
    }
}