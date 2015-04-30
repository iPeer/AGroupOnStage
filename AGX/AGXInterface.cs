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

        public static string getAGXGroupDesc(int group)
        {
            Type calledType;
            if (HighLogic.LoadedSceneIsEditor)
                calledType = Type.GetType("ActionGroupsExtended.AGXEditor, AGExt");
            else
                calledType = Type.GetType("ActionGroupsExtended.AGXFlight, AGExt");

            Dictionary<int, string> agxNames =
                (Dictionary<int, string>)calledType.InvokeMember("AGXguiNames", BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static, null, null, null);

            if (agxNames.Count < 0 || !agxNames.ContainsKey(group) || agxNames[group] == "") { return null; }
            return agxNames[group];
        }

        public static void AGExtToggleGroup(int group)
        {
            Type calledType = Type.GetType("ActionGroupsExtended.AGExtExternal, AGExt");
            calledType.InvokeMember("AGXToggleGroup", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new System.Object[] { group });
        }

    }
}
