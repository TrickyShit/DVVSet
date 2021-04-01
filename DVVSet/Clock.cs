// 
// A C# implementation of *compact* Dotted Version Vectors, which
// provides a container for a set of concurrent values (siblings) with causal
// order information.
// 

using System.Collections.Generic;

namespace DVVSet
{
    public class Clock //{entries(), values()}
    {
        public Clock()
        {
        }

        public Clock(Entries entries)
        {
            Entrie = entries;
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

        public void Deconstruct(out SortedList<string, Vector> entries, out List<string>values)
        {
            entries = Entries;
            values = Values;
        }

        public int Counter { get; set; }
        public string Id { get; set; }
        public Entries Entrie { get; }
        public SortedList<string, Vector> Entries { get; set; }
        public List<string> Values { get; set; }
    }
}