using AGroupOnStage.ActionGroups;
using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class AGOSDebug : MonoBehaviour
    {

        private static bool guiVisible = false;
        private static Rect _winPos = new Rect();
        private static Vector2 scrollPos = Vector2.zero;

        public void Start()
        {
            if (isDebugBuild())
                HighLogic.fetch.showConsole = true;
        }

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
                Orbit oldOrbit = FlightGlobals.fetch.activeVessel.orbit;
                Orbit newOrbit = new Orbit(0.0, 1.0, 400000, oldOrbit.LAN, 0.0, Mathf.PI, Planetarium.GetUniversalTime(), oldOrbit.referenceBody);
                FlightGlobals.fetch.SetShipOrbit(newOrbit.referenceBody.flightGlobalsIndex, 1.0, 3468.75, 0.0, newOrbit.LAN, 1.0, 1.0, Planetarium.GetUniversalTime());
                //FlightGlobals.fetch.SetShipOrbit(Planetarium.fetch.CurrentMainBody.flightGlobalsIndex, 1.0d, 100d, 0d, 1d, 1d, 1d, 1d);
            }

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(300f));

            AGOSInputLockManager.DEBUGdrawLockButtons();
            
            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUI.DragWindow();


        }

        public static bool isDebugBuild()
        {
#if DEBUG
            return true;
#else
            return false;
#endif

        }

    }
}
