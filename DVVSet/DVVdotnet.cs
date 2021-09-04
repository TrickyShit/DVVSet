// A C# implementation of *compact* Dotted Version Vectors, which
// provides a container for a set of concurrent values (siblings) with causal
// order information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace LUC.DVVSet
{
    /// <summary>
    /// Methods for work with DVVSet
    /// </summary>
    public class Dvvdotnet : Clock
    {
        const Int32 DifferentKeys = -100;
        const Int32 Counter1isBigger = 1;
        const Int32 Counter1islesser = -1;
        const Int32 Values1more = 10;
        const Int32 Values1fewer = -10;

        //* 
        /// <summary>
        /// Return a version vector that represents the causal history.
        /// </summary>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static SortedList<String, Vector> Join(Clock clock)
        {
            var result = new SortedList<String, Vector>();
            foreach (var keyvalue in clock.Entries) result.Add(keyvalue.Key, new Vector(keyvalue.Value.Counter, new List<String>()));
            return result;
        }

        /// <summary>
        /// Advances the causal history of the first clock with the given id, while synchronizing
        /// with the second clock, thus the new clock is causally newer than both clocks in the argument.
        /// </summary>
        /// <param name="clock1">First clock. SHOULD BE a direct result of new/2, which is intended to be the client clock 
        /// with the new value in the* anonymous dot*</param>
        /// <param name="theId">Given id</param>
        /// <param name="clock2">Second clock from the local server.</param>
        /// <returns>New value is the *anonymous dot* of the clock.</returns>
        public static Clock Update(Clock clock1, String theId, Clock clock2 = null)
        {
            // Sync both clocks without the new value
            var c1 = new Clock
            {
                Entries = Entry(clock1, theId),
                ClockValues = new List<String>()
            };
            if (clock2 == null) return c1;
            var (entries, _) = SyncClocks(c1, clock2);
            return new Clock(entries, new List<String>());
        }

        /// <summary>
        /// Created a new event on the synced causal history, with the id and the new value.
        /// The anonymous values that were synced still remain.
        /// </summary>
        /// <param name="clock"></param>
        /// <param name="theId"></param>
        /// <param name="value">Optional string value, if needed</param>
        /// <returns></returns>
        public static SortedList<String, Vector> Entry(Clock clock, String theId, String value = null)
        {
            var vector = new SortedList<String, Vector>();
            foreach (var i in clock.Entries) vector.Add(i.Key, i.Value);
            var values = new List<String>();
            if (value != null) values.Add(value);
            foreach (var i in clock.ClockValues) values.Add(i);
            var result = new SortedList<String, Vector>();
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
                var i = String.Compare(vectorId, theId, StringComparison.Ordinal);
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
            var eventValue = new SortedList<String, Vector>(vector);
            eventValue.RemoveAt(0);
            var eventV = Entry(new Clock(eventValue, values), theId);
            foreach (var keyvalue in eventV)
                result.Add(keyvalue.Key, keyvalue.Value);
            return result;
        }

        private static KeyValuePair<String, Vector> Merge(String id, Int32 count1, List<String> values1, Int32 count2, List<String> values2)
        {
            var values = new List<String>();
            var count = count1 > count2 ? count1 + 1 : count2 + 1;
            Int32 comparison;
            switch (values1.Count)
            {
            case 0:
                values.Add(values2[0]);
                break;
            default:
                if (values2.Count == 0)
                {
                    values.Add(values1[0]);
                }
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
                    else
                    {
                        values.Add(values1[0]);
                    }
                }

                break;
            }
            var value = new Vector(count, values);
            var result = new KeyValuePair<String, Vector>(id, value);
            return result;
        }

        //_________________________________________________________________
        // SortedList represents a collection of key/value pairs
        // that are automatically sorted by the key(ID) and are accessible
        // by key and by index. So method foldl is don't needed.
        //_________________________________________________________________

        /// <summary>
        /// Synchronizes a list of clocks. It discards (causally) outdated values, while merging all causal histories. 
        /// </summary>
        /// <param name="clock1"></param>
        /// <param name="clock2"></param>
        /// <returns>Synchronized Clock value</returns>
        public static Clock SyncClocks(Clock clock1, Clock clock2)
        {
            if (clock1 == null) return clock2;
            if (clock2 == null) return clock1;
            var result = new Clock();
            var (entries1, values1) = clock1;
            var (entries2, values2) = clock2;
            var syncvalue = new List<String>();
            if (Less(clock2, clock1) && values1.Any())
            {
                syncvalue.Add(values1.First());
            }
            else if (Less(clock1, clock2) && values2.Any())
            {
                syncvalue.Add(values2.First());
            }
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
            result.Entries = SyncEntries(new SortedList<String, Vector>(entries1), new SortedList<String, Vector>(entries2));
            return result;
        }

        private static SortedList<String, Vector> SyncEntries(SortedList<String, Vector> entry1, SortedList<String, Vector> entry2)
        {
            KeyValuePair<String, Vector> KeyValue(SortedList<String, Vector> entry)
            {
                var temp = entry.First();
                var newValue = new Vector(temp.Value.Counter, temp.Value.Values);
                return new KeyValuePair<String, Vector>(temp.Key, newValue);
            }

            if (!entry1.Any()) return entry2;
            if (!entry2.Any()) return entry1;

            var result = new SortedList<String, Vector>();
            var count1 = entry1.Count;
            var count2 = entry2.Count;
            var count = count1 > count2 ? count1 : count2;
            KeyValuePair<String, Vector> head1;
            KeyValuePair<String, Vector> head2;
            for (var c = 0; c < count; c++)
            {
                head1 = entry1.Count > 0 ? KeyValue(entry1) : new KeyValuePair<String, Vector>();
                head2 = entry2.Count > 0 ? KeyValue(entry2) : new KeyValuePair<String, Vector>();

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
                var temp = new List<String>();
                var headresult = new SortedList<String, Vector>();
                KeyValuePair<String, Vector> mergepair;
                switch (ComparePairs(head1, head2))
                {
                    case DifferentKeys:
                        result.Add(head2.Key, head2.Value);
                        result.Add(head1.Key, head1.Value);
                        break;
                    case Counter1isBigger:
                        if (head1.Value.Values.Count > 0) temp.Add(head1.Value.Values[0]);
                        head1.Value.Values = temp;
                        headresult.Add(head1.Key, head1.Value);
                        break;
                    case Counter1islesser:
                        headresult.Add(head2.Key, head2.Value);
                        if (headresult.Values[0].Values[0].Any()) temp.Add(headresult.Values[0].Values[0]);
                        headresult.Values[0].Values = temp;
                        break;
                    case Values1more:
                        if (head2.Value.Values.Count > 0 && head1.Value.Values.Count > 0)
                        {
                            mergepair = Merge(head2.Key, head2.Value.Counter, head2.Value.Values, head1.Value.Counter, head1.Value.Values);
                            headresult.Add(mergepair.Key, mergepair.Value);
                        }
                        else
                            if (head1.Value.Values.Count > 0)
                    {
                        headresult.Values[0].Values.Add(head1.Value.Values[0]);
                    }
                    else
                    {
                        headresult.Add(head1.Key, head1.Value);
                    }

                    break;
                    case Values1fewer:
                        if (head2.Value.Values.Count > 0 && head1.Value.Values.Count > 0)
                        {
                            mergepair = Merge(head2.Key, head2.Value.Counter, head2.Value.Values, head1.Value.Counter, head1.Value.Values);
                            headresult.Add(mergepair.Key, mergepair.Value);
                        }
                        else
                    {
                        headresult.Add(head2.Key, head2.Value);
                    }

                    break;
                    default:
                        if (!head1.Value.Values.Equals(head2.Value.Values) && head1.Value.Values.Count > 0)
                        {
                            mergepair = Merge(head1.Key, head1.Value.Counter, head1.Value.Values, head2.Value.Counter, head2.Value.Values);
                            result.Add(mergepair.Key, mergepair.Value);
                        }
                        else
                    {
                        result.Add(head1.Key, head1.Value);
                    }

                    break;
                }
                if (headresult.Count > 0) result.Add(headresult.Keys[0], headresult.Values[0]);
                if (entry2.Count > 0) entry2.RemoveAt(0);
                if (entry1.Count > 0) entry1.RemoveAt(0);
            }
            return result;
        }

        private static Int32 ComparePairs(KeyValuePair<String, Vector> pair1, KeyValuePair<String, Vector> pair2)
        {
            if (!pair1.Key.Equals(pair2.Key)) return DifferentKeys;
            if (pair1.Value.Counter > pair2.Value.Counter) return Counter1isBigger;
            if (pair1.Value.Counter < pair2.Value.Counter) return Counter1islesser;
            if (pair1.Value.Values.Count == 0 && pair2.Value.Values.Count == 0) return 0;
            if (pair1.Value.Values.Count == 0) return Values1more;
            if (pair2.Value.Values.Count == 0) return Values1fewer;
            if (pair1.Value.Values.Count > pair2.Value.Values.Count) return Values1more;
            if (pair2.Value.Values.Count > pair1.Value.Values.Count) return Values1fewer;
            return 0;
        }

        /// <summary>
        /// Returns True if the first clock is causally older than the second clock, thus values on the first clock are outdated.
        /// Returns False otherwise.
        /// </summary>
        /// <param name="clock1"></param>
        /// <param name="clock2"></param>
        /// <returns></returns>
        public static Boolean Less(Clock clock1, Clock clock2)
        {
            return Greater(clock2.Entries, clock1.Entries);
        }

        private static Boolean Greater(SortedList<String, Vector> vector1, SortedList<String, Vector> vector2)
        {
            if (!vector1.Any() && !vector2.Any())
            {
                return false;
            }
            if (!vector2.Any()) return true;
            if (!vector1.Any()) return false;
            //if (vector1.Count > vector2.Count) return true;
            //if (vector2.Count > vector1.Count) return false;
            var isStrict = false;
            var count = vector1.Count < vector2.Count ? vector1.Count : vector2.Count;
            var vector1isbigger = vector1.Count > vector2.Count;
            var node1 = vector1.ToArray()[count - 1];
            var node2 = vector2.ToArray()[count - 1];
            if (!node1.Key.Equals(node2.Key)) return false;
            switch (ComparePairs(node1, node2))
            {
                case Counter1isBigger:
                case Values1more:
                    if (vector1isbigger) return true;
                    isStrict = true;
                    break;
                case Counter1islesser:
                case Values1fewer:
                    if (vector1.Count < vector2.Count) return false;
                    isStrict = false;
                    break;

                default:
                    if (vector1isbigger) return true;
                    //if(isStrict&vector1isbigger)return true;
                    //if(!isStrict&!vector1isbigger)return false;
                    break;
            }
            KeyValuePair<String, Vector> node;
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

        /// <summary>
        /// Size of Clock value
        /// </summary>
        /// <param name="clock"></param>
        /// <returns>Returns the total number of values in this clock set.</returns>
        public static Int32 Size(Clock clock)
        {
            var result = 0;
            foreach(var i in clock.Entries)result += i.Value.Values.Count;
            result += clock.ClockValues.Count;
            return result;
        }

        /// <summary>
        /// Number of Entries in the Clock value
        /// </summary>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static Int32 Count(Clock clock)
        {
            return clock.Entries.Count;
        }

        /// <summary>
        /// List of ID in the Clock value
        /// </summary>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static List<String> Ids(Clock clock)
        {
            return clock.Entries.Keys as List<String>;
        }

        /// <summary>
        /// Returns all the values used in this clock set, including the anonymous values.
        /// </summary>
        /// <param name="clock"></param>
        /// <returns></returns>
        public static List<String> ListValues(Clock clock)
        {
            var result = new List<String>();
            foreach (var i in clock.Entries)
            {
                result.AddRange(i.Value.Values);
            }
            result.AddRange(clock.ClockValues);
            return result;
        }

        /// <summary>
        /// Compares the equality of both clocks, regarding only the causal histories, thus ignoring the values.
        /// </summary>
        /// <param name="clock1"></param>
        /// <param name="clock2"></param>
        /// <returns>True if Clock values are equal, or false.</returns>
        public static Boolean Equal(Clock clock1, Clock clock2)
        {
            return Equal(clock1.Entries, clock2.Entries);
        }

        private static Boolean Equal(SortedList<String, Vector> vector1, SortedList<String, Vector> vector2)
        {
            if (vector1.Count != vector2.Count) return false;
            for (var i = 0; i < vector2.Count; i++)
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
