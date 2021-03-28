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


        public bool IsNumeric(object obj)       //Implementation of Number class (Python, Java). If object is numeric => return true.
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
