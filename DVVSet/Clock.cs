// 
// A C# implementation of *compact* Dotted Version Vectors, which
// provides a container for a set of concurrent values (siblings) with causal
// order information.
// 
using System;
using System.Collections.Generic;
using System.Linq;

namespace DVVSet
{
    public class Clock//{entries(), values()}
    {
        public int Counter { get; set; }
        public string Id { get; set; }
        private List<string> Values { get; set; }
        private string Value { get; set; }
        private SortedList<string, Vector> Entries { get; set; }
        private Entries Entrie { get; set; }

        public Clock() { }

        public Clock(Entries entries, string value)
        {
            Entrie = entries;
            Value = value;
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

        public Clock(string value)
        {
            Entries = new SortedList<string, Vector>();
            Values = new List<string> { value };
        }

        public override int GetHashCode() => HashCode.Combine(Values, Entries);



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
            return values == null ? new Clock(entries, new List<string> {value}) : new Clock(entries, values);
        }

        public Clock NewWithHistory(Entries vector, string value="", List<string> values = null)
        {
            var entries = new SortedList<string, Vector>();
            values ??= new List<string> {value};
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
            var (id, counter, value) = clock.Entrie;
            var result = new Vector(id, counter, value);
            return new Clock(Entry(result, theId, value), new List<string>());
        }

        private SortedList<string, Vector> Entry(Vector vector, string theId, string value)
        {
            var result = new SortedList<string, Vector>();  //Автоматическая сортировка по ID
            var values = vector.Values;
            if (!vector.Any())
            {
                var res = new Vector
                {
                    Counter = 1,
                    Values = new List<string> { value }
                };
                result.Add(theId, res);
                return result;
            }
            if (vectorId[0] == theId)
            {
                var newvalues = new List<string> { value };           //* the values in each triple of entries()
                var oldvalues = new List<string>();                 //* are causally ordered and each new value
                foreach (var i in vector)       //* goes to the head of the list, as Erlang
                {
                    oldvalues.Insert(0, i.Value.Value);
                }
                newvalues.AddRange(oldvalues);
                var res = new Vector
                {
                    Counter = +1,
                    Values = newvalues
                };
                result.Add(vectorId, res);
                return result;
            }
            else
            {
                var i = vectorId.CompareTo(theId);
                if (i > 0)
                {
                    var res = new Vector
                    {
                        Counter = 1,
                        Values = value
                    };
                    result.Add(theId, res);
                    foreach (var v in vector)
                    {
                        if (!v.Equals(vector[0]))
                        {
                            result.Add(v);
                        }
                    }
                    return result;
                }
            }
            result.Add(vector[0]);
            var eventValue = vector;
            eventValue.RemoveAt(0);
            var eventV = Entry(eventValue, theId, value);
            result.AddRange(eventV);
            return result;
        }


    }
}

