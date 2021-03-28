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
        private List<Entries> Entries { get; }
        private List<string> Values { get; }

        public Clock() { }

        private Clock(List<Entries> entries, List<string> values)
        {
            Entries = entries;
            Values = values;
        }

        private Clock(List<string> values)
        {
            Entries = new List<Entries>();
            Values = values;
        }

        public Clock(string value)
        {
            Entries = new List<Entries>();
            Values = new List<string> { value };
        }

        /*
        * Constructs a new clock set without causal history,
        * and receives one value or list of values that goes to the anonymous list.
        */
        public Clock NewList(List<string> values) => new Clock(values);
        public Clock NewList(string value) => new Clock(value);

        /* Advances the causal history with the given id.
        * The new value is the *anonymous dot* of the clock.
        * The client clock SHOULD BE a direct result of method NewList.
        */
        public Clock Create(Clock clock, string theId)
        {
            var value = clock.Values;
            List<Entries> result = Entry(clock.Entries, theId, value);
            return new Clock(result, new List<string>());
        }
        public List<Entries> Entry(List<Entries> vector, string theId, List<string> value)
        {
            List<Entries> result = new List<Entries>();
            (string vectorId, int counter, List<string> oldvalues) = vector[0];
            if (!vector.Any())
            {
                var res = new Vector
                {
                    Id = theId,
                    Counter = 1,
                    Values = value,
                };
                result[0] = res;
                return result;
            }
            if (vector[0].Equals(result[0]))
            {
                if (vectorId.Equals(theId))
                {
                    var newvalues = value;
                    newvalues.AddRange(oldvalues);
                    var res = new Vector
                    {
                        Id = vectorId,
                        Counter = counter + 1,
                        Values = newvalues
                    };
                    result[0] = res;
                    return result;
                }
                else
                {
                    var i = vectorId.CompareTo(theId);
                    if (i > 0)
                    {
                        var res = new Vector
                        {
                            Id = theId,
                            Counter = 1,
                            Values = value
                        };
                        result[0] = res;
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
            }
            result.Add(vector[0]);
            var eventValue = vector;
            eventValue.RemoveAt(0);
            var eventV = Entry(eventValue, theId, value);
            result.AddRange(eventV);
            return result;
        }


        public override int GetHashCode() => HashCode.Combine(Values, Entries);
        public override bool Equals(object obj) => base.Equals(obj);
    }
}

