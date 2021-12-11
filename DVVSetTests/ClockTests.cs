using System.Collections.Generic;
using LUC.DVVSet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static LUC.DVVSet.Dvvdotnet;
using static LUC.DVVSet.Clock;

namespace DVVSetTests
{
    [TestClass()]
    public class ClockTests
    {
        [TestMethod()]
        public void NewWithHistoryTest()
        {
            var a = new Clock("v1");
            var a1 = Dvvdotnet.Update(a, "a");
            var b = new Clock(Dvvdotnet.Join(a1), "v2");
            var b1 = Update(b, "b", a1);
            Assert.AreEqual(ClockToString(a), "[],[v1];");
            Assert.AreEqual(ClockToString(a1), "[{a,1,[v1]}],[];");
            Assert.AreEqual(ClockToString(b), "[{a,1,[]}],[v2];");
            Assert.AreEqual(ClockToString(b1), "[{a,1,[]}],[{b,1,[v2]}],[];");
        }

        [TestMethod()]
        public void UpdateTest()
        {
            var a0 = Update(new Clock("v1"), "a");
            var a1 = Update(new Clock(Join(a0), "v2"), "a", a0);
            var a2 = Update(new Clock(Join(a1), "v3"), "b", a1);
            var a3 = Update(new Clock(Join(a0), "v4"), "b", a1);
            var a4 = Update(new Clock(Join(a0), "v5"), "a", a1);
            Assert.AreEqual(ClockToString(a0), "[{a,1,[v1]}],[];");
            Assert.AreEqual(ClockToString(a1), "[{a,2,[v2]}],[];");
            Assert.AreEqual(ClockToString(a2), "[{a,2,[]}],[{b,1,[v3]}],[];");
            Assert.AreEqual(ClockToString(a3), "[{a,2,[v2]}],[{b,1,[v4]}],[];");
            Assert.AreEqual(ClockToString(a4), "[{a,3,[v5][v2]}],[];");    
        }

        [TestMethod()]
        public void SyncTest()
        {
            var temp = new Clock();
            var x = Update(temp, "x");              //{[{x,1,[]}],[] as Erlang
            var a = Update(new Clock("v1"), "a");   //{[{a,1,[v1]}],[]
            var y = Update(new Clock("v2"), "b");
            var a1 = Update(new Clock(Join(a), "v2"), "a");
            var a3 = Update(new Clock(Join(a1), "v3"), "b");
            var a4 = Update(new Clock(Join(a1), "v3"), "c");
            //F   = fun (L,R) -> L>R end;
            var w = Update(temp, "a");           //W = {[{a,1,[]}],[]}
            var z = Update(temp, "a");           //z = {[{a,2,[v2][v1]}],[]};
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
            var a0 = Update(new Clock(new List<System.String> { "v1" }), "a");            // Mary writes v1 w / o VV
            var vv1 = Join(a0);                                                 // Peter reads v1 with version vector(VV)
            var a1 = Update(new Clock(new List<System.String> { "v2" }), "a", a0);       // Mary writes v2 w / o VV
            var a2 = Update(new Clock(vv1, new List<System.String> { "v3" }), "a", a1);   // Peter writes v3 with VV from v1
            Assert.AreEqual(ClockToString(vv1), "[{a,1,[]}],[];");
            Assert.AreEqual(ClockToString(a0), "[{a,1,[v1]}],[];");
            Assert.AreEqual(ClockToString(a1), "[{a,2,[v2][v1]}],[];");
            // now A2 should only have v2 and v3, since v3 was causally newer than v1
            Assert.AreEqual(ClockToString(a2), "[{a,3,[v3][v2]}],[];");
        }

        [TestMethod()]
        public void EventTest()
        {
            var a = Update(new Clock("v1"), "a");
            Assert.AreEqual(ClockToString(Entry(a, "a", "v2")), "[{a,2,[v2][v1]}],[];");
            Assert.AreEqual(ClockToString(Entry(a, "b", "v2")), "[{a,1,[v1]}],[{b,1,[v2]}],[];");
        }

