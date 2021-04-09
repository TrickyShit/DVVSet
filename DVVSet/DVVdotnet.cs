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
            foreach ((string key, Vector value) in clock.Entries)result.Add(key, new Vector(value.Counter, new List<string>()));
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
        private SortedList<string, Vector> Entry(Clock clock, string theId)
        {
            var vector = new SortedList<string, Vector>();
            foreach (var i in clock.Entries) vector.Add(i.Key, i.Value);
            var values = new List<string>();
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
                return new SortedList<string, Vector> { { mergeclock.Key, mergeclock.Value } };
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
                comparison = String.Compare(values1[0], values2[0], comparisonType: StringComparison.OrdinalIgnoreCase);
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
            else
            if (Less(clock1, clock2) && values2.Any()) syncvalue.Add(values2.First());
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
                List<string> temp;
                var headresult = new SortedList<string, Vector>();
                KeyValuePair<string, Vector> mergepair;
                switch (ComparePairs(head1, head2))
                {
                    case differentkeys:
                        result.Add(head2.Key, head2.Value);
                        result.Add(head1.Key, head1.Value);
                        break;
                    case counter1isbigger:
                        headresult.Add(head1.Key, head1.Value);
                        temp = new List<string> { headresult.Values[0].Values[0] };
                        headresult.Values[0].Values = temp;
                        break;
                    case counter1islesser:
                        headresult.Add(head2.Key, head2.Value);
                        temp = new List<string> { headresult.Values[0].Values[0] };
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
                        break;
                    case values1fewer:
                        if (head2.Value.Values.Count > 0 && head1.Value.Values.Count > 0)
                        {
                            mergepair = Merge(head2.Key, head2.Value.Counter, head2.Value.Values, head1.Value.Counter, head1.Value.Values);
                            headresult.Add(mergepair.Key, mergepair.Value);
                        }
                        else headresult.Add(head1.Key, head1.Value);
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
            if (pair1.Value.Values == null && pair2.Value.Values == null) return 0;
            if (pair1.Value.Values == null) return values1fewer;
            if (pair2.Value.Values == null) return values1more;
            if (pair1.Value.Values.Count > pair2.Value.Values.Count) return values1more;
            if (pair2.Value.Values.Count > pair1.Value.Values.Count) return values1fewer;
            return 0;
        }

        /* Returns True if the first clock is causally older than
        * the second clock, thus values on the first clock are outdated.
        * Returns False otherwise.*/

        public static bool Less(Clock clock1, Clock clock2) => Greater(clock2.Entries, clock1.Entries, false);

        private static bool Greater(SortedList<string, Vector> vector1, SortedList<string, Vector> vector2, bool isStrict)
        {
            if (!vector1.Any() && !vector2.Any())
            {
                return isStrict;
            }
            if (!vector2.Any()) return true;
            if (!vector1.Any()) return false;
            int counter = 0;
            foreach (var node1 in vector1)
            {
                var node2 = vector2.ToArray()[counter];
                try
                {
                }
                catch (Exception)
                {
                    return true;
                }
                var value1 = node1.Key;
                var value2 = node2.Key;
                if (!value1.Equals(value2)) continue;
                var dotNum1 = node1.Value.Counter;
                var dotNum2 = node2.Value.Counter;
                if (dotNum1 > dotNum2) return true;
                if (dotNum1 < dotNum2) return false;
                if (string.Compare(value2, value1, StringComparison.Ordinal) > 0) continue;
                if (vector1.Count > counter) return true;
                if (vector2.Count > counter) return false;
            }
            return false;
        }

        //Returns the total number of values in this clock set.
        public static int Size(Clock clock)
        {
            var result = clock.Entries.Values.Count(i => i.Values.Any());
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
            var entries = clock.Entries;
            foreach (var i in entries)
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
            if (vector1.Any() && vector2.Any()) return true;
            var (key1, (counter1, list1)) = vector1.First();
            var (key2, (counter2, list2)) = vector2.First();
            if (counter1 == 0 || counter2 == 0) return false;
            if (!key1.Equals(key2)) return false;

            if (list1.Count == list2.Count)
            {
                var subList1 = vector1;
                subList1.Remove(subList1.First().Key);
                var subList2 = vector2;
                subList2.Remove(subList2.First().Key);

                return Equal(subList1, subList2);
            }
            return false;
        }
    }
}
