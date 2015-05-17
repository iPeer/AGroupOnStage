using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.Extensions
{
    public static class RandomExtensions
    {

        /*
         * Super quick, and super dirty.
         * I call it "LogicalNext" because the default C# .Next is illogical (to me) with it being 0-(maxValue-1) instead of 0-maxValue.
         */

        /// <summary>
        /// Returns a random integer ranging from 0-<paramref name="maxValue"/>, unlike Random's default Next taht returns 0 to <paramref name="maxValue"/>-1.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="maxValue">Maximum possible return value for this random</param>
        /// <returns></returns>
        public static int LogicalNext(this Random r, int maxValue)
        {
            return r.Next(maxValue + 1);
        }

        /// <summary>
        /// Returns a Boolean with a  50% chance of being either true or false.
        /// </summary>
        /// <param name="r"></param>
        /// <returns>Boolean</returns>
        public static bool NextBool(this Random r)
        {
            return r.LogicalNext(1) == 0;
        }

        /// <summary>
        /// <see cref="NextBool"/>
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Boolean NextBoolean(this Random r)
        {
            return r.NextBool();
        }

        /// <summary>
        /// Returns a boolean based on a one in <paramref name="chance"/> chance of being true. If <paramref name="chance"/> is less than 3 it behaves the same as <see cref="NextBool"/>
        /// </summary>
        /// <param name="r">This <ref>System.Random</ref> object</param>
        /// <param name="chance">An Integer denoting that this is a 1 in *n* chance. For example using NextBoolOneIn(10) would have a 1 in 10 chance of being true</param>
        /// <returns>Boolean</returns>
        public static bool NextBoolOneIn(this Random r, int chance)
        {
            if (chance <= 2)
                return r.NextBool();
            return (r.Next(chance) == 0); // Default .Next actually makes sense here! D:
        }

    }
}
