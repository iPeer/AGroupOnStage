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
        public static List<IActionGroup> backupActionGroups = new List<IActionGroup>();
        public Dictionary<int, string> actionGroupList = new Dictionary<int, string>();
        public Dictionary<int, bool> actionGroupSettings = new Dictionary<int, bool>();
        public Dictionary<int, KSPActionGroup> stockAGMap;
        public static AGOSSettings Settings { get; protected set; }
        public bool FlightEventsRegistered { get; set; }
        public bool EditorEventsRegistered { get; set; }

        #endregion

        #region GUI control vars

        private float throttleLevel = 0f;
        private string stageList = "";

        #endregion

        #region private vars

        private string[] stockAGNames;
        private KSPActionGroup[] stockAGList;
        private static readonly int AGOS_GUI_WINDOW_ID = 03022007;
        //                                               ^ pointless 0 is pointless
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
            LOCK_STAGING = -7,
            CAMERA_LOCKED = -8
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
            {"LOCK_STAGING", "Lock staging"},
            {"CAMERA_LOCKED", "Set camera: LOCKED"}

        };

        #endregion

        #region initialization

        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSMain.Start()");
            Settings = new AGroupOnStage.Main.AGOSSettings(IOUtils.GetFilePathFor(this.GetType(), "settings.cfg"));
            // Create the pluginData folder for AGOS, if it doesn't exist
            System.IO.Directory.CreateDirectory(Settings.configPath.Replace("settings.cfg", ""));
            Instance = this;
            useAGXConfig = AGXInterface.isAGXInstalled();
            Logger.Log("This install is " + (useAGXConfig ? "" : "not ") + "running AGX.");
            //if (useAGXConfig) { /* DO NAAHTHING! */ } // AGX is installed - use its controller, not stock's
            //else // Not installed - fall back to stock controller.
            loadActionGroups();
            Logger.Log("Loading AGOS' settings");
            Settings.load();
            Logger.Log("AGOS' Settings loaded");
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
            GameEvents.onVesselChange.Add(onVesselLoaded);
            GameEvents.onGameSceneLoadRequested.Add(onSceneLoadRequested);
            GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
            GameEvents.onEditorUndo.Add(OnEditorUndo);
            GameEvents.onEditorRedo.Add(OnEditorUndo);
#if DEBUG
            if (Settings.SHOW_DRAGONS_DIALOG)
                RenderingManager.AddToPostDrawQueue(AGOS_GUI_WINDOW_ID + 1, OnDraw_Dragons);
