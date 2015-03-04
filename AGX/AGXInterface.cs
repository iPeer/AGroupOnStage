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
            catch 
            { 
                return false; 
            }

        }

        public static void AGExtToggleGroup(int group)
        {
            Type calledType = Type.GetType("ActionGroupsExtended.AGExtExternal, AGExt");
            calledType.InvokeMember("AGXToggleGroup", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new System.Object[] { group });
        }

    }
}
