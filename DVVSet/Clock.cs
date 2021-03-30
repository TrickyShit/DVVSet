// 
// A C# implementation of *compact* Dotted Version Vectors, which
// provides a container for a set of concurrent values (siblings) with causal
// order information.
// 

using System;
using System.Collections.Generic;

namespace DVVSet
{
    public class Clock //{entries(), values()}
    {
        public Clock()
        {
        }

        public Clock(Entries entries, string value)
        {
            Entrie = entries;
            Value = value;
        }

        public Clock(string value)
        {
            Entries = new SortedList<string, Vector>();
            Values = new List<string> { value };
        }

        private Clock(SortedList<string, Vector> entries, List<string> values)
        {
            Entries = entries;
            Values = values;
        }

        private Clock(List<string> values)
        {
            Entries = new SortedList<string, Vector>();
            Values = values;
        }

        public int Counter { get; set; }
        public string Id { get; set; }
        private Entries Entrie { get; }
        public string Value { get; private set; }
        private SortedList<string, Vector> Entries { get; }
        private List<string> Values { get; set; }


        public override int GetHashCode()
        {
            return HashCode.Combine(Values, Entries);
        }



        /*********************************************************************************
        * Constructs a new clock set without causal history,
        * and receives one value or list of values that goes to the anonymous list.
        */
        public Clock NewList(List<string> values) => new Clock(values);

        public Clock NewList(string value) => new Clock(value);

        /*
        * Constructs a new clock set with the causal history
        * of the given version vector / vector clock,
        * and receives one value that goes to the anonymous list.
        * The version vector SHOULD BE the output of join.
        */

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

        /* Advances the causal history with the given id.
        * The new value is the *anonymous dot* of the clock.
        * The client clock SHOULD BE a direct result of method NewList.
        */
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
        // with the id I and the new value.
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
                    {{mergeclock.Id, new Vector(mergeclock.Counter, new List<string>{mergeclock.Value})}};
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
            foreach (var v in vector)
                result.Add(v.Key, v.Value);
            result.AddRange(eventV);
            return result;
        }

        private Entries Merge(string id, int count1, List<string> values1, int count2, List<string> values2)
        {

        }
    }
}