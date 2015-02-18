﻿using AGroupOnStage.ActionGroups;
using AGroupOnStage.AGX;
using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class AGOSMain : MonoBehaviour
    {

        #region public vars

        public static AGOSMain Instance { get; protected set; } // Protected set in case we need/want to extend this class in the future

        #endregion

        #region GUI control vars

        private float throttleLevel = 0f;
        private string stageList = "";

        #endregion

        #region private vars

        private static int AGOS_GUI_WINDOW_ID = 03022007;
        private bool launcherButtonAdded = false;
        private bool guiVisible = false;
        private bool useAGXConfig = false;
        private List<IActionGroup> actionGroups = new List<IActionGroup>();
        private Dictionary<int, string> actionGroupList = new Dictionary<int, string>();
        private Dictionary<int, bool> actionGroupSettings = new Dictionary<int, bool>();
        private string[] stockAGNames;
        private KSPActionGroup[] stockAGList;
        private Dictionary<int, KSPActionGroup> stockAGMap;
        private ApplicationLauncherButton agosButton = null;
        private bool hasSetupStyles = false;
        private enum AGOSActionGroups
        {
            THROTTLE = -1,
            FINE_CONTROLS = -2,
            CAMERA_AUTO = -3,
            CAMERA_ORBITAL = -4,
            CAMERA_CHASE = -5,
            CAMERA_FREE = -6
        }

        #endregion

        #region GUI vars

        private Rect _windowPos = new Rect();
        private Vector2 _scrollPosGroups = Vector2.zero, _scrollPosConfig = Vector2.zero;
        private GUIStyle _buttonStyle,
            _scrollStyle,
            _windowStyle,
            _labelStyle,
            _toggleStyle,
            _sliderStyle,
            _sliderSliderStyle,
            _sliderThumbStyle,
            _textFieldStyle;

        private Dictionary<string, string> agosGroupPrettyNames = new Dictionary<string, string>() {

            {"FINE_CONTROLS", "Toggle fine controls"},
            {"THROTTLE", "Throttle control"},
            {"CAMERA_AUTO", "Set camera: AUTO"},
            {"CAMERA_ORBITAL", "Set camera: ORBITAL"},
            {"CAMERA_CHASE", "Set camera: CHASE"},
            {"CAMERA_FREE", "Set camera: FREE"}

        };

        #endregion

        #region initialization

        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSMain.Start()");
            Instance = this;
            useAGXConfig = AGXInterface.isAGXInstalled();
            Logger.Log("This install is " + (useAGXConfig ? "" : "not ") + "running AGX.");
            //if (useAGXConfig) { /* DO NAAHTHING! */ } // AGX is installed - use its controller, not stock's
            //else // Not installed - fall back to stock controller.
            loadActionGroups();
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
        }

        private void OnGUIApplicationLauncherReady()
        {
            setupToolbarButton();
        }

        private void loadActionGroups()
        {
            Logger.Log("Loading AGOS action group list");
            string[] agosGroupNames = Enum.GetNames(typeof(AGOSActionGroups));
            int[] agosGroupIDs = (int[])Enum.GetValues(typeof(AGOSActionGroups));
            stockAGMap = new Dictionary<int, KSPActionGroup>();
            Logger.Log("Done loading AGOS action group list");
            Logger.Log("Loading stock action group list");
            stockAGNames = Enum.GetNames(typeof(KSPActionGroup)); // get ag names
            stockAGList = (KSPActionGroup[])Enum.GetValues(typeof(KSPActionGroup));
            Logger.Log("Done loading stock action group list");
            Logger.Log("Building action group list");
            Logger.Log("\tAGOS...");
            for (int x = 0; x < agosGroupNames.Length; x++)
            {
                string groupName;
                try
                {
                    groupName = agosGroupPrettyNames[agosGroupNames[x]];
                }
                catch
                {
                    groupName = agosGroupNames[x];
                    Logger.LogWarning("No pretty name set for action group '{0}'", groupName);
                }
                actionGroupList.Add(agosGroupIDs[x], groupName);
                actionGroupSettings.Add(agosGroupIDs[x], false);
            }
            Logger.Log("\tDone!");
            if (useAGXConfig)
            {
                Logger.Log("\tAGX...");
                throw new NotImplementedException();
            }
            else
            {
                Logger.Log("\tStock...");
                for (int x = 0; x < stockAGNames.Length; x++)
                {
                    stockAGMap.Add(x, stockAGList[x]);
                    actionGroupList.Add(x, stockAGNames[x]);
                    actionGroupSettings.Add(x, false);
                }
                Logger.Log("\tDone!");
            }
            Logger.Log("Finished loading action groups");
            Logger.Log("Loaded {0} action group(s)", actionGroupList.Count);
        }

        #endregion

        #region GUI

        private void setupToolbarButton()
        {
            if (!launcherButtonAdded)
            {
                Logger.Log("Adding ApplicationLauncher button");
                agosButton = ApplicationLauncher.Instance.AddModApplication(
                    toggleGUI,
                    toggleGUI,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.VAB,
                    (Texture)GameDatabase.Instance.GetTexture("iPeer/AGroupOnStage/Textures/Toolbar", false)
                );
                launcherButtonAdded = true;
            }
            else
                Logger.LogWarning("ApplicationLauncher button is already present (harmless)");

        }

        private void removeToolbarButton()
        {
            Logger.Log("Removing ApplicationLauncher button");
            ApplicationLauncher.Instance.RemoveModApplication(agosButton);
            launcherButtonAdded = false;
        }

        private void toggleGUI()
        {
            if (guiVisible)
            {
                guiVisible = false;
                RenderingManager.RemoveFromPostDrawQueue(AGOS_GUI_WINDOW_ID, OnDraw);
            }
            else
            {
                guiVisible = true;
                RenderingManager.AddToPostDrawQueue(AGOS_GUI_WINDOW_ID, OnDraw);
            }
        }

        private void OnDraw()
        {

            if (!hasSetupStyles)
                setUpStyles();
            _windowPos = GUILayout.Window(AGOS_GUI_WINDOW_ID, _windowPos, OnWindow, "Action group control", _windowStyle);
            // TODO: GUI position sanity checks

        }


        private void OnWindow(int windowID)
        {

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            if (useAGXConfig)
            {
                throw new NotImplementedException();
            }
            else
            {
                _scrollPosGroups = GUILayout.BeginScrollView(_scrollPosGroups, _scrollStyle, GUILayout.Width(230f), GUILayout.Height(270f));
                //_scrollPosGroups = GUILayout.BeginScrollView(_scrollPosGroups, false, false, _scrollStyle, _scrollStyle, _scrollStyle, GUILayout.Width(250f), GUILayout.Height(450f));

                int[] AG_MIN_MAX = getMinMaxGroupIds();
                int AG_MIN = AG_MIN_MAX[0];
                int AG_MAX = AG_MIN_MAX[1];

                //Logger.LogDebug("MAX: {0}, MIN: {1}", AG_MAX, -AG_MIN);    

                for (int x = -AG_MIN; x < AG_MAX; x++)
                {
                    //Logger.Log("AG {0}: {1}", x, actionGroupList[x]);
                    if (x == 0) { continue; } // "None" action group
                    actionGroupSettings[x] = GUILayout.Toggle(actionGroupSettings.ContainsKey(x) ? actionGroupSettings[x] : false, actionGroupList[x], _buttonStyle);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(240f));
            GUILayout.Label("Stages: ", _labelStyle);
            stageList = GUILayout.TextField(stageList, _textFieldStyle);
            GUILayout.Label("Separate multiple by a comma (,)", _labelStyle);
            GUILayout.Space(4);
            if (actionGroupSettings[actionGroupList.First(a => a.Value.Contains("Throttle")).Key])
            {
                GUILayout.Label("Throttle control:", _labelStyle);
                GUILayout.BeginHorizontal(GUILayout.Width(240f));
                throttleLevel = GUILayout.HorizontalSlider(throttleLevel, 0f, 1f, _sliderSliderStyle, _sliderThumbStyle);
                GUILayout.Label(String.Format("{0:P2}", throttleLevel), _labelStyle);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }

            if (GUILayout.Button("Commit group(s)", _buttonStyle))
            {
                commitGroups();
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _scrollPosConfig = GUILayout.BeginScrollView(_scrollPosConfig, _scrollStyle);

            IActionGroup[] groups = actionGroups.ToArray();

            foreach (IActionGroup ag in groups)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(actionGroupList[ag.Group], _labelStyle);
                int[] stages = ag.Stages;
                string stagesString = "";
                foreach (int s in stages) {
                    stagesString = stagesString+(stagesString.Length > 0 ? ", " : "")+s;
                }
                GUILayout.Label(stagesString, _labelStyle);
                if (GUILayout.Button("Remove", _buttonStyle, GUILayout.MaxWidth(70f)))
                    actionGroups.Remove(ag);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            GUI.DragWindow(); // Make window dragable

        }

        private void commitGroups()
        {
            Logger.Log("Commiting current action group configuration...");
            int[] AG_MIN_MAX = getMinMaxGroupIds();
            int AG_MIN = -AG_MIN_MAX[0];
            int AG_MAX = AG_MIN_MAX[1];

            for (int x = AG_MIN; x < AG_MAX; x++)
            {
                if (actionGroupSettings[x])
                {

                    /*
                    THROTTLE = -1,
                    FINE_CONTROLS = -2,
                    CAMERA_AUTO = -3,
                    CAMERA_ORBITAL = -4,
                    CAMERA_CHASE = -5,
                    CAMERA_FREE = -6
                    */

                    IActionGroup ag;
                    if (x == -1) // Throttle
                    {
                        ag = new ThrottleControlActionGroup();
                        ag.ThrottleLevel = throttleLevel;
                    }
                    else if (x == -2) // Fine controls
                    {
                        ag = new FineControlActionGroup();
                    }
                    else if (x < -2 && x > -7) // Camera mode
                    {
                        ag = new CameraControlActionGroup();
                        ag.cameraMode = AGOSUtils.getCameraModeForGroupID(x);
                    }
                    else
                    {
                        ag = new BasicActionGroup();
                    }

                    ag.Group = x;
                    int[] stages;
                    string[] sList = stageList.Split(',');
                    stages = new int[sList.Length];
                    for (int i = 0; i < sList.Length; i++)
                        try
                        {
                            stages[i] = Convert.ToInt32(sList[i]);
                        }
                        catch
                        {
                            Logger.LogWarning("Couldn't parse stage number '{0}'. Skipping.", sList[i]);
                        }
                    ag.Stages = stages;

                    Logger.Log("\t{0}", ag.ToString());

                    actionGroups.Add(ag);
                    actionGroupSettings[x] = false;

                }
            }
            throttleLevel = 0f;
            stageList = "";
        }

        private int[] getMinMaxGroupIds()
        {
            int[] ret = new int[2];
            ret[0] = Enum.GetNames(typeof(AGOSActionGroups)).Length;
            ret[1] = actionGroupSettings.Count - ret[0];
            return ret;
        }

        private void setUpStyles()
        {
            Logger.Log("Setting up GUI styles");
            hasSetupStyles = true;
            GUISkin skin = AGOSUtils.getBestAvailableSkin()/*HighLogic.Skin*/;
            Logger.LogDebug("Skin name: {0}", skin.name);
            _windowStyle = new GUIStyle(skin.window);
            _windowStyle.fixedHeight = 500f;
            _windowStyle.fixedWidth = 500f;
            _windowStyle.stretchWidth = true;
            _buttonStyle = new GUIStyle(skin.button);
            _labelStyle = new GUIStyle(skin.label);
            _labelStyle.stretchWidth = true;
            _toggleStyle = new GUIStyle(skin.toggle);
            _sliderStyle = new GUIStyle(skin.horizontalSlider);
            _sliderSliderStyle = skin.horizontalSlider;
            _sliderThumbStyle = skin.horizontalSliderThumb;
            _sliderStyle.stretchWidth = true;
            _scrollStyle = new GUIStyle(skin.scrollView);
            _scrollStyle.stretchHeight = true;
            _textFieldStyle = new GUIStyle(skin.textField);
            _textFieldStyle.fixedWidth = 235f;
            Logger.Log("Done setting up GUI styles");
        }

        #endregion

        #region saving and loading

        // These aren't actually used.
        public void OnSave(ConfigNode node)
        {
            Logger.Log("AGOS.Main.AGOSMain.OnSave()");
            Logger.Log("Vessel name is '{0}'", (HighLogic.LoadedSceneIsEditor ? EditorLogic.fetch.shipNameField.Text : FlightGlobals.fetch.activeVessel.vesselName));
        }

        public void OnLoad(ConfigNode node)
        {
            Logger.Log("AGOS.Main.AGOSMain.OnLoad()");
            Logger.Log("Vessel name is '{0}'", (HighLogic.LoadedSceneIsEditor ? EditorLogic.fetch.shipNameField.Text : FlightGlobals.fetch.activeVessel.vesselName));
        }

        #endregion

        #region misc. methods

        public AGOSModule getMasterAGOSModule(Vessel vessel)
        {
            List<Part> partList;
            if (HighLogic.LoadedSceneIsEditor)
                partList = EditorLogic.fetch.ship.parts;
            else if (vessel == null)
                partList = new List<Part>();
            else
                partList = vessel.Parts;

            return (from p in partList from m in p.Modules.OfType<AGOSModule>() select m).First();

        }

        #endregion

    }
}
