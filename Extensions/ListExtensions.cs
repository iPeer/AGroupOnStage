using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Returns the second-to-last object in a List. If the list only has one entry, it will return that entry.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="list">this list</param>
        /// <returns></returns>
        public static T LastButOne<T>(this List<T> list)
        {
            if (list.Count < 2)
                return list.First();
            return list[list.Count - 2];
        }

    }
}
