using System;
using System.Collections.Generic;

namespace DVVSet
{
    public class Vector //[{id(), counter()}, {values()}}].
    {
        public Vector()
        {
        }

        public Vector(int counter, List<string> value)
        {
            Counter = counter;
            Values = value;
        }

        public int Counter { get; set; }
        public List<string> Values { get; set; }
        public string Value { get; set; }
        public string Id { get; set; }

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
            return HashCode.Combine(Counter, Value);
        }
    }
}