using AGroupOnStage.ActionGroups;
using AGroupOnStage.AGX;
using AGroupOnStage.Logging;
using KSP.IO;
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
        public Part linkPart { get; set; }
        public bool guiVisible = false;
        public bool useAGXConfig = false;
        public bool launcherButtonAdded = false;
        public List<IActionGroup> actionGroups = new List<IActionGroup>();
        public Dictionary<int, string> actionGroupList = new Dictionary<int, string>();
        public Dictionary<int, bool> actionGroupSettings = new Dictionary<int, bool>();
        public Dictionary<int, KSPActionGroup> stockAGMap;
        public static AGOSSettings Settings { get; protected set; }

        #endregion

        #region GUI control vars

        private float throttleLevel = 0f;
        private string stageList = "";

        #endregion

        #region private vars

        private string[] stockAGNames;
        private KSPActionGroup[] stockAGList;
        private static readonly int AGOS_GUI_WINDOW_ID = 03022007;
        private ApplicationLauncherButton agosButton = null;
        private bool hasSetupStyles = false;
        private enum AGOSActionGroups
        {
            THROTTLE = -1,
            FINE_CONTROLS = -2,
            CAMERA_AUTO = -3,
            CAMERA_ORBITAL = -4,
            CAMERA_CHASE = -5,
            CAMERA_FREE = -6,
            LOCK_STAGING = -7
        }

        #endregion

        #region GUI vars

        private Rect _windowPos = new Rect();
        private Vector2 _scrollPosGroups = Vector2.zero, _scrollPosConfig = Vector2.zero;
        private GUIStyle _buttonStyle,
            _scrollStyle,
            _windowStyle,
            _labelStyle,
            _labelStyleRed,
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
            {"CAMERA_FREE", "Set camera: FREE"},
            {"LOCK_STAGING", "Lock staging"}

        };

        #endregion

        #region initialization

        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSMain.Start()");
            Settings = new AGroupOnStage.Main.AGOSSettings(IOUtils.GetFilePathFor(this.GetType(), "settings.cfg"));
            Instance = this;
            useAGXConfig = AGXInterface.isAGXInstalled();
            Logger.Log("This install is " + (useAGXConfig ? "" : "not ") + "running AGX.");
            //if (useAGXConfig) { /* DO NAAHTHING! */ } // AGX is installed - use its controller, not stock's
            //else // Not installed - fall back to stock controller.
            loadActionGroups();
            Logger.Log("Loading AGOS' settings");
            Settings.load();
            _windowPos.x = Settings.WIN_POS_X;
            _windowPos.y = Settings.WIN_POS_Y;
            Logger.Log("AGOS' Settings loaded");
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
            GameEvents.onVesselChange.Add(onVesselLoaded);
            GameEvents.onGameSceneLoadRequested.Add(onSceneLoadRequested);
            //GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
            GameEvents.onEditorUndo.Add(OnEditorUndo);
            GameEvents.onEditorRedo.Add(OnEditorUndo);
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

        public void toggleGUI()
        {
            if (guiVisible)
            {
                guiVisible = false;
                RenderingManager.RemoveFromPostDrawQueue(AGOS_GUI_WINDOW_ID, OnDraw);
                Settings.WIN_POS_X = _windowPos.x;
                Settings.WIN_POS_Y = _windowPos.y;
                Settings.save();
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

                //Logger.LogDebug("MAX: {0}, MIN: {1}", AG_MAX, AG_MIN);    

                for (int x = AG_MIN; x < AG_MAX; x++)
                {
                    //Logger.Log("AG {0}: {1}", x, actionGroupList[x]);
                    if (x == 0 || x == 1) { continue; } // "None" and "Stage" action groups
                    actionGroupSettings[x] = GUILayout.Toggle(actionGroupSettings.ContainsKey(x) ? actionGroupSettings[x] : false, actionGroupList[x], _buttonStyle);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(240f));
            if (linkPart != null)
            {
                GUILayout.Label("Part: " + linkPart.name);
            }
            else
            {
                GUILayout.Label("Stages: ", _labelStyle);
                stageList = GUILayout.TextField(stageList, _textFieldStyle);
                GUILayout.Label("Separate multiple by a comma (,)", _labelStyle);
            }
            GUILayout.Space(4);
            if (actionGroupSettings[actionGroupList.First(a => a.Value.Contains("Throttle")).Key])
            {
                GUILayout.Label("Throttle control:", _labelStyle);
                GUILayout.BeginHorizontal(GUILayout.Width(240f));
                throttleLevel = GUILayout.HorizontalSlider(throttleLevel, 0f, 1f, _sliderSliderStyle, _sliderThumbStyle);
                GUILayout.Label(String.Format("{0:P0}", throttleLevel), _labelStyle);
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
                //AGOSUtils.printActionGroupInfo(ag);
                string stagesString;
                if (ag.isPartLocked)
                {
                    if (ag.linkedPart == null)
                    {
                        Logger.LogWarning("Action group '{0}' is invalid (part reference is null), removing it from the list", ag.Group);
                        actionGroups.Remove(ag);
                        continue;
                    }
                    stagesString = String.Format("(PART) {1}", ag.partRef, ag.linkedPart.inverseStage);
                }
                else
                {
                    int[] stages = ag.Stages;
                    stagesString = AGOSUtils.intArrayToString(stages, ", ");
                }

                GUILayout.BeginHorizontal();

                GUILayout.Label(actionGroupList[ag.Group], _labelStyle, GUILayout.MinWidth(150f));

                GUIStyle __labelStyle = _labelStyle;
                // TODO: Fix the nullref that this causes.
                /*if ((ag.linkedPart != null || !AGOSUtils.getVesselPartsList().Contains(ag.linkedPart)) || ag.Stages.Count(a => a > Staging.StageCount) > 0)
                    __labelStyle.normal.textColor = XKCDColors.Red;
                else
                    __labelStyle.normal.textColor = _labelStyle.normal.textColor;*/
                GUILayout.Label(stagesString, __labelStyle);
                if (GUILayout.Button("Edit", _buttonStyle, GUILayout.MaxWidth(40f)))
                {
                    actionGroupSettings[ag.Group] = true;
                    throttleLevel = ag.ThrottleLevel;
                    if (ag.linkedPart != null)
                    {
                        linkPart = ag.linkedPart;
                        stageList = "";
                    }
                    else
                        stageList = AGOSUtils.intArrayToString(ag.Stages, ",");
                    actionGroups.Remove(ag);
                }
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
            int AG_MIN = AG_MIN_MAX[0];
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
                    CAMERA_FREE = -6,
                    LOCK_STAGING = -7
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
                    else if (x == -7)
                    {
                        ag = new StageLockActionGroup();
                    }
                    else
                    {
                        ag = new BasicActionGroup();
                    }
                    if (linkPart != null)
                    {
                        ag.linkedPart = linkPart;
                        ag.isPartLocked = true;
                        ag.partRef = String.Format("{0}_{1}", linkPart.name, linkPart.craftID);
                    }
                    else
                    {
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
                    }

                    ag.Group = x;

                    Logger.Log("\t{0}", ag.ToString());
                    actionGroups.Add(ag);
                    actionGroupSettings[x] = false;

                }
            }
            throttleLevel = 0f;
            stageList = "";
            linkPart = null;
        }

        private int[] getMinMaxGroupIds()
        {
            int[] ret = new int[2];
            int min = Enum.GetNames(typeof(AGOSActionGroups)).Length;
            ret[0] = -min;
            ret[1] = actionGroupSettings.Count - min;
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

        public void updatePartLockedStages(bool suppressLog = false)
        {
            List<IActionGroup> toUpdate = actionGroups.FindAll(a => a.linkedPart != null);
            if (toUpdate.Count == 0)
            {
                if (!suppressLog)
                    Logger.Log("No part locked groups to update");
                return;
            }
            foreach (IActionGroup ag in toUpdate)
                if (ag.Stages[0] != ag.linkedPart.inverseStage)
                    ag.Stages[0] = ag.linkedPart.inverseStage;
            if (!suppressLog)
                Logger.Log("{0} part linked action group(s) checked and updated where neccessary", toUpdate.Count);
        }
        public void findHomesForPartLockedGroups(Vessel vessel)
        {
            findHomesForPartLockedGroups(vessel.parts);
        }
        public void findHomesForPartLockedGroups(List<Part> vessel)
        {
            if (vessel.Count() == 0) // Empty parts list
                return;
            Logger.Log("Finding homes for part locked action group configurations");
            List<IActionGroup> partLinkedGroups = actionGroups.FindAll(a => a.isPartLocked && a.linkedPart == null);
            Logger.Log("{0} homeless part(s)", partLinkedGroups.Count());
            foreach (IActionGroup g in partLinkedGroups)
            {
                Part part = AGOSUtils.findPartByReference(g.partRef, vessel);
                if (part == null)
                {
                    Logger.LogWarning("Action group supplied invalid part reference '{0}', skipping.", g.partRef);
                    continue;
                }
                g.linkedPart = part;
                Logger.Log("Action group '{2}' and part '{0}' ({1}) have been paired", part.partInfo.title, String.Format("{0}_{1}", part.name, part.craftID), g.Group);
            }
            Logger.Log("Finished finding homes for all part locked action group configurations");
        }

        #endregion

        internal void onVesselLoaded(Vessel data)
        {
            findHomesForPartLockedGroups(data);
        }

        private void onSceneLoadRequested(GameScenes scene)
        {
            AGOSUtils.resetActionGroupConfig();
        }


        private void OnEditorUndo(ShipConstruct data)
        {
            //AGOSUtils.resetActionGroupConfig();
            findHomesForPartLockedGroups(data.parts);
        }

        private void onLevelWasLoaded(GameScenes level)
        {
            if (AGOSUtils.isLoadedSceneOneOf(GameScenes.EDITOR, GameScenes.FLIGHT))
                findHomesForPartLockedGroups(AGOSUtils.getVesselPartsList());
        }
    }
}