#endif
        }

        private void OnGUIApplicationLauncherReady()
        {
            setupToolbarButton();
        }

        public void backupActionGroupList()
        {
            foreach (IActionGroup a in this.actionGroups)
                backupActionGroups.Add(a);
            Logger.Log("Backed up {0} group(s)", backupActionGroups.Count);
        }

        public void restoreBackedUpActionGroups()
        {
            restoreBackedUpActionGroups(false);
        }

        public void restoreBackedUpActionGroups(bool clear)
        {
            if (backupActionGroups != null && backupActionGroups.Count > 0)
            {
                Logger.Log("B:{0} / L:{1}", backupActionGroups.Count, this.actionGroups.Count);
                this.actionGroups.Clear();
                foreach (IActionGroup a in backupActionGroups)
                    this.actionGroups.Add(a);
                Logger.Log("Restored {0} group(s)", backupActionGroups.Count);
                if (clear)
                    backupActionGroups.Clear();
            }
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
                for (int x = 8; x <= 257; x++)
                {
                    actionGroupList.Add(x, x.ToString());
                    actionGroupSettings.Add(x, false);
                }
                Logger.Log("\tDone!");
            }
            Logger.Log("\tStock...");
            if (useAGXConfig)
                Logger.Log("\tAGX is installed, limiting stock AG loading to non Customs");
            for (int x = 0; x < (useAGXConfig ? 8 : stockAGNames.Length); x++)
            {
                stockAGMap.Add(x, stockAGList[x]);
                actionGroupList.Add(x, stockAGNames[x]);
                actionGroupSettings.Add(x, false);
            }
            Logger.Log("\tDone!");

            Logger.Log("Finished loading action groups");
            Logger.Log("Loaded {0} action group(s)", actionGroupList.Count);
        }

        #endregion

        #region GUI

        private void setupToolbarButton()
        {
            if (!launcherButtonAdded)
            {
                string _texture = "iPeer/AGroupOnStage/Textures/Toolbar";
                System.Random r = new System.Random();
                if (r.Next(5) == 5) // 10
                {
                    Logger.Log("Are you hungry?");
                    _texture = "iPeer/AGroupOnStage/Textures/Toolbar_alt";
                }
                Logger.Log("Adding ApplicationLauncher button");
                agosButton = ApplicationLauncher.Instance.AddModApplication(
                    toggleGUI,
                    toggleGUI,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.VAB,
                    (Texture)GameDatabase.Instance.GetTexture(_texture, false)
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
            toggleGUI(false);
        }

        public void toggleGUI(bool fromPart)
        {
            if (guiVisible && !fromPart)
            {
                EditorLogic.fetch.Unlock("AGOS_INPUT_LOCK");
                guiVisible = false;
                agosButton.SetFalse(false);
                RenderingManager.RemoveFromPostDrawQueue(AGOS_GUI_WINDOW_ID, OnDraw);
                Settings.WIN_POS_X = _windowPos.x;
                Settings.WIN_POS_Y = _windowPos.y;
                Settings.save();
                if (HighLogic.LoadedSceneIsEditor)
                {
                    if (linkPart != null)
                        linkPart = null;
                    AGOSUtils.resetActionGroupConfig(false);
                }

            }
            else
            {
                if (fromPart && guiVisible) { return; }
                EditorTooltip.Instance.HideToolTip();
                EditorLogic.fetch.Lock(true, true, true, "AGOS_INPUT_LOCK");
                guiVisible = true;
                agosButton.SetTrue(false);
                _windowPos.x = Settings.WIN_POS_X;
                _windowPos.y = Settings.WIN_POS_Y;
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
            _scrollPosGroups = GUILayout.BeginScrollView(_scrollPosGroups, _scrollStyle, GUILayout.Width(230f), GUILayout.Height(270f));
            /*if (useAGXConfig)
            {
                throw new NotImplementedException();
            }
            else
            {*/
            //_scrollPosGroups = GUILayout.BeginScrollView(_scrollPosGroups, false, false, _scrollStyle, _scrollStyle, _scrollStyle, GUILayout.Width(250f), GUILayout.Height(450f));

            int[] AG_MIN_MAX = getMinMaxGroupIds();
            int AG_MIN = AG_MIN_MAX[0];
            int AG_MAX = AG_MIN_MAX[1];

            //Logger.LogDebug("MAX: {0}, MIN: {1}", AG_MAX, AG_MIN);    

            for (int x = AG_MIN; x < AG_MAX; x++)
            {
                //Logger.Log("AG {0}: {1}", x, actionGroupList[x]);
                if (x == 0 || x == 1 || x == -7) { continue; } // "None", "Stage" and "Lock Staging" action groups
                if (useAGXConfig && x >= 8)
                {
                    string groupName = (x - 7 < 0 ? actionGroupList[x] : x - 7 + (AGX.AGXInterface.getAGXGroupDesc(x - 7) != null ? ": " + AGX.AGXInterface.getAGXGroupDesc(x - 7) : ""));
                    actionGroupSettings[x] = GUILayout.Toggle(actionGroupSettings.ContainsKey(x) ? actionGroupSettings[x] : false, x+"/"+(x-7)+" "+groupName, _buttonStyle);
                }
                else
                    actionGroupSettings[x] = GUILayout.Toggle(actionGroupSettings.ContainsKey(x) ? actionGroupSettings[x] : false, x+" "+actionGroupList[x], _buttonStyle);
            }
            /*}*/
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(240f));
            if (linkPart != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Part: " + linkPart.name + "_" + linkPart.craftID, _labelStyle);
                if (GUILayout.Button("X", _buttonStyle, GUILayout.MaxWidth(30f)))
                    linkPart = null;
                GUILayout.EndHorizontal();
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
                //if (!HighLogic.LoadedSceneIsEditor && ag.Vessel != FlightGlobals.fetch.activeVessel) { continue; }
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
                string groupName;
                if (useAGXConfig && ag.Group >= 8)
                    groupName = ag.Group - 7 + (AGX.AGXInterface.getAGXGroupDesc(ag.Group - 7) != null ? ": " + AGX.AGXInterface.getAGXGroupDesc(ag.Group - 7) : "");
                else
                    groupName = actionGroupList[ag.Group];
                GUILayout.Label(groupName + (ag.GetType() == typeof(ThrottleControlActionGroup) ? String.Format(" ({0:P0})", ag.ThrottleLevel) : ""), _labelStyle, GUILayout.MinWidth(150f));

                GUIStyle __labelStyle = _labelStyle;
                // TODO: Fix the nullref that this causes.
                /*if ((ag.linkedPart != null || !AGOSUtils.getVesselPartsList().Contains(ag.linkedPart)) || ag.Stages.Count(a => a > Staging.StageCount) > 0)
                    __labelStyle.normal.textColor = XKCDColors.Red;
                else
                    __labelStyle.normal.textColor = _labelStyle.normal.textColor;*/
                GUILayout.Label(stagesString, __labelStyle);
                if (GUILayout.Button("Edit", _buttonStyle, GUILayout.MaxWidth(40f)))
                {
                    if (linkPart != null)
                        linkPart = null;
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
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close", _buttonStyle))
                toggleGUI();
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
                if (x == 0 && useAGXConfig) { continue; } // No group '0' when using AGX
                if (actionGroupSettings[x])
                {

                    /*
                    THROTTLE = -1,
                    FINE_CONTROLS = -2,
                    CAMERA_AUTO = -3,
                    CAMERA_ORBITAL = -4,
                    CAMERA_CHASE = -5,
                    CAMERA_FREE = -6,
                    LOCK_STAGING = -7,
                    CAMERA_LOCKED = -8
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
                    else if (x < -2 && x > -7 || x == -8) // Camera mode
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
            //linkPart = null; // 2.0-dev5/2.0-rel: No longer clear part link when commiting groups
        }

        public int[] getMinMaxGroupIds()
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

        [Obsolete("Use findHomesForPartLockedGroups() instead", true)]
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
            //Logger.Log("Finding homes for part locked action group configurations");
            List<IActionGroup> partLinkedGroups = actionGroups.FindAll(a => a.isPartLocked && a.linkedPart == null);
            Logger.Log("{0} homeless group(s)", partLinkedGroups.Count());
            foreach (IActionGroup g in partLinkedGroups)
            {
                Part part = AGOSUtils.findPartByReference(g.partRef, vessel);
                if (part == null)
                {
                    Logger.LogWarning("Action group '{1}' supplied invalid part reference '{0}', skipping.", g.partRef, g.Group);
                    continue;
                }
                g.linkedPart = part;
                Logger.Log("Action group '{2}' and part '{0}' ({1}) have been paired", part.partInfo.title, String.Format("{0}_{1}", part.name, part.craftID), g.Group);
            }
            //Logger.Log("Finished finding homes for all part locked action group configurations");
        }

        #endregion

        public void handleLevelLoaded(GameScenes scene)
        {
            if (AGOSUtils.isLoadedSceneOneOf(GameScenes.EDITOR, GameScenes.FLIGHT))
            {
                //Logger.Log("Revert");
                if (AGOSUtils.getVesselPartsList().Count > 0)
                {
                    AGOSMain.Instance.restoreBackedUpActionGroups(false); // 2.0.6-dev1: Changed to false to prevent duping if player reverts multiple times (-> launch [-> launch [-> ...]] -> editor)
                    AGOSMain.Instance.findHomesForPartLockedGroups(AGOSUtils.getVesselPartsList());
                }
            }
        }

        internal void onVesselLoaded(Vessel data)
        {
            Logger.Log("Vessel was loaded.");
            if (AGOSUtils.isLoadedSceneOneOf(GameScenes.EDITOR, GameScenes.FLIGHT))
                findHomesForPartLockedGroups(data);
        }

        private void onSceneLoadRequested(GameScenes scene)
        {
            Logger.Log("Scene change to '{0}' from '{1}' requested", scene.ToString(), HighLogic.LoadedScene.ToString());
            /*if (!FlightDriver.CanRevert)
                Logger.Log("Player cannot revert, no group backup will be taken.");*/
            if (HighLogic.LoadedSceneIsEditor/* && FlightDriver.CanRevert*/) // The crap I have to do to get reverting working...
            {
                backupActionGroups.Clear(); // 2.0.6-dev1 fix for yet another dupe (I hope)
                backupActionGroupList();
            }
            AGOSUtils.resetActionGroupConfig();
        }


        private void OnEditorUndo(ShipConstruct data)
        {
            Logger.Log("Undo/Redo");
            //AGOSUtils.resetActionGroupConfig();
            findHomesForPartLockedGroups(data.parts);
        }

        private void onLevelWasLoaded(GameScenes level)
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && backupActionGroups.Count > 0)
            {
                Logger.Log("Player has left to the Space Centre, clearing AG config backups.");
                backupActionGroups.Clear();
            }

        }

        #region herebedragons
        // Here be dragons GUI on startup

        private void OnDraw_Dragons()
        {

            if (!hasSetupStyles)
                setUpStyles();
            _windowPos = GUILayout.Window(AGOS_GUI_WINDOW_ID + 1, _windowPos, OnWindow_Dragons, "Roar!", HighLogic.Skin.window);

            _windowPos.x = Screen.width / 2 - _windowPos.width / 2;
            _windowPos.y = Screen.height / 2 - _windowPos.height / 2;

        }

        public void OnWindow_Dragons(int wID)
        {
            GUIStyle label = HighLogic.Skin.label;
            label.stretchWidth = true;

            GUILayout.BeginHorizontal(GUILayout.Width(250f));

            GUILayout.Label("HERE BE DRAGONS!\nThis is a *very* early experimental release of the new AGOS. Things are going to be broken.\n\nIf you find a bug, which is really quite likely, please report it on AGOS' GitHub issues page.\n\nYou can get to this page by clicking the \"Issues Page\" button below.\n\nWhen reporting a bug, please include your output_log file and a craft file  and/or persistent file (stock only, please!) if you feel it will help with the report.\n\nPlease check back at the releases page regularly to see if there's a new release!\n\nThis message will only display once.", label, GUILayout.Width(245f));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUILayout.Width(250f));
            if (GUILayout.Button("Issues Page"))
                Application.OpenURL("https://github.com/iPeer/AGroupOnStage/issues");
            if (GUILayout.Button("Okay, okay, I get it!"))
            {
                Settings.SHOW_DRAGONS_DIALOG = false;
                Settings.save();
                RenderingManager.RemoveFromPostDrawQueue(AGOS_GUI_WINDOW_ID + 1, OnDraw_Dragons);
            }

            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        #endregion
    }
}
