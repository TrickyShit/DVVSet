// 
// A C# implementation of *compact* Dotted Version Vectors, which
// provides a container for a set of concurrent values (siblings) with causal
// order information.
// 
using System.Collections.Generic;

namespace DVVSet
{
    // 
    //     DVVSet object for type reference.
    // 
    public class Clock
    {
        public class NodeClock
        {
            public Entries Entries { get; set; }
            public Values Values { get; set; }
            public NodeClock(Entries entries, Values values)
            {
                this.Entries = entries;
                this.Values = values;
            }
        }
        public List<NodeClock> Clocks = new List<NodeClock>();
        public Clock(List<NodeClock> clocks) => Clocks = clocks;
    }

    public class Entries
    {
        
    }
    public class Values
    {

        public override object ToString()
        {
            return "Entries {}".format(this.ToString());
        }
    }

    // 
    //     Vector object for type reference.
    //     
    public class Vector
    {
        public DotwithID[] Vect = new Vector[Dotwithid];
    }

    // 
    //     Dot object for type reference.
    //   
    public class DotwithID
    {
        public DotwithID() { }
        public string Id { get; set; }
        public IDictionary<int, int> Dot => new Dictionary<int, int>();

    }

    // 
    //     Entries object for type reference.
    //     

}
