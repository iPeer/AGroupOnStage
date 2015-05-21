using AGroupOnStage.ActionGroups;
using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{
    public class AGOSDebug : MonoBehaviour
    {

        private static bool guiVisible = false;
        private static bool hasSetupStyles = false;
        private static Rect _winPos = new Rect();

        public static void printAllActionGroups()
        {

            Logger.Log("Dumping action group info");
            foreach (IActionGroup a in AGOSMain.Instance.actionGroups)
                AGOSUtils.printActionGroupInfo(a);

        }

        public static void toggleGUI()
        {
            if (guiVisible)
            {
                guiVisible = false;
                RenderingManager.RemoveFromPostDrawQueue(AGOSMain.AGOS_DEBUG_GUI_WINDOW_ID, OnDraw);
            }
            else
            {
                guiVisible = true;
                RenderingManager.AddToPostDrawQueue(AGOSMain.AGOS_DEBUG_GUI_WINDOW_ID, OnDraw);
            }
        }

        public static void OnDraw()
        {


            _winPos = GUILayout.Window(AGOSMain.AGOS_DEBUG_GUI_WINDOW_ID, _winPos, OnWindow, "AGOS: Debug CHEATS!");

        }

        public static void OnWindow(int id)
        {



            GUILayout.BeginVertical(GUILayout.MinWidth(300f), GUILayout.MinHeight(400f));


            if (GUILayout.Button("Move current vessel to orbit"))
            {
                FlightGlobals.fetch.SetShipOrbit(Planetarium.fetch.CurrentMainBody.flightGlobalsIndex, 1.0d, 100d, 0d, 1d, 1d, 1d, 1d);
            }


            GUILayout.EndVertical();

            GUI.DragWindow();


        }

    }
}
