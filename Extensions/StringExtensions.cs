using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.Extensions
{
    public static class StringExtensions
    {

        public static bool isInt32(this string s)
        {
            int a;
            return Int32.TryParse(s, out a);
        }

        public static int toInt32(this string s)
        {
            if (s.isInt32())
                return Convert.ToInt32(s);
            return 0;
        }

        // Because to ignore case, String.StartsWith() requires 3 parameters that I don't always want to provide.
        public static bool StartsWithIgnoreCase(this string s, string start)
        {
            return s.ToLower().StartsWith(start.ToLower());
        }

    }
}
