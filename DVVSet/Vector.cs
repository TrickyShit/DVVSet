using System;
using System.Collections.Generic;

namespace LUC.DVVSet
{
    public class Vector //[{id(), counter()}, {values()}}].
    {
        public int Counter { get; set; }
        public List<string> Values { get; set; }

        public Vector()
        {
        }

        public Vector(int counter, List<string> values)
        {
            Counter = counter;
            Values = values;
        }


        public void Deconstruct(out int counter, out List<string> values)
        {
            counter = Counter;
            values = Values;
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

        protected static void AddEntries(SortedList<string, Vector> entries, string id="", int counter=0, List<string> values=null)
        {
            var vector = new Vector
            {
                Counter = counter,
                Values = values,
            };
            if (id != null) entries.Add(key: id, value: vector);
        }
    }
}