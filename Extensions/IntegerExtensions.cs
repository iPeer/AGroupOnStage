using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.Extensions
{
    public static class IntegerExtensions
    {

        public static bool isWithinRange(this int val, int min, int max) 
        {
            return (val >= min && val <= max);
        }

    }
}