        [TestMethod()]
        public void LessTest()
        {
            var a = Update(new Clock("v1"), "a");
            var b = Update(new Clock(Join(a), "v2"), "a");
            var b2 = Update(new Clock(Join(a), "v2"), "b");
            var b3 = Update(new Clock(Join(a), "v2"), "z");
            var c = Update(new Clock(Join(b), "v3"), "c", a);
            var d = Update(new Clock(Join(c), "v4"), "d", b2);
            Assert.IsTrue(Less(a, b));
            Assert.IsTrue(Less(a, c));
            Assert.IsTrue(Less(b, c));
            Assert.IsTrue(Less(b, d));
            Assert.IsTrue(Less(b2, d));
            Assert.IsTrue(Less(a, d));
            Assert.IsFalse(Less(b2, c));
            Assert.IsFalse(Less(b, b2));
            Assert.IsFalse(Less(b2, b));
            Assert.IsFalse(Less(a, a));
            Assert.IsFalse(Less(c, c));
            Assert.IsFalse(Less(d, b2));
            Assert.IsFalse(Less(b3, d));
        }

        [TestMethod()]
        public void EqualTest()
        {
            var a = new Clock();            //[{a,4,[v5][v0]}],[{b,0,[]}],[{c,1,[v3]}],[v0]
            var a1 = new KeyValuePair<System.String, Vector>("a", new Vector(4, new List<System.String> { "v5", "v0" }));
            var a2 = new KeyValuePair<System.String, Vector>("b", new Vector(0, new List<System.String>()));
            var a3 = new KeyValuePair<System.String, Vector>("c", new Vector(1, new List<System.String> { "v3" }));
            a.Entries.Add(a1.Key, a1.Value);
            a.Entries.Add(a2.Key, a2.Value);
            a.Entries.Add(a3.Key, a3.Value);
            a.ClockValues.Add("v0");

            var b = new Clock();              //[{a,4,[v555,v0]}],[{b,0,[]}],[{c,1,[v3]}],[];
            var b1 = new KeyValuePair<System.String, Vector>("a", new Vector(4, new List<System.String> { "v555", "v0" }));
            var b2 = new KeyValuePair<System.String, Vector>("b", new Vector(0, new List<System.String>()));
            var b3 = new KeyValuePair<System.String, Vector>("c", new Vector(1, new List<System.String> { "v3" }));
            b.Entries.Add(b1.Key, b1.Value);
            b.Entries.Add(b2.Key, b2.Value);
            b.Entries.Add(b3.Key, b3.Value);

            var c = new Clock();              //[{a,4,[v5,v0]}],[{b,0,[]}],[v6,v1];
            var c1 = new KeyValuePair<System.String, Vector>("a", new Vector(4, new List<System.String> { "v5", "v0" }));
            var c2 = new KeyValuePair<System.String, Vector>("b", new Vector(0, new List<System.String>()));
            c.Entries.Add(c1.Key, c1.Value);
            c.Entries.Add(c2.Key, c2.Value);
            c.ClockValues.Add("v6");
            c.ClockValues.Add("v1");
            // compare only the causal history
            Assert.IsTrue(Equal(a, b));
            Assert.IsTrue(Equal(b, a));
            Assert.IsFalse(Equal(a, c));
            Assert.IsFalse(Equal(b, c));
        }

        [TestMethod()]
        public void SizeTest()
        {
            var a = new Clock();            //[{a,4,[v5][v0]}],[{b,0,[]}],[{c,1,[v3]}],[v4][v1]
            var a1 = new KeyValuePair<System.String, Vector>("a", new Vector(4, new List<System.String> { "v5", "v0" }));
            var a2 = new KeyValuePair<System.String, Vector>("b", new Vector(0, new List<System.String>()));
            var a3 = new KeyValuePair<System.String, Vector>("c", new Vector(1, new List<System.String> { "v3" }));
            a.Entries.Add(a1.Key, a1.Value);
            a.Entries.Add(a2.Key, a2.Value);
            a.Entries.Add(a3.Key, a3.Value);
            a.ClockValues.Add("v4");
            a.ClockValues.Add("v1");
            Assert.AreEqual(1, Size(new Clock("v1")));
            Assert.AreEqual(5, Size(a));
        }
    }
}