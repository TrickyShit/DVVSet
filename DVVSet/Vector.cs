using System;
using System.Collections.Generic;
using System.Linq;

namespace DVVSet
{
    public class Vector //[{id(), counter(), {values()}}].
    {
        public string Id { get; set; }
        public int Counter { get; set; }
        public string Value { get; set; }
        public List<string> Values { get; set; }


        public Vector() { }

        public Vector(string id, int counter, List<string> values)
        {
            Id = id;
            Counter = counter;
            Values = values;
        }

        public void Deconstruct(out string id, out int counter, out List<string> values)
        {
            id = this.Id;
            counter = this.Counter;
            values = this.Values;
        }


        public override bool Equals(object obj)
        {
            return obj is Vector vector &&
                   Id == vector.Id &&
                   Counter == vector.Counter &&
                   Value == vector.Value &&
                   EqualityComparer<List<string>>.Default.Equals(Values, vector.Values);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Counter, Value, Values);
        }
    }
}