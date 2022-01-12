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
using Newtonsoft.Json.Linq;

namespace LUC.DVVSet
{

    /// <summary>
    /// Constructor class
    /// Clock includes two values - Entries and ClockValues
    /// Entries is a sorted list, in which key is a string value and the values ​​are Vector value from the Vector class.
    /// </summary>
    [Serializable]
    public class Clock //{entries(), values()}
    {
        /// <summary>
        /// {id,{counter,{values}}
        /// Each operation with clock causes a counter increment. Id is a unique value, entries in the clock cannot have the same id value.
        /// </summary>
        public SortedList<String, Vector> Entries { get; set; }
        /// <summary>
        /// Array of strings with information about current Entry. 
        /// </summary>
        public List<String> ClockValues { get; set; }

        /// <summary>
        /// Default Clock with empty Entries and ClockValues
        /// </summary>
        public Clock()
        {
            Entries = new SortedList<String, Vector>();
            ClockValues = new List<String>();
        }

        /// <summary>
        /// Clock with prepared Entries and empty ClockValues
        /// </summary>
        /// <param name="entries">{id,{counter,{values}}</param>
        public Clock(SortedList<String, Vector> entries)
        {
            Entries = entries;
            ClockValues = new List<String>();
        }

        /// <summary>
        /// Clock with empty Entries and single ClockValue
        /// </summary>
        /// <param name="value">String value</param>
        public Clock(String value)
        {
            Entries = new SortedList<String, Vector>();
            ClockValues = new List<String> { value };
        }

        /// <summary>
        /// Clock with empty Entries and prepared ClockValues
        /// </summary>
        /// <param name="clockValues">List of string values</param>
        public Clock(List<String> clockValues)
        {
            Entries = new SortedList<String, Vector>();
            ClockValues = clockValues;
        }

        /// <summary>
        /// Makes a Clock with prepared Entries and ClockValues
        /// </summary>
        /// <param name="entries">{id,{counter,{values}}</param>
        /// <param name="clockValues">List of string values</param>
        public Clock(SortedList<String, Vector> entries, List<String> clockValues)
        {
            Entries = entries;
            ClockValues = clockValues;
        }

        /// <summary>
        /// Makes a Clock with prepared Entries and single ClockValue
        /// </summary>
        /// <param name="entries">{id,{counter,{values}}</param>
        /// <param name="value">String value</param>
        public Clock(SortedList<String, Vector> entries, String value)
        {
            Entries = entries;
            ClockValues = new List<String> { value };
        }

        /// <summary>
        /// Deconstructs Clock to two values, Entries and ClockValues.
        /// Usage: var (entries1, values1) = clock1;
        /// </summary>
        /// <param name="entries">Entries from Clock</param>
        /// <param name="values">ClockValues from Clock</param>
        public void Deconstruct(out SortedList<String, Vector> entries, out List<String> values)
        {
            entries = Entries;
            values = ClockValues;
        }

        /// <summary>
        /// Translates Clock to Json-string format. Can translate to Json-serializable format.
        /// </summary>
        /// <param name="clocks"></param>
        /// <param name="tests"></param>
        /// <returns></returns>
        public static String ClockToString(Object clocks, Boolean tests = false)
        {
            var result = "";
            var spacer = "";
            if (!tests)
                spacer = "\"";
            var clock = new Clock();
            if (tests && clocks.GetType() == typeof(SortedList<String, Vector>))
            {
                clock.Entries = clocks as SortedList<String, Vector>;
                clock.ClockValues = new List<String>();
            }

            if (clocks.GetType() == typeof(Clock))
            {
                clock = clocks as Clock;
            }
            if (clock.Entries.Count > 0)
            {
                var count = 0;
                foreach (var keyvalue in clock.Entries)
                {
                    if (tests)
                        result += "[{";
                    else
                        result += "[[[";
                    result += $"{spacer}{keyvalue.Key}{spacer},{keyvalue.Value.Counter},";
                    if (keyvalue.Value.Values.Count == 0)
                        result += "[]";
                    else
                        result = keyvalue.Value.Values.Aggregate(result, (current, i) =>
                        {
                            i = i.Replace("\r\n", "");
                            return $"{current}[{i}]";
                        });
                    if (tests)
                        result += "}],";
                    else
                        result += "]],";
                    //if (clock.ClockValues.Any()) result += "[" + clock.ClockValues[count] + "];";
                    //else result+="[];";
                    count++;
                }
            }
            else
            {
                result += "[],";
            }

            if (clock.ClockValues.Any())
                result += "[" + spacer + clock.ClockValues.Aggregate((current, i) => current + "[" + i + "]") + "]";
            else
                result += "[]";
            if (tests)
                result += ";";
            else
                result += "]";
            return result;
        }

