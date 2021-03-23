// 
// A C# implementation of *compact* Dotted Version Vectors, which
// provides a container for a set of concurrent values (siblings) with causal
// order information.
// 

using System;
//using NCalc;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DVVSet
{
    public static class DVVdotnet 
    {
        public static bool IsNumeric(this object obj)       //Implementation of Number class (Python, Java)
        {
            if (obj == null) return false;

            return obj switch
            {
                sbyte _ => true,
                byte _ => true,
                short _ => true,
                ushort _ => true,
                int _ => true,
                uint _ => true,
                long _ => true,
                ulong _ => true,
                float _ => true,
                double _ => true,
                decimal _ => true,
                _ => false,
            };
        }
        
        // 
        //     Allows to compare lists with strings, as in Erlang.
        //     ( list > string )
        //
        public static bool IsList(object o)     //проверка на принадлежность объекта о к типу данных List
        {
            if (o == null) return false;
            return o is IList
                   && o.GetType().IsGenericType
                   && o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }
        public static object cmp_fun(object a, object b) {
            if (a is string && b is string) {
                return a.Equals(b);
            }
            if (IsNumeric(a)==true&&IsNumeric(b)==true){
                return a.Equals(b);
            }
            if (IsList(a)==true && IsList(b)==true) {
                var a1=a[0];
                if (a.ElementAt(0)>0 && b!=null) {
                    if (IsNumeric(a[0])==true&&IsNumeric(b[0])==true) {
                        return a[0].Count > b[0].Count;
                    }
                    if (a[0] is List) {
                        return true;
                    }
                }
            }
            if (a is list && !(b is list)) {
                return true;
            }
            if (b is list) {
                return false;
            }
            return a.Count > b.Count;
        }
        
        

        
        // 
        //     Clock object.
        //     * Entries are sorted by id
        //     * Each counter also includes the number of values in that id
        //     * The values in each triple of entries are causally ordered
        //       and each new value goes to the head of the list
        //     
        public class Clock
            : list {
            
            public Clock(object entries, object values, Hashtable kwargs, params object [] args)
                : base(kwargs) {
                this.append(entries);
                this.append(values);
            }
            
            public virtual object _get_entries() {
                return this[0];
            }
            
            public object entries = property(_get_entries);
            
            public virtual object _get_values() {
                return this[1];
            }
            
            public object values = property(_get_values);
        }
        
        // 
        //     DVVSet helper object.
        //     
        public class DVVSet {
            
            // 
            //         Constructs a new clock set without causal history,
            //         and receives one value that goes to the anonymous list.
            //         
            public virtual object @new(object value) {
                return Clock(new List<object>(), new List<object> {
                    value
                });
            }
            
            // 
            //         Same as new, but receives a list of values, instead of a single value.
            //         
            public virtual object new_list(object value) {
                if (value is list) {
                    return Clock(new List<object>(), value);
                }
                return Clock(new List<object>(), new List<object> {
                    value
                });
            }
            
            // 
            //         Constructs a new clock set with the causal history
            //         of the given version vector / vector clock,
            //         and receives one value that goes to the anonymous list.
            //         The version vector SHOULD BE the output of join.
            //         
            public virtual object new_with_history(object vector = Vector, object value) {
                // defense against non-order preserving serialization
                var vectors = vector.OrderBy(functools.cmp_to_key(cmp_fun)).ToList();
                var entries = new List<object>();
                foreach (var _tup_1 in vectors) {
                    var i_value = _tup_1.Item1;
                    var number = _tup_1.Item2;
                    entries.append(new List<object> {
                        i_value,
                        number,
                        new List<object>()
                    });
                }
                return Clock(entries, value);
            }
            
            // 
            //         Same as new_with_history, but receives a list of values, instead of a single value.
            //         
            public virtual object new_list_with_history(object vector = Vector, object value) {
                if (!(value is list)) {
                    return this.new_list_with_history(vector, new List<object> {
                        value
                    });
                }
                return this.new_with_history(vector, value);
            }
            
            // 
            //         Synchronizes a list of clocks using _sync().
            //         It discards (causally) outdated values, while merging all causal histories.
            //         
            public virtual Clock sync(object clock = Clock) 
            {
                // 
                //     Erlang's implementation of lists:foldl/3
                //     
                Clock Foldl(Func<object, object> _sync, string[] xs, Clock acc) => xs.Reverse().Aggregate(acc, _sync); //TODO разобраться со сверткой в Питоне/Эрланге и сделать аналог
                return Foldl(this._sync, new List<object>(), clock);
            }
            
            public virtual Clock _sync(Clock clock1, Clock clock2) {
                if (!clock1) {
                    return clock2;
                }
                if (!clock2) {
                    return clock1;
                }
                var clock1_entires = clock1[0];
                var clock1_values = clock1[1];
                var clock2_entires = clock2[0];
                var clock2_values = clock2[1];
                if (this.less(clock1, clock2)) {
                    var values = clock2_values;
                } else if (this.less(clock2, clock1)) {
                    values = clock1_values;
                } else {
                    values = new HashSet<object>(clock1_values + clock2_values);
                    if (values) {
                        values = values.ToList();
                    } else {
                        values = new List<object>();
                    }
                }
                return new List<object> {
                    this._sync2(clock1_entires, clock2_entires),
                    values
                };
            }
            
            public virtual object _sync2(object entries1, object entries2) {
                if (!entries1) {
                    return entries2;
                }
                if (!entries2) {
                    return entries1;
                }
                var head1 = entries1[0];
                var head2 = entries2[0];
                if (cmp_fun(head2[0], head1[0])) {
                    return new List<object> {
                        head1
                    } + this._sync2(entries1[1], entries2);
                }
                if (cmp_fun(head1[0], head2[0])) {
                    return new List<object> {
                        head2
                    } + this._sync2(entries2[1], entries1);
                }
                var to_merge = head1 + new List<object> {
                    head2[1],
                    head2[2]
                };
                return new List<object> {
                    this._merge(to_merge)
                } + this._sync2(entries1[1], entries2[1]);
            }
            
            // 
            //         Returns [id(), counter(), values()]
            //         
            public virtual object _merge(
                object the_id,
                object counter1,
                object values1,
                object counter2,
                object values2) {
                var len1 = values1.Count;
                var len2 = values2.Count;
                if (counter1 >= counter2) {
                    if (counter1 - len1 >= counter2 - len2) {
                        return new List<object> {
                            the_id,
                            counter1,
                            values1
                        };
                    }
                    return new List<object> {
                        the_id,
                        counter1,
                        values1[::((counter1  -  counter2)  +  len2)]
                    };
                }
                if (counter2 - len2 >= counter1 - len1) {
                    return new List<object> {
                        the_id,
                        counter2,
                        values2
                    };
                }
                return new List<object> {
                    the_id,
                    counter2,
                    values2[::((counter2  -  counter1)  +  len1)]
                };
            }
            
            // 
            //         Return a version vector that represents the causal history.
            //         
            public virtual object join(object clock) {
                var values = clock[0];
                var result = new List<object>();
                foreach (var value in values) {
                    if (!value) {
                        continue;
                    }
                    result.append(new List<object> {
                        value[0],
                        value[1]
                    });
                }
                return result;
            }
            
            // 
            //         Advances the causal history with the given id.
            //         The new value is the *anonymous dot* of the clock.
            //         The client clock SHOULD BE a direct result of new.
            //         
            public virtual object create(object clock, object the_id) {
                var values = clock[1];
                if (values is list && values.Count > 0) {
                    values = clock[1][0];
                }
                return Clock(this.@event(clock[0], the_id, values), new List<object>());
            }
            
            // 
            //         Advances the causal history of the
            //         first clock with the given id, while synchronizing
            //         with the second clock, thus the new clock is
            //         causally newer than both clocks in the argument.
            //         The new value is the *anonymous dot* of the clock.
            //         The first clock SHOULD BE a direct result of new/2,
            //         which is intended to be the client clock with
            //         the new value in the *anonymous dot* while
            //         the second clock is from the local server.
            //         
            public virtual object update(object clock1, object clock2, object the_id) {
                // Sync both clocks without the new value
                new List<object> {
                    clock,
                    values
                } = this._sync(Clock(clock1.entries, new List<object>()), clock2);
                // We create a new event on the synced causal history,
                // with the id I and the new value.
                // The anonymous values that were synced still remain.
                var clock_values = clock1.values;
                if (clock1.values is list) {
                    clock_values = clock1.values[0];
                }
                return Clock(this.@event(clock, the_id, clock_values), values);
            }
            
            public virtual object @event(object vector, object the_id, object value) {
                object values;
                if (!vector) {
                    return new List<object> {
                        new List<object> {
                            the_id,
                            1,
                            new List<object> {
                                value
                            }
                        }
                    };
                }
                if (vector.Count > 0 && vector[0].Count > 0 && vector[0][0] == the_id) {
                    if (value is list) {
                        values = value + vector[0][2];
                    } else {
                        values = new List<object> {
                            value
                        } + vector[0][2];
                    }
                    return new List<object> {
                        new List<object> {
                            vector[0][0],
                            vector[0][1] + 1,
                            values
                        } + vector[1]
                    };
                }
                if (vector.Count > 0 && vector[0].Count > 0) {
                    if (vector[0][0] is list || vector[0][0].Count > the_id.Count) {
                        return new List<object> {
                            new List<object> {
                                the_id,
                                1,
                                new List<object> {
                                    value
                                }
                            }
                        } + vector;
                    }
                }
                return new List<object> {
                    vector[0]
                } + this.@event(vector[1], the_id, value);
            }
            
            // 
            //         Returns the total number of values in this clock set.
            //         
            public virtual object size(object clock) {
                var result = 0;
                foreach (var entry in clock.entries) {
                    result += entry[2].Count;
                }
                return result + clock.values.Count;
            }
            
            // 
            //         Returns all the ids used in this clock set.
            //         
            public virtual object ids(object clock) {
                return (from i in clock[0]
                    select i[0]).ToList();
            }
            
            // 
            //         Returns all the values used in this clock set,
            //         including the anonymous values.
            //         
            public virtual object values(object clock) {
                var lst = new List<object>();
                foreach (var entry in clock[0]) {
                    var value = entry[2];
                    if (!value) {
                        continue;
                    }
                    lst.append(value);
                }
                var flat_list = new List<object>();
                foreach (var sublist in lst) {
                    foreach (var item in sublist) {
                        flat_list.append(item);
                    }
                }
                return clock[1] + flat_list;
            }
            
            // 
            //         Compares the equality of both clocks, regarding
            //         only the causal histories, thus ignoring the values.
            //         
            public virtual object equal(object clock1, object clock2) {
                if (!(clock1 is list)) {
                    throw TypeError("clock1 should be a list");
                }
                if (!(clock2 is list)) {
                    throw TypeError("clock2 should be a list");
                }
                if (clock1.Count == 2 && clock2.Count == 2) {
                    return this._equal2(clock1[0], clock2[0]);
                }
                return this._equal2(clock1, clock2);
            }
            
            public virtual object _equal2(object vector1, object vector2) {
                if (!vector1 && !vector2) {
                    return true;
                }
                if (vector1.Count > 0 && vector1[0].Count > 0 && vector2.Count > 0 && vector2[0].Count > 0) {
                    if (vector1[0][0] == vector2[0][0]) {
                        if (vector1[0].Count > 1 && vector2[0].Count > 1 && vector1[0][1] == vector2[0][1]) {
                            if (vector1[0][2].Count == vector2[0][2].Count) {
                                return this._equal2(vector1[1], vector2[1]);
                            }
                        }
                    }
                }
                return false;
            }
            
            public virtual object _greater(object vector1 = Vector, object vector2 = Vector, object strict) {
                if (!vector1 && !vector2) {
                    return strict;
                }
                if (!vector2) {
                    return true;
                }
                if (!vector1) {
                    return false;
                }
                if (vector1[0][0] == vector2[0][0]) {
                    var dot_number1 = vector1[0][1];
                    var dot_number2 = vector2[0][1];
                    if (dot_number1 == dot_number2) {
                        return this._greater(vector1[1], vector2[1], strict);
                    }
                    if (dot_number1 > dot_number2) {
                        return this._greater(vector1[1], vector2[1], true);
                    }
                    if (dot_number1 < dot_number2) {
                        return false;
                    }
                }
                if (cmp_fun(vector2[0][0], vector1[0][0])) {
                    return this._greater(vector1[1], vector2, true);
                }
                return false;
            }
            
            // 
            //         Returns True if the first clock is causally older than
            //         the second clock, thus values on the first clock are outdated.
            //         Returns False otherwise.
            //         
            public virtual object less(object clock1, object clock2) {
                return this._greater(clock2[0], clock1[0], false);
            }
        }
    }
}
