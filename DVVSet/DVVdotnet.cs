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
    public class DVVdotnet
    {
        public DVVdotnet()
        {
        }

        /*
        * Constructs a new clock set without causal history,
        * and receives one value that goes to the anonymous list.
        */
        public Clock NewDvv(string value)
        {
            List<string> values = new List<string>{value};
            return new Clock(values);
        }

        //Same as new, but receives a list of values, instead of a single value.
        public Clock NewList(List<string> values)=>new Clock(values);



        public bool IsNumeric(object obj)       //Implementation of Number class (Python, Java)
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
    }
}