        /// <summary>
        /// Translate Entries from sorted list to list of objects. This is alternative format of Clock, if needed.
        /// </summary>
        /// <param name="clocks"></param>
        /// <returns>List of objects</returns>
        public static List<Object> ClockToList(Object clocks)
        {
            var result = new List<Object>();
            var clock = new Clock();
            if (clocks.GetType() == typeof(SortedList<String, Vector>))
            {
                clock.Entries = clocks as SortedList<String, Vector>;
                clock.ClockValues = new List<String>();
            }
            if (clocks.GetType() == typeof(Clock))
            {
                clock = clocks as Clock;
            }

            if (clock.Entries.Count > 0)
            {
                //var listvalue = new ClockToList();
                foreach (var keyvalue in clock.Entries)
                {
                    var listvalue = new List<Object>
                    {
                        keyvalue.Key,
                        keyvalue.Value.Counter,
                        keyvalue.Value.Values
                    };
                    result.Add(listvalue);
                    //if (keyvalue.Value.Values.Count == 0)
                    //    result.Add(new List<String>());
                    //else
                    //    result.Add(keyvalue.Value.Values);
                }
            }
            else
            {
                result.Add(new List<String>());
            }
            if (clock.ClockValues.Any())
                result.Add(clock.ClockValues);
            else
                result.Add(new List<String>());
            return result;
        }

        /// <summary>
        /// Transforms Json-string to Clock value
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static Clock StringToClock(String version)
        {
            var testsarray = Encoding.UTF8.GetString(Convert.FromBase64String(version));

            var objectList = new JArray();
            try
            {
                objectList = JsonConvert.DeserializeObject<JArray>(testsarray);
            }
            catch (Exception)
            {
                return null;
            }
            return ObjectToClock(objectList);
        }

        private static Clock ObjectToClock(JArray clockList)
        {
            var incomeClock = new Clock();
            var incomeEntries = incomeClock.Entries;
            var cycleCount = 0;
            var foo = clockList.Values();

            foreach (var objectEntries in foo)
            {
                var clockValues = new ArrayList();

                //if recurse runs
                if (objectEntries.GetType().Equals(typeof(JValue)) || cycleCount > 0)
                {
                    cycleCount++;
                    switch (cycleCount)
                    {
                        case 1:
                            clockValues.Add(objectEntries.ToString());
                            break;
                        case 2:
                            try
                            {
                                clockValues.Add(Int32.Parse(objectEntries.ToString()));
                            }
                            catch
                            {
                                clockValues.Add(objectEntries.ToString());
                            }
                            break;
                        case 3:
                            clockValues.Add((IList)objectEntries);
                            break;
                        default:
                            break;
                    }
                    continue;
                }

                var entriesList = objectEntries.ToObject<JArray>();

                if (entriesList.Count == 3)
                {
                    var incomeVector = new Vector
                    {
                        Counter = (entriesList[1]).ToObject<int>(),
                        Values = new List<String>()
                    };

                    var values = entriesList[2];
                    foreach (var objectValues in values)
                    {
                        //var value = ClockToString(ObjectToClock(objectValues));
                        incomeVector.Values.Add(objectValues.ToString());
                    }

                    incomeEntries.Add(entriesList[0].ToString(), incomeVector);
                }
            }

            if (clockList.Count == 1)
            {
                incomeClock.ClockValues = new List<String>();
            }
            else
            {
                var clockValues = new List<String>();
                var values = (IList)clockList[1];

                foreach (var objectValues in values)
                {
                    clockValues.Add(objectValues.ToString());
                }
                incomeClock.ClockValues = clockValues;
            }
            return incomeClock;
        }
    }
}