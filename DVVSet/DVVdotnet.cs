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
    public class Dvvdotnet : Clock
    {

        //*******************************************************************************//
        //*   NewList constructs a new clock set without causal history, and            *//
        //*   receives one value or list of values that goes to the anonymous list.     *//
        //*******************************************************************************//
        public static Clock NewList(List<string> values = null, string value = "")
        {
            if (value == "") return new Clock(values);
            values = new List<string> { value };
            return new Clock(values);
        }

        //* NewWithHistory constructs a new clock set with the causal
        //* history of the given version vector / vector clock,
        //* and receives one value that goes to the anonymous list.
        //* The version vector SHOULD BE the output of join.

        protected static Clock NewWithHistory(SortedList<string, Vector> vector, string value = "", List<string> values = null)
        {
            if (value != "") values = new List<string> { value };
            return new Clock(vector, values);
        }

        // Method Create advances the causal history with the given id.
        // The new value is the *anonymous dot* of the clock.
        // The client clock SHOULD BE a direct result of method NewList.
        protected Clock Create(Clock clock, string theId)
        {
            var entries = clock.Entries;
            var values = clock.ClockValues;
            var result = Entry(entries, theId, new List<string> {values[0]});
            return new Clock(result, new List<string>());
        }

        //* Return a version vector that represents the causal history.

        protected static SortedList<string, Vector> Join(Clock clock)
        {
            var result = new SortedList<string, Vector>();
            foreach (var (key, value) in clock.Entries)
            {
                value.Values = new List<string>();
                result.Add(key, value);
            }
            return result;
        }

        /* Advances the causal history of the first clock with the given id, while synchronizing
        * with the second clock, thus the new clock is causally newer than both clocks in the argument.
        * The new value is the *anonymous dot* of the clock.
        * The first clock SHOULD BE a direct result of new/2, which is intended to be the client clock with
        * the new value in the *anonymous dot* while the second clock is from the local server.*/

        protected Clock Update(Clock clock1, Clock clock2, string theId)
        {
            // Sync both clocks without the new value
            var entries1 = clock1.Entries;
            var values1 = clock1.ClockValues;
            var c1 = new Clock { Entries = entries1, ClockValues = new List<string>() };
            var (entries, values) = SyncClocks(c1, clock2);
            // We create a new event on the synced causal history,
            // with the id I and the new value.
            // The anonymous values that were synced still remain.
            var entry = Entry(entries, theId, values1);
            return new Clock(entry, values);
        }


        // We create a new event on the synced causal history,
        // with the id and the new value.
        // The anonymous values that were synced still remain.
        private SortedList<string, Vector> Entry(SortedList<string, Vector> vector, string theId, List<string> values)
        {
            var result = new SortedList<string, Vector>();
            if (vector.Count == 0)
            {
                result.Add(theId,new Vector(1, values));
                return result;
            }
            if (vector.ContainsKey(theId))
            {
                var (mergeCounter, mergeValues) = vector[theId];
                int newCounter = mergeCounter + 1;
                var mergeclock = Merge(theId, mergeCounter, mergeValues, newCounter, values);
                return new SortedList<string, Vector>
                    {{mergeclock.Keys[0], mergeclock.Values[0]
    }
};
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
            var eventValue = vector;
            eventValue.RemoveAt(0);
            var eventV = Entry(eventValue, theId, values);
            foreach (var (key, vector1) in eventV)
                result.Add(key, vector1);
            return result;
        }

        private static SortedList<string, Vector> Merge(string id, int count1, List<string> values1, int count2, List<string> values2)
        {
            var len1 = values1?.Count ?? 0;
            var len2 = values2?.Count ?? 0;
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
                count = count2;
                values = count2 - len2 >= count1 - len1 ? values2 : values2.GetRange(0, count2 - count1 + len1);
            }
            var value = new Vector(count, values);
            result.Add(id, value);
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

        private Clock SyncClocks(Clock clock1, Clock clock2)
        {
            if (clock1 == null) return clock2;
            if (clock2 == null) return clock1;
            var result = new Clock();
            List<string> value;
            var (entries1, values1) = clock1;
            var (entries2, values2) = clock2;
            if (Greater(entries1, entries2, false)) value = values1;
            else
            if (Greater(entries2, entries1, false)) value = values2;
            else
            {
                value = values1;
                if (values1 == null)
                {
                    value = values2;
                }
                else value.AddRange(values2);
            }
            result.ClockValues = value.Distinct().ToList();      //Duplicate values are removed here
            result.Entries = SyncEntries(entries1, entries2);
            return result;
        }

        private SortedList<string, Vector> SyncEntries(SortedList<string, Vector> entry1, SortedList<string, Vector> entry2)
        {
            if (!entry1.Any()) return entry2;
            if (!entry2.Any()) return entry1;
            var result = new SortedList<string, Vector>();
            var head1 = new KeyValuePair<string, Vector>(entry1.First().Key, entry1.First().Value);
            var head2 = new KeyValuePair<string, Vector>(entry2.First().Key, entry2.First().Value);
            var subList1 = entry1;
            subList1.Remove(subList1.First().Key);
            var subList2 = entry2;
            subList2.Remove(subList2.First().Key);
            if (CompareEntries(entry2, entry1) > 0)
            {
                result.Add(head1.Key, head1.Value);
                var toAppend = SyncEntries(subList1, entry2);
                result = (SortedList<string, Vector>)result.Concat(toAppend);
                return result;
            }
            if (CompareEntries(entry1, entry2) > 0)
            {
                result.Add(head2.Key, head2.Value);
                var toAppend = SyncEntries(subList2, entry1);
                result = (SortedList<string, Vector>)result.Concat(toAppend);
                return result;
            }
            var mergeResult = Merge(head1.Key, head1.Value.Counter, head1.Value.Values, head2.Value.Counter, head2.Value.Values);
            foreach (var i in mergeResult) result.Add(i.Key, i.Value);
            var syncResult = SyncEntries(subList1, subList2);
            foreach (var i in syncResult) result.Add(i.Key, i.Value);
            return result;
        }

        private static int CompareEntries(SortedList<string, Vector> entries1, SortedList<string, Vector> entries2)
        {
            int counter = 0;
            foreach (var va in entries1)
            {
                var vb = entries2.ToArray()[counter];
                try
                {
                }
                catch (Exception)               //if size(va) > size(vb) and all values in vb are compared => return 1
                {
                    return 1;
                }
                counter++;
                var s1 = va.Value.Values.Count;     //length of values list in vector
                var s2 = vb.Value.Values.Count;
                if ((s1 > 0) && (s2 > 0))
                {
                    if (s1 > s2) return 1;
                    if (s1 == s2) continue;
                    return -1;
                }
                if (s1 != 0) return 1;
                if (s2 != 0) return -1;
                return 0;
            }
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
                counter++;
                var value1 = node1.Key;
                var value2 = node2.Key;
                if (!value1.Equals(value2)) continue;
                var dotNum1 = node1.Value.Counter;
                var dotNum2 = node2.Value.Counter;
                if (dotNum1 > dotNum2) return true;
                if (dotNum1 < dotNum2) return false;
                if (string.Compare(value2, value1, StringComparison.Ordinal) > 0) continue;
                return false;
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
        public bool Equal(Clock clock1, Clock clock2) => Equal(clock1.Entries, clock2.Entries);

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
