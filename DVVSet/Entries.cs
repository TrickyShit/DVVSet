using System;
using System.Collections.Generic;

namespace DVVSet
{
    //      * entries() are sorted by id()
    //      * each counter() also includes the number of values in that id()
    //      * the values in each triple of entries() are causally ordered and each new value goes to the head of the list

    public class Entries:Vector //[{id(), counter(), values()}, {values()}] => [{vector},{values}]
    {
        public SortedList<string, Vector>Entry{get;set;}

        public Entries()
        {
        }

        public Entries(SortedList<string, Vector>entries)
        {
            Entry = entries;
        }


        protected static void AddEntries(SortedList<string, Vector> entries, string id=null, int counter=1, List<string> values=null)
        {
            var vector = new Vector
            {
                Counter = counter,
                Values = values,
                Id = id
            };

            if (id != null) entries.Add(key: id, value: vector);
            else entries = new SortedList<string, Vector>();
        }
    }
}