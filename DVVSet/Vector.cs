using System;
using System.Collections.Generic;
using System.Linq;

namespace DVVSet
{
    public class Vector //[{id(), counter()}, {values()}}].
    {
        public int Counter { get; set;}
        public List<string> Values { get; set;}
        public string Value { get; set; }
        public string Id { get; set; }


        public Vector(){ }

        public Vector(string id, int counter, string value)
        {
            Id=id;
            Counter = counter;
            Value = value;
        }

        public void Deconstruct(out int counter, out string value)
        {
            id = this.Id;
            counter = this.Counter;
            value = this.Value;
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