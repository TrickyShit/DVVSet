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

        public Clock() { }

        public Clock(SortedList<string, Vector> entries)
        {
            Entries = entries;
            ClockValues = new List<string>();
        }

        public Clock(string value)
        {
            Entries = new SortedList<string, Vector>();
            ClockValues = new List<string> {value};
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

        protected static string ClockToString(Clock clock)
        {
            var result="";
            if (clock.Entries.Count > 0)
            {
                int count = 0;
                foreach (var (key, (counter, value)) in clock.Entries)
                {
                    result += "[{" + key + "," + counter + ",";
                    if (value.Count == 0) result += "[]";
                    else result = value.Aggregate(result, (current, i) => current + "[" + i + "]");
                    result += "}],";
                    if (clock.ClockValues.Any()) result += "[" + clock.ClockValues[count] + "];";
                    else result+="[];";
                    count++;
                }
            }
            else
            {
                if(clock.ClockValues.Any())return result+clock.ClockValues.Aggregate("[],", (current, i) => current + "[" + i + "]")+";";
                else return "[],[];";
            }
            return result;
        }
    }
}