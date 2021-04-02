using System;
using System.Collections.Generic;

namespace DVVSet
{
    public class Vector //[{id(), counter()}, {values()}}].
    {
        public int Counter { get; set; }
        public List<string> Values { get; set; }
        public string Id { get; set; }

        public Vector()
        {
        }

        public Vector(int counter, List<string> values)
        {
            Counter = counter;
            Values = values;
        }


        public void Deconstruct(out int counter, out List<string> value)
        {
            counter = Counter;
            value = Values;
        }

        //public override bool Equals(object obj)
        //{
        //    return obj is Vector vector &&
        //           Id == vector.Id &&
        //           Counter == vector.Counter &&
        //           Value == vector.Value &&
        //           EqualityComparer<List<string>>.Default.Equals(Values, vector.Values);
        //}

        public override int GetHashCode()
        {
            return HashCode.Combine(Counter, Values);
        }
    }
}