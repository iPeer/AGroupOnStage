using AGroupOnStage.ActionGroups;
using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.Main
{
    public class AGOSDebug
    {

        public static void printAllActionGroups()
        {

            Logger.Log("Dumping action group info");
            foreach (IActionGroup a in AGOSMain.Instance.actionGroups)
                AGOSUtils.printActionGroupInfo(a);

        }

    }
}
