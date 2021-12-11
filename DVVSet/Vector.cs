using System;
using System.Collections.Generic;

namespace LUC.DVVSet
{
    /// <summary>
    /// Constructs a Vector value
    /// </summary>
    public class Vector //[{id(), counter()}, {values()}}].
    {
        /// <summary>
        /// integer value, counts every logical tick of clocks
        /// </summary>
        public Int32 Counter { get; set; }
        /// <summary>
        /// Any information, needed for work with DVVSet. 
        /// </summary>
        public List<String> Values { get; set; }

        /// <summary>
        /// Empty exemplar of Vector value
        /// </summary>
        public Vector()
        {
        }

        /// <summary>
        /// New Vector value with prepared Counter and Values
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="values"></param>
        public Vector(Int32 counter, List<String> values)
        {
            Counter = counter;
            Values = values;
        }

        /// <summary>
        /// Deconstructs Vector to Counter and Values.
        /// </summary>
        /// <param name="counter"></param>
        /// <param name="values"></param>
        public void Deconstruct(out Int32 counter, out List<String> values)
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

#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
        public override Int32 GetHashCode()
#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
        {
            return HashCode.Combine(Counter, Values);
        }

        /// <summary>
        /// Adds single entry to Entries
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="id"></param>
        /// <param name="counter"></param>
        /// <param name="values"></param>
        protected static void AddEntries(SortedList<String, Vector> entries, String id ="", Int32 counter =0, List<String> values=null)
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