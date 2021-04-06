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
        const int falsecondition = -10;

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
            var entries = new SortedList<string, Vector>(clock.Entries);
            var values = new List<string>(clock.ClockValues);
            var result = Entry(entries, theId, values);
            return new Clock(result, new List<string>());
        }

        //* Return a version vector that represents the causal history.

        protected static SortedList<string, Vector> Join(Clock clock)
        {
            var result = new SortedList<string, Vector>();
            foreach (var (key, value) in clock.Entries)
            {
                var resvalue = new Vector(value.Counter, new List<string>());
                result.Add(key, resvalue);
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
            var entries1 = new SortedList<string, Vector>(clock1.Entries);
            var values1 = new List<string>(clock1.ClockValues);
            var entry = Entry(entries1, theId, values1);
            var c1 = new Clock { Entries = entry, ClockValues = new List<string>() };
            var (entries, _) = SyncClocks(c1, clock2);
            return new Clock(entries, new List<string>());
        }


        // We create a new event on the synced causal history,
        // with the id and the new value.
        // The anonymous values that were synced still remain.
        private SortedList<string, Vector> Entry(SortedList<string, Vector> vector, string theId, List<string> values)
        {
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
                //int newCounter = mergeCounter + 1;    was 4th position in Merge
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
            var eventValue = vector;
            eventValue.RemoveAt(0);
            var eventV = Entry(eventValue, theId, values);
            foreach (var (key, vector1) in eventV)
                result.Add(key, vector1);
            return result;
        }

        private static KeyValuePair<string, Vector> Merge(string id, int count1, List<string> values1, int count2, List<string> values2)
        {
            List<string> values = new List<string>();
            int count;
            if (count1 > count2)
            {
                count = count1 + 1;
                values.AddRange(values2);
                values.AddRange(values1);
            }
            else
            {
                count = count2 + 1;
                values.AddRange(values1);
                values.AddRange(values2);
            }
            var value = new Vector(count, values);
            var result = new KeyValuePair<string, Vector>(id, value);
            return result;
            //if (count1 >= count2)
            //{
            //    count = count1;
            //    values = count1 - len1 >= count2 - len2 ? values1 : values1.GetRange(0, count1 - count2 + len2);
            //}
            //else
            //{
            //    count = count2;
            //    values = count2 - len2 >= count1 - len1 ? values2 : values2.GetRange(0, count2 - count1 + len1);
            //}
            //var value = new Vector(count, values);
            //result.Add(id, value);
            //return result;
        }

        //_________________________________________________________________
        // SortedList represents a collection of key/value pairs
        // that are automatically sorted by the key(ID) and are accessible
        // by key and by index. So method foldl is don't needed.
        //_________________________________________________________________


        //*******************************************************************************//
        //* Synchronizes a list of clocks using Sync().                                 *// 
        //* It discards (causally) outdated values, while merging all causal histories. *//

        public Clock SyncClocks(Clock clock1, Clock clock2)
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
                    else syncvalue = values1;
                }
            }
            result.ClockValues = syncvalue;//syncvalue.Distinct().ToList();      //Duplicate values are removed here
            result.Entries = SyncEntries(new SortedList<string, Vector>(entries1), new SortedList<string, Vector>(entries2));
            return result;
        }

        public SortedList<string, Vector> SyncEntries(SortedList<string, Vector> entry1, SortedList<string, Vector> entry2)
        {
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
                if (entry1.Count > 0)
                {
                    head1 = new KeyValuePair<string, Vector>(entry1.First().Key, entry1.First().Value);
                    if (head1.Value.Values.Count > 1)
                        for (int i = 1; i < head1.Value.Values.Count; i++) head1.Value.Values.RemoveAt(i);    //lasts newest value only
                }
                else head1 = new KeyValuePair<string, Vector>();

                if (entry2.Count > 0)
                {
                    head2 = new KeyValuePair<string, Vector>(entry2.First().Key, entry2.First().Value);
                    if (head2.Value.Values.Count > 1)
                        for (int i = 1; i < head2.Value.Values.Count; i++) head2.Value.Values.RemoveAt(i);    //lasts newest value only
                }
                else head2 = new KeyValuePair<string, Vector>();

                var comparePairs = ComparePairs(head2, head1);
                if (comparePairs == 1)
                {
                    result.Add(head2.Key, head2.Value);
                }
                if (comparePairs == -1)
                {
                    result.Add(head1.Key, head1.Value);
                }
                if (comparePairs == 0)
                {
                    var mergePair = Merge(head1.Key, head1.Value.Counter, head1.Value.Values, head2.Value.Counter, head2.Value.Values);
                    result.Add(mergePair.Key, mergePair.Value);
                }
                if (entry2.Count > 0) entry2.RemoveAt(0);
                if (entry1.Count > 0) entry1.RemoveAt(0);
            }
            return result;
        }

        private static int ComparePairs(KeyValuePair<string, Vector> pair1, KeyValuePair<string, Vector> pair2)
        {
            if (pair1.Key == null) return -1;
            if (pair2.Key == null) return 1;
            if (!pair1.Key.Equals(pair2.Key)) return falsecondition;
            if (pair1.Value.Counter > pair2.Value.Counter) return 1;
            if (pair1.Value.Counter < pair2.Value.Counter) return -1;
            if (pair1.Value.Values.Count > pair2.Value.Values.Count) return -1;
            if (pair2.Value.Values.Count > pair1.Value.Values.Count) return 1;
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
