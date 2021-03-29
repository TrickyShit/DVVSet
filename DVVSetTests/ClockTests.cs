using Microsoft.VisualStudio.TestTools.UnitTesting;
using DVVSet;
using System;
using System.Collections.Generic;
using System.Text;

namespace DVVSet.Tests
{
    [TestClass()]
    public class ClockTests
    {
        [TestMethod()]
        public void NewWithHistoryTest()
        {
            var test=new Clock();
            var a = new Clock("v1");
            var a1 = test.Create(a, "a");
            //var b = test.NewWithHistory(Join(a1), "v2");
            //var b1 = test.Update(b, a1, "b");

        }
    }
}