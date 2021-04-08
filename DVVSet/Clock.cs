// 
// A C# implementation of *compact* Dotted Version Vectors, which
// provides a container for a set of concurrent values (siblings) with causal
// order information.
// 

using System;
using System.Collections.Generic;
using System.Linq;

namespace DVVSet
{
    public class Clock //{entries(), values()}
    {
        public SortedList<string, Vector> Entries { get; set; }
        public List<string> ClockValues { get; set; }

        public Clock()
        {
            Entries = new SortedList<string, Vector>();
            ClockValues = new List<string>();
        }

        public Clock(SortedList<string, Vector> entries)
        {
            Entries = entries;
            ClockValues = new List<string>();
        }

        public Clock(string value)
        {
            Entries = new SortedList<string, Vector>();
            ClockValues = new List<string> { value };
        }

        public Clock(List<string> clockValues)
        {
            Entries = new SortedList<string, Vector>();
            ClockValues = clockValues;
        }

        public Clock(SortedList<string, Vector> entries, List<string> clockValues) : this(clockValues)
        {
            Entries = entries;
            ClockValues = clockValues;
        }

        public void Deconstruct(out SortedList<string, Vector> entries, out List<string> values)
        {
            entries = Entries;
            values = ClockValues;
        }

        protected static string ClockToString(object tostring)
        {
            var result = "";
            var clock=new Clock();
            if (tostring is SortedList<string, Vector>)
            {
                clock.Entries = (SortedList<string, Vector>)tostring;
                clock.ClockValues = new List<string>();
            }
            if(tostring is Clock) clock=(Clock)tostring;
            if (clock.Entries.Count > 0)
            {
                int count = 0;
                foreach (var (key, (counter, value)) in clock.Entries)
                {
                    result += "[{" + key + "," + counter + ",";
                    if (value.Count == 0) result += "[]";
                    else result = value.Aggregate(result, (current, i) => current + "[" + i + "]");
                    result += "}],";
                    //if (clock.ClockValues.Any()) result += "[" + clock.ClockValues[count] + "];";
                    //else result+="[];";
                    count++;
                }
            }
            else result += "[],";
            if (clock.ClockValues.Any()) return result + "[" + clock.ClockValues.Aggregate((current, i) => current + "[" + i + "]") + "];";
            else return result + "[];";
        }

        private static int CompareEntries(SortedList<string, Vector> entries1, SortedList<string, Vector> entries2)
        {
            int counter = 0;
            foreach (var va in entries1)
            {
                var vb = entries2.ToArray()[counter];
                try
                {
                }
                catch (Exception)               //if size(va) > size(vb) and all values in vb are compared => return 1
                {
                    return 1;
                }
                counter++;
                var s1 = entries1.Count;     //length of values list in vector
                var s2 = entries2.Count;
                if ((s1 > 0) && (s2 > 0))
                {
                    if (s1 > s2) return 1;
                    if (s1 == s2) continue;
                    return -1;
                }
                if (s1 != 0) return 1;
                if (s2 != 0) return -1;
                return 0;
            }
            return 0;
        }
    }
}