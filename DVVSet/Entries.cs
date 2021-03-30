using System.Collections.Generic;

namespace DVVSet
{
    //      * entries() are sorted by id()
    //      * each counter() also includes the number of values in that id()
    //      * the values in each triple of entries() are causally ordered and each new value goes to the head of the list

    public class Entries //[{id(), counter(), values()}, {values()}] => [{vector},{values}]
    {
        public Entries()
        {
        }

        public Entries(string id, int counter, string value)
        {
            Id = id;
            Counter = counter;
            Value = value;
        }


        public Entries(string id, Vector node)
        {
            Id = id;
            Node = node;
            Counter = node.Counter;
            Value = node.Value;
        }

        public Entries(SortedList<string, Vector> vector)
        {
            VecValues = (List<Vector>) vector.Values;
            Idlist = (List<string>) vector.Keys;
            Vectors = vector;
        }

        public List<Vector> VecValues { get; set; }
        public List<string> Idlist { get; set; }
        public int Counter { get; set; }
        public int LogicalTime { get; set; }
        public string Id { get; set; }
        public List<string> Values { get; set; }
        public string Value { get; set; }
        public SortedList<string, Vector> Vectors { get; set; }
        public Vector Node { get; set; }

        public void Deconstruct(out string id, out int counter, out string value)
        {
            id = Id;
            counter = Counter;
            value = Value;
        }
    }
}