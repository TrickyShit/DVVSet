// 
// A C# implementation of *compact* Dotted Version Vectors, which
// provides a container for a set of concurrent values (siblings) with causal
// order information.
// 

using System;
using System.Collections.Generic;

namespace DVVSet
{
    public class Dvvdotnet
    {
        
        //*******************************************************************************//
        //*   NewList constructs a new clock set without causal history, and            *//
        //*   receives one value or list of values that goes to the anonymous list.     *//
        //*******************************************************************************//
        public Clock NewList(List<string> values=null, string value="") => value=="" ? new Clock(values) : new Clock(value);

        //* NewWithHistory constructs a new clock set with the causal
        //* history of the given version vector / vector clock,
        //* and receives one value that goes to the anonymous list.
        //* The version vector SHOULD BE the output of join.
        public Clock NewWithHistory(List<Entries> vector, string value = "", List<string> values = null)
        {
            var entries = new SortedList<string, Vector>();
            var node = new Vector();
            foreach (var i in vector)
            {
                node.Counter = i.Counter;
                node.Values = i.Values;
                entries.Add(i.Id, node);
            }

            return values == null ? new Clock(entries, new List<string> { value }) : new Clock(entries, values);
        }

        public Clock NewWithHistory(Entries vector, string value = "", List<string> values = null)
        {
            var entries = new SortedList<string, Vector>();
            values ??= new List<string> { value };
            var (id, counter, val) = vector;
            var node = new Vector
            {
                Counter = counter,
                Value = val
            };
            entries.Add(id, node);
            return new Clock(entries, values);
        }

        // Method Create advances the causal history with the given id.
        // The new value is the *anonymous dot* of the clock.
        // The client clock SHOULD BE a direct result of method NewList.
        public Clock Create(Clock clock, string theId) 
        {
            var entries = clock.Entries;
            List<string> values;
            if (entries != null)
            {
                var result = entries;
                values = clock.Values;
                return new Clock(Entry(result, theId, values), new List<string>());

            }
            else
            {
                var (id, counter, value) = clock.Entrie;
                values = new List<string> { value };
                var vector = new Vector(counter, values);
                var result = new SortedList<string, Vector> { { id, vector } };
                return new Clock(Entry(result, theId, values), new List<string>());
            }
        }

        // We create a new event on the synced causal history,
        // with the id and the new value.
        // The anonymous values that were synced still remain.
        private SortedList<string, Vector> Entry(SortedList<string, Vector> vector, string theId, List<string> value)
        {
            var result = new SortedList<string, Vector>();
            if (vector.Count == 0)
            {
                var res = new Vector
                {
                    Counter = 1,
                    Values = value
                };
                result.Add(theId, res);
                return result;
            }
            if (vector.ContainsKey(theId))
            {
                var (mergeCounter, mergeValues) = vector[theId];
                int newCounter = mergeCounter + 1;
                var mergeclock = Merge(theId, mergeCounter, mergeValues, newCounter, value);
                return new SortedList<string, Vector>
                    {{mergeclock.Keys[0], mergeclock.Values[0]}};
            }
            else
            {
                var vectorId = vector.Keys[0];
                var i = string.Compare(vectorId, theId, StringComparison.Ordinal);
                if (i > 0)
                {
                    var newVector = new Vector { Counter = 1, Values = value };
                    result.Add(theId, newVector);
                    foreach (var v in vector)
                        result.Add(v.Key, v.Value);
                    return result;
                }
            }
            result.Add(vector.Keys[0], vector.Values[0]);
            var eventValue = vector;
            eventValue.RemoveAt(0);
            var eventV = Entry(eventValue, theId, value);
            foreach (var (key, vector1) in eventV)
                result.Add(key, vector1);
            return result;
        }

        private SortedList<string, Vector> Merge(string id, int count1, List<string> values1, int count2, List<string> values2)
        {
            int len1 = values1.Count;
            int len2 = values2.Count;
            var result = new SortedList<string, Vector>();
            List<string> values;
            int count;
            if (count1 >= count2)
            {
                count = count1;
                values = count1 - len1 >= count2 - len2 ? values1 : values1.GetRange(0, count1 - count2 + len2);
            }
            else
            {
                count=count2;
                values=count2 - len2 >= count1 - len1?values2:values2.GetRange(0, count2 - count1 + len1);
            }
            var value = new Vector(count, values);
            result.Add(id, value);
            return result;
        }

        // SortedList represents a collection of key/value pairs
        // that are automatically sorted by the key(ID) and are accessible by key
        // and by index. So method foldl is don't needed.

        //*******************************************************************************//
        //* Synchronizes a list of clocks using Sync().                                 *// 
        //* It discards (causally) outdated values, while merging all causal histories. *//

        private Clock Sync(Clock clock1, Clock clock2)
        {
            if(clock1==null)return clock2;
            if(clock2==null)return clock1;
            var(entries1, values1)=clock1;
            var(entries2, values2)=clock2;

        }
    }
}
