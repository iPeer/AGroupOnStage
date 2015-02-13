using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AGroupOnStage.AGX
{
    public class AGXInterface
    {

        public static bool isAGXInstalled() // Is AGX running on this instance?
        {

            try
            {
                Type calledType = Type.GetType("ActionGroupsExtended.AGExtExternal, AGExt");
                return (bool)calledType.InvokeMember("AGXInstalled", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
            }
            catch { 
                return false; 
            }

        }

        public static bool AGXFireAG(uint flightID, int group) // Activate the action group via AGX
        {
            Type calledType = Type.GetType("ActionGroupsExtended.AGExtExternal, AGExt");
            bool success = (bool)calledType.InvokeMember("AGX2VslToggleGroup", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new System.Object[] { flightID, group });
            return success;
        }

    }
}
