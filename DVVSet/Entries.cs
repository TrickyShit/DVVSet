using System;
using System.Collections.Generic;
using System.Linq;

namespace DVVSet
{
    public class Entries //[{id(), counter(), values()}, {values()}] => [{vector},{values}]
    {
        public int Counter { get; set; }
        public int LogicalTime { get; set; }
        public string Id { get; set; }
        public List<string> Values { get; set; }
        public List<string> VecValues { get; set; }

        public Vector Vector { get; set; }
        public List<Vector> Vectors { get; set; }

        public Entries() { }

        public Entries(Vector vector, List<string> values) 
        {
            Vector = vector;
            Id = vector.Id;
            Counter = vector.Counter;
            VecValues = vector.Values;
            Values = values;
        }

        public Entries(List<Vector> vector, List<string> values)
        {
            Vectors = vector;
            Values = values;
        }

        public void Deconstruct(out string id, out int counter, out List<string> values)
        {
            id = this.Id;
            counter = this.Counter;
            values = this.Values;
        }

        public static implicit operator Entries(Vector v)
        {
            throw new NotImplementedException();
        }
    }
}