using AGroupOnStage.ActionGroups;
using AGroupOnStage.Logging;
using AGroupOnStage.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.IO;

namespace AGroupOnStage.Main
{
/*#if DEBUG
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
#else*/
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
//#endif
    public class AGOSDebug : MonoBehaviour
    {
        private static bool guiVisible = false;
        private static Rect _winPos = new Rect();
        private static Vector2 scrollPos = Vector2.zero;
        private static Vector2 logScrollPos = Vector2.zero;
        private static System.Random _random = new System.Random();
        private static List<String> logStrings = new List<String>();
        /*private int hello = 0;*/

        public void Start()
        {
            if (isDebugBuild() && HighLogic.LoadedScene == GameScenes.LOADING)
                HighLogic.fetch.showConsole = true;
        }

        public static void printAllActionGroups()
        {

            Logger.Log("Dumping action group info");
            foreach (AGOSActionGroup a in AGOSMain.Instance.actionGroups)
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


            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MinWidth(300f), GUILayout.MinHeight(400f), GUILayout.ExpandHeight(true));


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

            if (GUILayout.Button("Add test action groups"))
            {

                List<AGOSActionGroup> groups = new List<AGOSActionGroup>();

                AGOSActionGroup ag = new BasicActionGroup();
                ag.Stages = new int[] { 0, 1 };
                ag.Group = 8; // Custom01
                ag.IsTester = true;

                groups.Add(ag);

                AGOSActionGroup ag2 = new BasicActionGroup();
                ag2.isPartLocked = true;
                ag2.partRef = (HighLogic.LoadedSceneIsFlight ? FlightGlobals.fetch.activeVessel.rootPart : EditorLogic.fetch.ship.parts.LastButOne()).savedPartName();
                ag2.Group = getRandomGroupID();
                ag2.IsTester = true;

                groups.Add(ag2);

                AGOSActionGroup ag3 = new CameraControlActionGroup();
                ag3.Stages = new int[] { 1, 4 };
                ag3.cameraMode = FlightCamera.Modes.FREE;
                ag3.Group = -6;
                ag3.IsTester = true;

                groups.Add(ag3);

                AGOSActionGroup ag4 = new TimeDelayedActionGroup();
                ag4.timerDelay = (new System.Random()).LogicalNext(9) + 1;
                ag4.fireGroupID = getRandomGroupID();
                ag4.Stages = new int[] { 1 };
                ag4.IsTester = true;
                ag4.Group = -9;
                groups.Add(ag4);

                AGOSActionGroup ag5 = new BasicActionGroup();
                ag5.onDock = true; // Not used
                ag5.FireType = AGOSActionGroup.FireTypes.DOCK;
                ag5.Group = /*getRandomGroupID()*/8;
                ag5.IsTester = true;

                groups.Add(ag5);

                AGOSActionGroup ag6 = new BasicActionGroup();
                ag6.onDock = true; // Not used
                ag6.FireType = AGOSActionGroup.FireTypes.UNDOCK;
                ag6.Group = /*getRandomGroupID()*/8;
                ag6.IsTester = true;

                groups.Add(ag6);

                AGOSMain.Instance.actionGroups.AddRange(groups);
                if (HighLogic.LoadedSceneIsFlight)
                {
                    Vessel v = FlightGlobals.fetch.activeVessel;
                    AGOSMain.Instance.findHomesForPartLockedGroups(v);
                    AGOSMain.Instance.getMasterAGOSModule(v).setFlightID(v.rootPart.flightID);
                }
                AGOSMain.Instance.removeDuplicateActionGroups();

            }

            if (GUILayout.Button("Dump loaded vessel list (spammy)"))
            {
                Logger.Log("Current list of loaded vessels:");
                foreach (Vessel v in new List<Vessel>(FlightGlobals.fetch.vessels))
                    Logger.Log("\t{0} {1}", v.vesselName, (v.rootPart != null ? "(" + v.rootPart.flightID + ")" : ""));
            }

            if (GUILayout.Button("Print current stageLock state"))
            {
                Logger.Log("FlightInputHandler.fetch.stageLock = {0}", FlightInputHandler.fetch.stageLock);
            }

            if (GUILayout.Button("Dump orbital info (spammy)"))
            {
                Logger.Log("Dumping orbital data:");
                Orbit orbit = FlightGlobals.fetch.activeVessel.orbit;
                FieldInfo[] fields = orbit.GetType().GetFields(); // We only need public ones for this

                foreach (FieldInfo f in fields)
                    Logger.Log("\t{0} = {1}", f.Name, f.GetValue(orbit));

            }
            if (AGOSPartSelectionHandler.Instance != null)
            {
                if (GUILayout.Button("Force-enable Part Selection Mode"))
                    AGOSPartSelectionHandler.Instance.enterPartSelectionMode();
            }

            if (GUILayout.Button("Globally activate group Custom01"))
            {
                FlightGlobals.fetch.vessels.FindAll(v => !v.HoldPhysics).ForEach(_v => { Logger.Log("Firing Custom01 on " + _v.vesselName); _v.ActionGroups.ToggleGroup(KSPActionGroup.Custom01); });
            }

            if (GUILayout.Button("Print all (loaded) AGOS configs (spammy)"))
            {
                if (AGOSMain.Instance.actionGroups.Count == 0) { Logger.Log("No configs to output"); return; }
                foreach (AGOSActionGroup ag in AGOSMain.Instance.actionGroups.ToList()) // Already a list, but this should give us a safe one to iterate over
                    Logger.Log(ag.ToString());
            }

            GUILayout.EndVertical();

            // Debug log (AGOS only)
            logScrollPos = GUILayout.BeginScrollView(logScrollPos, GUILayout.MinWidth(300f), GUILayout.MinHeight(400f));
            foreach (string s in logStrings)
                GUILayout.Label(s);
            GUILayout.EndScrollView();


            GUILayout.EndHorizontal();

            GUI.DragWindow();


        }

        private static int getRandomGroupID()
        {
            return AGOSMain.Instance.actionGroupList.Keys.ElementAt(_random.Next(AGOSMain.Instance.actionGroupList.Count));
        }

        public static bool isDebugBuild()
        {
#if DEBUG
            return true;
#else
            return false;
#endif

        }

        public static void addLogString(String str)
        {
            while (logStrings.Count > 150)
                logStrings.RemoveAt(0);
            logStrings.Add(str);
            if (guiVisible)
                logScrollPos.y = Mathf.Infinity;
        }

        public void Update()
        {
            /*hello++;
            if (hello == Int32.MaxValue)
                hello = 0;
            if (hello % 60 == 0)
                Logger.Log("Debug Update");*/
            if (isDebugBuild() || AGOSMain.Settings.get<bool>("DebugMenuShortcut"))
            {
                /*if (Input.anyKey)
                    Logger.Log("A key is down! Key down! I repeat, KEY. DOWN!");*/
                if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.A))
                {
                    toggleGUI();
                }
            }
        }

    }
}
