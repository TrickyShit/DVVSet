// A C# implementation of *compact* Dotted Version Vectors, which
// provides a container for a set of concurrent values (siblings) with causal
// order information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace DVVSet
{
    public class Dvvdotnet : Clock
    {
        const int differentkeys = -100;
        const int counter1isbigger = 1;
        const int counter1islesser = -1;
        const int values1more = 10;
        const int values1fewer = -10;

        //* Return a version vector that represents the causal history.

        protected static SortedList<string, Vector> Join(Clock clock)
        {
            var result = new SortedList<string, Vector>();
            foreach ((string key, Vector value) in clock.Entries) result.Add(key, new Vector(value.Counter, new List<string>()));
            return result;
        }

        /* Advances the causal history of the first clock with the given id, while synchronizing
        * with the second clock, thus the new clock is causally newer than both clocks in the argument.
        * The new value is the *anonymous dot* of the clock.
        * The first clock SHOULD BE a direct result of new/2, which is intended to be the client clock with
        * the new value in the *anonymous dot* while the second clock is from the local server.*/

        protected Clock Update(Clock clock1, string theId, Clock clock2 = null)
        {
            // Sync both clocks without the new value
            var c1 = new Clock
            {
                Entries = Entry(clock1, theId),
                ClockValues = new List<string>()
            };
            if (clock2 == null) return c1;
            var (entries, _) = SyncClocks(c1, clock2);
            return new Clock(entries, new List<string>());
        }

        // We create a new event on the synced causal history,
        // with the id and the new value.
        // The anonymous values that were synced still remain.
        public SortedList<string, Vector> Entry(Clock clock, string theId, string value = null)
        {
            var vector = new SortedList<string, Vector>();
            foreach (var i in clock.Entries) vector.Add(i.Key, i.Value);
            var values = new List<string>();
            if (value != null) values.Add(value);
            foreach (var i in clock.ClockValues) values.Add(i);
            var result = new SortedList<string, Vector>();
            if (vector.Count == 0)
            {
                result.Add(theId, new Vector(1, values));
                return result;
            }
            if (vector.ContainsKey(theId))
            {
                var mergeCounter = vector[theId].Counter;
                var mergeValues = vector[theId].Values;
                var mergeclock = Merge(theId, mergeCounter, mergeValues, mergeCounter, values);
                result.Add(mergeclock.Key, mergeclock.Value);
                return result;
            }
            else
            {
                var vectorId = vector.Keys[0];
                var i = string.Compare(vectorId, theId, StringComparison.Ordinal);
                if (i > 0)
                {
                    var newVector = new Vector { Counter = 1, Values = values };
                    result.Add(theId, newVector);
                    foreach (var v in vector)
                        result.Add(v.Key, v.Value);
                    return result;
                }
            }
            result.Add(vector.Keys[0], vector.Values[0]);
            var eventValue = new SortedList<string, Vector>(vector);
            eventValue.RemoveAt(0);
            var eventV = Entry(new Clock(eventValue, values), theId);
            foreach (var (key, vector1) in eventV)
                result.Add(key, vector1);
            return result;
        }

        private static KeyValuePair<string, Vector> Merge(string id, int count1, List<string> values1, int count2, List<string> values2)
        {
            List<string> values = new List<string>();
            int count = count1 > count2 ? count1 + 1 : count2 + 1;
            int comparison;
            if (values1.Count == 0) values.Add(values2[0]);
            else
            if (values2.Count == 0) values.Add(values1[0]);
            else
            {
                comparison = String.Compare(values1[0], values2[0], comparisonType: StringComparison.Ordinal);
                if (comparison > 0)
                {
                    values.Add(values1[0]);
                    values.Add(values2[0]);
                }
                else if (comparison < 0)
                {
                    values.Add(values2[0]);
                    values.Add(values1[0]);
                }
                else values.Add(values1[0]);
            }
            var value = new Vector(count, values);
            var result = new KeyValuePair<string, Vector>(id, value);
            return result;
        }

        //_________________________________________________________________
        // SortedList represents a collection of key/value pairs
        // that are automatically sorted by the key(ID) and are accessible
        // by key and by index. So method foldl is don't needed.
        //_________________________________________________________________


        //*******************************************************************************//
        //* Synchronizes a list of clocks using Sync().                                 *// 
        //* It discards (causally) outdated values, while merging all causal histories. *//

        public static Clock SyncClocks(Clock clock1, Clock clock2)
        {
            if (clock1 == null) return clock2;
            if (clock2 == null) return clock1;
            var result = new Clock();
            var (entries1, values1) = clock1;
            var (entries2, values2) = clock2;
            var syncvalue = new List<string>();
            if (Less(clock2, clock1) && values1.Any()) syncvalue.Add(values1.First());
            else if (Less(clock1, clock2) && values2.Any()) syncvalue.Add(values2.First());
            else
            {
                if ((values1.Count > 0) && (values2.Count > 0))
                {
                    syncvalue = values1;
                    syncvalue.AddRange(values2);
                }
                else
                {
                    if (values2.Count > 0) syncvalue = values2;
                }
            }
            result.ClockValues = syncvalue;
            result.Entries = SyncEntries(new SortedList<string, Vector>(entries1), new SortedList<string, Vector>(entries2));
            return result;
        }

        public static SortedList<string, Vector> SyncEntries(SortedList<string, Vector> entry1, SortedList<string, Vector> entry2)
        {
            static KeyValuePair<string, Vector> KeyValue(SortedList<string, Vector> entry)
            {
                var temp = entry.First();
                Vector newValue = new Vector(temp.Value.Counter, temp.Value.Values);
                return new KeyValuePair<string, Vector>(temp.Key, newValue);
            }

            if (!entry1.Any()) return entry2;
            if (!entry2.Any()) return entry1;
            var result = new SortedList<string, Vector>();
            var count1 = entry1.Count;
            var count2 = entry2.Count;
            int count = count1 > count2 ? count1 : count2;
            KeyValuePair<string, Vector> head1;
            KeyValuePair<string, Vector> head2;
            for (int c = 0; c < count; c++)
            {
                head1 = entry1.Count > 0 ? KeyValue(entry1) : new KeyValuePair<string, Vector>();
                head2 = entry2.Count > 0 ? KeyValue(entry2) : new KeyValuePair<string, Vector>();

                if (head1.Key == null)
                {
                    result.Add(head2.Key, head2.Value);
                    continue;
                }

                if (head2.Key == null)
                {
                    result.Add(head1.Key, head1.Value);
                    continue;
                }
                List<string> temp = new List<string>();
                var headresult = new SortedList<string, Vector>();
                KeyValuePair<string, Vector> mergepair;
                switch (ComparePairs(head1, head2))
                {
                    case differentkeys:
                        result.Add(head2.Key, head2.Value);
                        result.Add(head1.Key, head1.Value);
                        break;
                    case counter1isbigger:
                        if (head1.Value.Values.Count > 0) temp.Add(head1.Value.Values[0]);
                        head1.Value.Values = temp;
                        headresult.Add(head1.Key, head1.Value);
                        break;
                    case counter1islesser:
                        headresult.Add(head2.Key, head2.Value);
                        if (headresult.Values[0].Values[0].Any()) temp.Add(headresult.Values[0].Values[0]);
                        headresult.Values[0].Values = temp;
                        break;
                    case values1more:
                        if (head2.Value.Values.Count > 0 && head1.Value.Values.Count > 0)
                        {
                            mergepair = Merge(head2.Key, head2.Value.Counter, head2.Value.Values, head1.Value.Counter, head1.Value.Values);
                            headresult.Add(mergepair.Key, mergepair.Value);
                        }
                        else
                            if (head1.Value.Values.Count > 0) headresult.Values[0].Values.Add(head1.Value.Values[0]);
                        else headresult.Add(head1.Key, head1.Value);

                        break;
                    case values1fewer:
                        if (head2.Value.Values.Count > 0 && head1.Value.Values.Count > 0)
                        {
                            mergepair = Merge(head2.Key, head2.Value.Counter, head2.Value.Values, head1.Value.Counter, head1.Value.Values);
                            headresult.Add(mergepair.Key, mergepair.Value);
                        }
                        else headresult.Add(head2.Key, head2.Value);
                        break;
                    default:
                        if (!head1.Value.Values.Equals(head2.Value.Values) && head1.Value.Values.Count > 0)
                        {
                            mergepair = Merge(head1.Key, head1.Value.Counter, head1.Value.Values, head2.Value.Counter, head2.Value.Values);
                            result.Add(mergepair.Key, mergepair.Value);
                        }
                        else result.Add(head1.Key, head1.Value);
                        break;
                }
                if (headresult.Count > 0) result.Add(headresult.Keys[0], headresult.Values[0]);
                if (entry2.Count > 0) entry2.RemoveAt(0);
                if (entry1.Count > 0) entry1.RemoveAt(0);
            }
            return result;
        }


        private static int ComparePairs(KeyValuePair<string, Vector> pair1, KeyValuePair<string, Vector> pair2)
        {
            if (!pair1.Key.Equals(pair2.Key)) return differentkeys;
            if (pair1.Value.Counter > pair2.Value.Counter) return counter1isbigger;
            if (pair1.Value.Counter < pair2.Value.Counter) return counter1islesser;
            if (pair1.Value.Values.Count == 0 && pair2.Value.Values.Count == 0) return 0;
            if (pair1.Value.Values.Count == 0) return values1more;
            if (pair2.Value.Values.Count == 0) return values1fewer;
            if (pair1.Value.Values.Count > pair2.Value.Values.Count) return values1more;
            if (pair2.Value.Values.Count > pair1.Value.Values.Count) return values1fewer;
            return 0;
        }

        /* Returns True if the first clock is causally older than
        * the second clock, thus values on the first clock are outdated.
        * Returns False otherwise.*/

        public static bool Less(Clock clock1, Clock clock2) => Greater(clock2.Entries, clock1.Entries);

        private static bool Greater(SortedList<string, Vector> vector1, SortedList<string, Vector> vector2)
        {
            if (!vector1.Any() && !vector2.Any())
            {
                return false;
            }
            if (!vector2.Any()) return true;
            if (!vector1.Any()) return false;
            //if (vector1.Count > vector2.Count) return true;
            //if (vector2.Count > vector1.Count) return false;
            bool isStrict = false;
            int count = vector1.Count < vector2.Count ? vector1.Count : vector2.Count;
            bool vector1isbigger = vector1.Count > vector2.Count;
            var node1 = vector1.ToArray()[count - 1];
            var node2 = vector2.ToArray()[count - 1];
            if (!node1.Key.Equals(node2.Key)) return false;
            switch (ComparePairs(node1, node2))
            {
                case counter1isbigger:
                case values1more:
                    if (vector1isbigger) return true;
                    isStrict = true;
                    break;
                case counter1islesser:
                case values1fewer:
                    if (vector1.Count < vector2.Count) return false;
                    isStrict = false;
                    break;

                default:
                    if (vector1isbigger) return true;
                    //if(isStrict&vector1isbigger)return true;
                    //if(!isStrict&!vector1isbigger)return false;
                    break;
            }
            KeyValuePair<string, Vector> node;
            if (vector1isbigger)
            {
                node = vector1.ToArray()[count];
                if (!node.Key.Equals(node2.Key)) return false;
            }
            else if (vector1.Count < vector2.Count)
            {
                node = vector2.ToArray()[count];
                if (!node.Key.Equals(node1.Key)) return false;
            }
            return isStrict;
        }

        //Returns the total number of values in this clock set.
        public static int Size(Clock clock)
        {
            int result = 0;
            foreach(var i in clock.Entries)result += i.Value.Values.Count;
            result += clock.ClockValues.Count;
            return result;
        }

        public static List<string> Ids(Clock clock) => clock.Entries.Keys as List<string>;

        /*
        * Returns all the values used in this clock set,
        * including the anonymous values.
        */
        public static List<string> ListValues(Clock clock)
        {
            var result = new List<string>();
            foreach (var i in clock.Entries)
            {
                result.AddRange(i.Value.Values);
            }
            result.AddRange(clock.ClockValues);
            return result;
        }

        /*
         * Compares the equality of both clocks, regarding
         * only the causal histories, thus ignoring the values.
         */
        public static bool Equal(Clock clock1, Clock clock2) => Equal(clock1.Entries, clock2.Entries);

        private static bool Equal(SortedList<string, Vector> vector1, SortedList<string, Vector> vector2)
        {
            if (vector1.Count != vector2.Count) return false;
            for (int i = 0; i < vector2.Count; i++)
            {
                var node1 = vector1.ToArray()[i];
                var node2 = vector2.ToArray()[i];
                if (node1.Value.Counter != node2.Value.Counter) return false;
                if (!node1.Key.Equals(node2.Key)) return false;
            }
            return true;
        }
    }
}
