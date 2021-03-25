// 
// A C# implementation of *compact* Dotted Version Vectors, which
// provides a container for a set of concurrent values (siblings) with causal
// order information.
// 
using System;
using System.Collections.Generic;

namespace DVVSet
{
    public class Clock      //{entries(), values()}
    {
        public List<Entries> Entries{ get; set; }
        public List<string> Values{ get; set; }

        public Clock(){ }

        public Clock(List<Entries> entries, List<string> values)
        {
            Entries=entries;
            Values=values;
        }

        public Clock(List<string> values)
        {
            Entries = null;
            Values = values;
        }

        public override int GetHashCode() => HashCode.Combine(Entries, Values);

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class Entries    //[{id(), counter(), values(), logical_time()}].
    {
        public string Id{ get; set; }
        public int Counter{ get; set; }
        public string Values { get; set; }
        public int Logical_time{ get; set; }
    }


    // 
    //     Vector object for type reference.
    //     
    public class Vector     //[{id(), counter(), logical_time()}]
    {
        public string Id { get; set; }
        public int Counter { get; set; }
        public int Logical_time { get; set; }
    }


}
