// 
// A C# implementation of *compact* Dotted Version Vectors, which
// provides a container for a set of concurrent values (siblings) with causal
// order information.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace LUC.DVVSet
{
    public class Clock //{entries(), values()}
    {
        public SortedList<string, Vector> Entries { get; set; }
        public List<string> ClockValues { get; set; }

        public Clock()
        {
            Entries = new SortedList<string, Vector>();
            ClockValues = new List<string>();
        }

        public Clock(SortedList<string, Vector> entries)
        {
            Entries = entries;
            ClockValues = new List<string>();
        }

        public Clock(string value)
        {
            Entries = new SortedList<string, Vector>();
            ClockValues = new List<string> { value };
        }

        public Clock(List<string> clockValues)
        {
            Entries = new SortedList<string, Vector>();
            ClockValues = clockValues;
        }

        public Clock(SortedList<string, Vector> entries, List<string> clockValues)
        {
            Entries = entries;
            ClockValues = clockValues;
        }

        public Clock(SortedList<string, Vector> entries, string value)
        {
            Entries = entries;
            ClockValues = new List<string> { value };
        }


        public void Deconstruct(out SortedList<string, Vector> entries, out List<string> values)
        {
            entries = Entries;
            values = ClockValues;
        }

        public static string ClockToString(object clocks, bool tests = false)
        {
            var result = "";
            var spacer = "";
            if (!tests) spacer = "\"";
            var clock = new Clock();
            if (tests)
                if (clocks.GetType() == typeof(SortedList<string, Vector>))
                {
                    clock.Entries = clocks as SortedList<string, Vector>;
                    clock.ClockValues = new List<string>();
                }
            if (clocks.GetType() == typeof(Clock))
            {
                clock = clocks as Clock;
            }
            if (clock.Entries.Count > 0)
            {
                int count = 0;
                foreach (var keyvalue in clock.Entries)
                {
                    if (tests) result += "[{";
                    else result += "[[[";
                    result += $"{spacer}{keyvalue.Key}{spacer},{keyvalue.Value.Counter},";
                    if (keyvalue.Value.Values.Count == 0) result += "[]";
                    else result = keyvalue.Value.Values.Aggregate(result, (current, i) => $"{current}[{spacer}{i}{spacer}]");
                    if (tests) result += "}],";
                    else result += "]],";
                   //if (clock.ClockValues.Any()) result += "[" + clock.ClockValues[count] + "];";
                   //else result+="[];";
                   count++;
                }
            }
            else result += "[],";
            if (clock.ClockValues.Any()) result += "[" + spacer+clock.ClockValues.Aggregate((current, i) => current + "[" + i + "]") + "]";
            else result += "[]";
            if (tests) result += ";";
            else result += "]";
            return result;
        }

        public static List<object> ClockToList(object clocks)
        {
            var result = new List<object>();
            var clock = new Clock();
            if (clocks.GetType() == typeof(SortedList<string, Vector>))
            {
                clock.Entries = clocks as SortedList<string, Vector>;
                clock.ClockValues = new List<string>();
            }
            if (clocks.GetType() == typeof(Clock))
            {
                clock = clocks as Clock;
            }
            if (clock.Entries.Count > 0)
            {
                ClockToList listvalue = new ClockToList();
                foreach (var keyvalue in clock.Entries)
                {
                    listvalue.Key = keyvalue.Key;
                    listvalue.Counter = keyvalue.Value.Counter;
                    listvalue.Values = keyvalue.Value.Values;
                    result.Add(listvalue);
                    if (keyvalue.Value.Values.Count == 0) result.Add(new List<string>());
                    else result.Add(keyvalue.Value.Values);
                }
            }
            if (clock.ClockValues.Any()) result.Add(clock.ClockValues);
            return result;
        }

        public static Clock StringToClock(string version)
        {
            var jsonarray = Encoding.UTF8.GetString(Convert.FromBase64String(version));
            var objectList = JsonConvert.DeserializeObject<IList>(jsonarray);
            Clock incomeClock = new Clock();
            var incomeEntries = incomeClock.Entries;

            foreach (var objectEntries in (IList)objectList[0])
            {
                var entriesList = objectEntries as IList;
                Vector incomeVector = new Vector
                {
                    Counter = Convert.ToInt32(entriesList[1]),
                    Values = new List<string>()
                };

                if (entriesList.Count == 3)
                {
                    var values = (IList)entriesList[2];
                    foreach (var objectValues in values)
                    {
                        var value = objectValues.ToString();
                        incomeVector.Values.Add(value);
                    }
                }
                incomeEntries.Add(entriesList[0].ToString(), incomeVector);
            }

            if (objectList.Count == 1) incomeClock.ClockValues = new List<string>();
            else
            {
                var clockValues = new List<string>();
                var values = (IList)objectList[1];
                foreach (var objectValues in values)
                {
                    clockValues.Add(objectValues.ToString());
                }
                incomeClock.ClockValues = clockValues;
            }
            return incomeClock;
        }
    }

    public class ClockToList
    {
        public string Key { get; set; }
        public int Counter { get; set; }
        public List<string> Values { get; set; }

        public ClockToList(string key, int counter, List<string> values)
        {
            Key = key;
            Counter = counter;
            Values = values;
        }

        public ClockToList() { }
    }
}