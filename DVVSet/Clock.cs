// 
// A C# implementation of *compact* Dotted Version Vectors, which
// provides a container for a set of concurrent values (siblings) with causal
// order information.
// 

using System.Collections.Generic;

namespace DVVSet
{
    public class Clock:Entries //{entries(), values()}
    {
        public List<string> Values { get; set; }
        public SortedList<string, Vector> Entries { get; set; }

        public Clock() { }

        public Clock(SortedList<string, Vector> entries)
        {
            Entries = entries;
            Values = new List<string>();
        }

        public Clock(string value)
        {
            Entries = new SortedList<string, Vector>();
            Values = new List<string> { value };
        }

        public Clock(SortedList<string, Vector> entries, List<string> values)
        {
            Entries = entries;
            Values = values;
        }

        public Clock(List<string> values)
        {
            Entries = new SortedList<string, Vector>();
            Values = values;
        }

        public void Deconstruct(out SortedList<string, Vector> entries, out List<string> values)
        {
            entries = Entries;
            values = Values;
        }
    }
}