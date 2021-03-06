﻿using AGroupOnStage._000Toolbar;
using AGroupOnStage.ActionGroups;
using AGroupOnStage.AGX;
using AGroupOnStage.Logging;
using AGroupOnStage.Extensions;
using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using AGroupOnStage.ActionGroups.Timers;
using UnityEngine;

namespace AGroupOnStage.Main
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class AGOSMain : MonoBehaviour
    {

        #region public vars

        public static AGOSMain Instance { get; protected set; } // Protected set in case we need/want to extend this class in the future
        public static readonly Dictionary<string, string> specialOccasionDates = new Dictionary<string, string>() 
        {

            {"66", "Today is iPeer's birthday!"},
            {"244", "Today is Roxy's birthday!"},
            {"89", "Today is AGroupOnStage's birthday!"},
            {"2412", "Santa Claus is coming to town!"},
            {"2512", "Merry Christmas!"},
            {"11", "Happy New Year!"}


        };
        public bool Is64bit { get { return IntPtr.Size == 8; } }
        public bool SpecialOccasion
        {
            get
            {
                DateTime today = DateTime.Now;
                string dayMonth = String.Format("{0}{1}", today.Day, today.Month);
                //Logger.Log(dayMonth);
                return specialOccasionDates.ContainsKey(dayMonth) || (AGOSDebug.isDebugBuild() && Settings.get<bool>("DEBUGForceSpecialOccasion"));
            }
        }
        public Part linkPart { get; set; }
        public bool isPartTriggered = false;
        public bool guiVisible = false;
        public bool settingsGUIVisible = false;
        public bool useAGXConfig = false;
        public List<AGOSActionGroup> actionGroups = new List<AGOSActionGroup>();
        public Dictionary<int, string> actionGroupList = new Dictionary<int, string>();
        public Dictionary<int, bool> actionGroupSettings = new Dictionary<int, bool>();
        public Dictionary<int, KSPActionGroup> stockAGMap;
        public static AGOSSettings Settings { get; protected set; }
        //public static AGOSGroupManager GroupManager { get; protected set; }
        public bool FlightEventsRegistered { get; set; }
        public bool EditorEventsRegistered { get; set; }
        public static readonly int AGOS_GUI_WINDOW_ID = 03022007;
        //                                              ^ pointless 0 is pointless
        public static readonly int AGOS_DRAGONS_GUI_WINDOW_ID = 13022007;
        public static readonly int AGOS_SETTINGS_GUI_WINDOW_ID = 23022007;
        public static readonly int AGOS_GROUP_LIST_WINDOW_ID = 33022007;
        public static readonly int AGOS_DEBUG_GUI_WINDOW_ID = 43022007;

        public const string AGOS_MAIN_GUI_NAME = "Main";
        public const string AGOS_SETTINGS_GUI_NAME = "Settings";
        public const string AGOS_MANAGER_GUI_NAME = "Manager";
        public const string AGOS_DEBUG_GUI_NAME = "Debug";

        public bool hasSetupStyles = false;
        public bool isGameGUIHidden = false;
        public static readonly List<string> agosKerbalNames = new List<string>() { "iPeer", "Roxy", "Shimmy", "Addle", "Gav", "Kofeyh", "Mator" }; // You have to be super awesome to make it into this list

        #endregion

        #region GUI control vars

        private float throttleLevel = 0f;
        private float timerDelay = 1f;
        private int delayedGroup = 2;
        private string manualGroup = "";
        private string stageList = "";
        private bool useAGXGroup = false;
        private bool debugButtonsVisible = false;
        private int sasMode = 0;
        private string[] sasModeNames;

        #endregion

        #region private vars

        private string[] stockAGNames;
        private KSPActionGroup[] stockAGList;
        private enum AGOSActionGroups
        {
            THROTTLE = -1,
            FINE_CONTROLS = -2,
            CAMERA_AUTO = -3,
            CAMERA_ORBITAL = -4,
            CAMERA_CHASE = -5,
            CAMERA_FREE = -6,
            LOCK_STAGING = -7,
            CAMERA_LOCKED = -8,
            TIMED_ACTION_GROUP = -9,
            SAS_MODE_SWITCH = -10
        }

        #endregion

        #region GUI vars

        private Rect _windowPos = new Rect();
        private Vector2 _scrollPosGroups = Vector2.zero, _scrollPosConfig = Vector2.zero;
        public GUIStyle _buttonStyle,
            _scrollStyle,
            _windowStyle,
            _labelStyle,
            _labelStyleRed,
            _toggleStyle,
            _sliderStyle,
            _sliderSliderStyle,
            _sliderThumbStyle,
            _textFieldStyle,
            _labelCenteredYellow,
            _tinyButtonStyle;

        private Dictionary<string, string> agosGroupPrettyNames = new Dictionary<string, string>() {

            {"FINE_CONTROLS", "Toggle fine controls"},
            {"THROTTLE", "Throttle control"},
            {"CAMERA_AUTO", "Set camera: AUTO"},
            {"CAMERA_ORBITAL", "Set camera: ORBITAL"},
            {"CAMERA_CHASE", "Set camera: CHASE"},
            {"CAMERA_FREE", "Set camera: FREE"},
            {"LOCK_STAGING", "Lock staging"},
            {"CAMERA_LOCKED", "Set camera: LOCKED"},
            {"TIMED_ACTION_GROUP", "Time-delayed Action Group"},
            {"SAS_MODE_SWITCH", "Set SAS mode"}

        };

        #endregion

        #region initialization

        public void Start()
        {

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Logger.Log("AGOS.Main.AGOSMain.Start()");
            Logger.Log("KSP architecture is {0}", (Is64bit ? "x64" : "x86"));
            Settings = new AGroupOnStage.Main.AGOSSettings(IOUtils.GetFilePathFor(this.GetType(), "settings.cfg"));
            // Create the pluginData folder for AGOS, if it doesn't exist
            System.IO.Directory.CreateDirectory(Settings.configPath.Replace("settings.cfg", ""));
            Instance = this;
            useAGXConfig = AGXInterface.isAGXInstalled();
            Logger.Log("This install is " + (useAGXConfig ? "" : "not ") + "running AGX.");
            //if (useAGXConfig) { /* DO NAAHTHING! */ } // AGX is installed - use its controller, not stock's
            //else // Not installed - fall back to stock controller.
            loadActionGroups();
            loadSASModes();
            Logger.Log("Loading AGOS' settings");
            Settings.load();
            if (Settings.get<bool>("EnableDebugOptions"))
                Logger.Log("Debug options are enabled.");
            Logger.Log("AGOS' Settings loaded");

            if (Settings.get<bool>("AddAGOSKerbals"))
                addAGOSKerbals();
            if (Settings.get<bool>("UnloadUnusedAssets")) // 2.0.10-dev2: Remove Buttons texture from memory because we don't need it in memory (it's loaded on-demand in AGOSToolbarManager)
            {
                Logger.Log("Unloading assets that don't need to be in memory...");
                bool success = GameDatabase.Instance.RemoveTexture("iPeer/AGroupOnStage/Textures/Buttons");
                Logger.Log(String.Format("{0} '{1}'", (success ? "Successfully unloaded" : "Couldn't unload"), "iPeer/AGroupOnStage/Textures/Buttons"));
                Logger.Log("Finished cleaning up un-needed assets");
            }

            //GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
            GameEvents.onVesselChange.Add(onVesselLoaded);
            GameEvents.onGameSceneLoadRequested.Add(onSceneLoadRequested);
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
            //GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded); // 2.0.8-dev2: No longer needed.
            GameEvents.onShowUI.Add(onShowUI);
            GameEvents.onHideUI.Add(onHideUI);
            //GameEvents.onGUIAstronautComplexSpawn.Add(onGUIAstronautComplexSpawn);
            //GameEvents.onGUIAstronautComplexDespawn.Add(onGUIAstronautComplexDespawn);
            //AGOSToolbarManager.addToolbarButton();

#if DEBUG
            if (Settings.get<bool>("HereBeDragons"))
            {
                DialogGUIButton[] options = new DialogGUIButton[] 
                {
                    new DialogGUIButton("OK", () => dragonsCallBack(0)),
                    new DialogGUIButton("OK - Don't show again", () => dragonsCallBack(1)),
                    new DialogGUIButton("Issue Page", () => dragonsCallBack(2), false)
                };
                MultiOptionDialog mod = new MultiOptionDialog("HERE BE DRAGONS!\n " +
                "This is a *very* early experimental release of the new AGOS. Things are going to be broken.\n\n"+
                "If you find a bug, which is really quite likely, please report it on AGOS' GitHub issues page.\n\n"+
                "You can get to this page by clicking the \"Issues Page\" button below.\n\n"+
                "When reporting a bug, please include your output_log file and a craft file  and/or persistent file (stock only, please!) if you feel it will help with the report.\n\n"+
                "Please check back at the releases page regularly to see if there's a new release!", "Here be Dragons - AGroupOnStage", HighLogic.UISkin, options);
                PopupDialog.SpawnPopupDialog(mod, false, HighLogic.UISkin);
            }
#endif
            sw.Stop();
            Logger.Log("AGOS {1} initalised in {0}s", sw.Elapsed.TotalSeconds, AGOSUtils.getModVersion());

        }

        private void loadSASModes()
        {
            Logger.Log("Creating SAS mode list...");
            sasModeNames = Enum.GetNames(typeof(VesselAutopilot.AutopilotMode));
            Logger.Log("{0} SAS modes loaded", sasModeNames.Length);
        }

        private void onGUIAstronautComplexSpawn()
        {
            if (guiVisible)
                this.toggleGUI();
        }

        private void onGUIAstronautComplexDespawn()
        {
        }

        private void dragonsCallBack(int opt)
        {
            if (opt == 1)
            {
                Settings.set("HereBeDragons", false);
                Settings.save();
            }
            else if (opt == 2)
            {
                Application.OpenURL("https://github.com/iPeer/AGroupOnStage/issues");
            }
        }

        public void addAGOSKerbals()
        {
            Logger.Log("Trying to add AGOS-related Kerbals to roster");
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                Logger.LogError("Current game is Career mode. Aborting adding Kerbals.");
                return;
            }
            KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
            //List<ProtoCrewMember> kerbals = new List<ProtoCrewMember>();
            foreach (string s in agosKerbalNames)
            {
                string kName = s + " Kerman";
                bool kerbalsPresent = roster[kName] != null;
                if (kerbalsPresent)
                {
                    Logger.LogWarning("{0} has already been signed up (harmless)", kName);
                    continue;
                }

                ProtoCrewMember kerbal = roster.GetNewKerbal(/*ProtoCrewMember.KerbalType.Applicant*/);
                kerbal.name = kName;
                // Pointless code below! Just leave it to the game to do it!
                //kerbal.isBadass = (new System.Random()).NextBoolOneIn(10); // 10%
                kerbal.gender = (kName.StartsWith("Roxy") ? ProtoCrewMember.Gender.Female : ProtoCrewMember.Gender.Male);

                //kerbal.type = ProtoCrewMember.KerbalType.Applicant;
                kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;

                KerbalRoster.SetExperienceTrait(kerbal);

                kerbal.experienceLevel = 5;
                kerbal.experience = 1337;

                Logger.Log("{0} has been voluntold that they're going to be a Kerbonaut (they're thrilled)!", kName);
            }

        }

        public Dictionary<string, ProtoCrewMember.RosterStatus> removeAGOSKerbals()
        {

            Dictionary<string, ProtoCrewMember.RosterStatus> ret = new Dictionary<string, ProtoCrewMember.RosterStatus>();
            foreach (string s in agosKerbalNames)
            {
                string name = s + " Kerman";
                KerbalRoster r = HighLogic.CurrentGame.CrewRoster;
                ProtoCrewMember c = r.Crew.First(k => k.name.Equals(name));
                if (c == null)
                    continue;
                if (c.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                {
                    Logger.Log(name + " has been told they will not go to space today (or tomorrow, or ever) :(");
                    r.Remove(c);
                }
                else
                    ret.Add(name, c.rosterStatus);
            }
            return ret;
        }

        private void onShowUI()
        {
            this.isGameGUIHidden = false;
        }

        private void onHideUI()
        {
            this.isGameGUIHidden = true;
        }

        private void OnGUIApplicationLauncherReady()
        {
            if (Settings.get<bool>("UseStockToolbar") || !_000Toolbar.ToolbarManager.ToolbarAvailable)
                AGOSToolbarManager.setupToolbarButton();
        }

        /*public void backupActionGroupList()
        {
            if (this.actionGroups.Count == 0)
                return;
            foreach (IActionGroup a in this.actionGroups)
                backupActionGroups.Add(a);
            Logger.Log("Backed up {0} group(s)", backupActionGroups.Count);
        }*/

        public void removeDuplicateActionGroups()
        {
            int start = this.actionGroups.Count;
            if (start < 2) // Don't bother if it's impossible for there to be duplicates
                return;
            List<AGOSActionGroup> newList = this.actionGroups.GroupBy(o =>
                new { o.cameraMode, o.fireGroupID, o.FlightID, o.Group, o.isPartLocked, o.linkedPart, o.partRef, o.StagesAsString, o.ThrottleLevel, o.timerDelay, o.Vessel, o.FireType }
                ).Select(n => n.First()).ToList<AGOSActionGroup>();
            int end = newList.Count;
            Logger.Log("Removed {0} duplicate action group(s)", (start - end));
            this.actionGroups = new List<AGOSActionGroup>(newList);
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

        /*public void toggleGUI()
        {
            toggleGUI(false);
        }*/

        public void toggleGUI(bool fromPart = false)
        {

            /*Logger.LogDebug("RenderingManager initialised: {0}", RenderingManager.fetch == null);
            try { bool _ = RenderingManager.fetch.postDrawQueue == null; }
            catch (NullReferenceException) { Logger.LogWarning("RederingManager's postDrawQueue is not initialised!"); RenderingManager.fetch.postDrawQueue = new Callback[0]; }
            Logger.LogDebug("Post Draw queue null?: {0}", RenderingManager.fetch.postDrawQueue == null);*/
            Logger.LogDebug("Scene: {0}", HighLogic.LoadedScene.ToString());
            Logger.LogDebug("Loaded Space Centre?: {0}", AGOSUtils.isLoadedSceneOneOf(GameScenes.SPACECENTER));
            Logger.LogDebug("GUI Visible: {0}", guiVisible);

            if (AGOSUtils.isLoadedSceneOneOf(GameScenes.SPACECENTER))
            {
                Settings.toggleGUI();
                return;
            }
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER && AGOSUtils.getTechLevel(SpaceCenterFacility.VehicleAssemblyBuilding) == 0f)
            {
                ScreenMessages.PostScreenMessage("You do not have access to action groups yet! Upgrade your VAB to unlock them!", 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            if (guiVisible && !fromPart)
            {
                AGOSInputLockManager.removeControlLocksForSceneDelayed(HighLogic.LoadedScene, AGOS_MAIN_GUI_NAME);
                _scrollPosConfig = _scrollPosGroups = Vector2.zero; // 2.0.10-dev2: Reset scroll list positions when GUI closes
                guiVisible = false;
                if (!AGOSToolbarManager.using000Toolbar)
                    AGOSToolbarManager.agosButton.SetFalse(false);
                Logger.LogDebug("GUI Pos update");
                Settings.set("wPosX", _windowPos.x);
                Settings.set("wPosY", _windowPos.y);
                Settings.save();
                Logger.LogDebug("=== /GUI pos update ===");
                if (HighLogic.LoadedSceneIsEditor)
                {
                    if (linkPart != null)
                        linkPart = null;
                    AGOSUtils.resetActionGroupConfig("Main->toggleGUI");
                }

            }
            else
            {
                if (fromPart)
                    this.isPartTriggered = true;
                if (fromPart && guiVisible) { return; }
                if (EditorTooltip.Instance != null) // 2.0.9-dev3: Fix for NRE when opening GUI without visiting the editor first.
                    EditorTooltip.Instance.HideToolTip();
                if (Settings.get<bool>("LockInputsOnGUIOpen"))
                    AGOSInputLockManager.setControlLocksForScene(HighLogic.LoadedScene, AGOS_MAIN_GUI_NAME);
                guiVisible = true;
                if (!AGOSToolbarManager.using000Toolbar)
                    AGOSToolbarManager.agosButton.SetTrue(false);
                _windowPos.x = Settings.get<float>("wPosX");
                _windowPos.y = Settings.get<float>("wPosY");
                Logger.LogDebug("Window ID: {0}", AGOS_GUI_WINDOW_ID);
            }
        }

        public void OnDraw()
        {

            if (!guiVisible) { return; }

            if (!hasSetupStyles)
                setUpStyles();

            if (Settings.get<bool>("RestrictGUIToScreen"))
            {
                _windowPos.x = Mathf.Clamp(_windowPos.x, 0f, Screen.width - _windowPos.width);
                _windowPos.y = Mathf.Clamp(_windowPos.y, 0f, Screen.height - _windowPos.height);
            }

            _windowPos = GUILayout.Window(AGOS_GUI_WINDOW_ID, _windowPos, OnWindow, String.Format("AGroupOnStage {0} {1}", AGOSUtils.getModVersion(), (AGOSDebug.isDebugBuild() ? "(Debug Mode / KSP "+Versioning.GetVersionStringFull()+")" : "")), _windowStyle, GUILayout.MinHeight(500f), GUILayout.MinWidth(500f), GUILayout.MaxWidth(500f));
            // TODO: GUI position sanity checks
        }

        public void renderGroupButton(int x)
        {
            if ((x == 0 || x == 1 || x == -7) || !AGOSUtils.techLevelEnoughForGroup(x)) { return; } // "None", "Stage" and "Lock Staging" action groups
            if (useAGXConfig && x >= 8)
            {
                string groupName = (x - 7 < 0 ? actionGroupList[x] : x - 7 + (AGXInterface.getAGXGroupName(x - 7) != "" ? ": " + AGXInterface.getAGXGroupName(x - 7) : ""));
                actionGroupSettings[x] = GUILayout.Toggle(actionGroupSettings.ContainsKey(x) ? actionGroupSettings[x] : false, groupName, _buttonStyle);
            }
            else
                actionGroupSettings[x] = GUILayout.Toggle(actionGroupSettings.ContainsKey(x) ? actionGroupSettings[x] : false, actionGroupList[x], _buttonStyle);
        }


        private void OnWindow(int windowID)
        {
            if (SpecialOccasion && Settings.get<bool>("DisplaySpecialOccasions"))
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                DateTime dt = DateTime.Now;
                string form = String.Format("{0}{1}", dt.Day, dt.Month);
                string occasionMessage = "Today is a special day!";
                if (specialOccasionDates.ContainsKey(form))
                    occasionMessage = specialOccasionDates[form];
                GUILayout.Label(occasionMessage, _labelCenteredYellow);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

            }

            // Draw settings button
            /*Rect settingsBtnRect;
            if (linkPart != null)*/
            Rect settingsBtnRect = new Rect(_windowPos.width - 20, 3, 16, 16);
            /*else
                settingsBtnRect = new Rect(_windowPos.width - 20, 20, 16, 16);*/
            if (GUI.Button(settingsBtnRect, Settings.buttonTex, _tinyButtonStyle))
            {
                Settings.toggleGUI();
            }
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
            if (Settings.get<bool>("AGOSGroupsLast"))
            {
                bool hasLooped = false;
                for (int x = 0; x <= AG_MAX; x++)
                {
                    if (x == AG_MAX)
                    {
                        x = AG_MIN;
                        hasLooped = true;
                    }
                    if (x == 0 && hasLooped)
                        break;

                    renderGroupButton(x);
                }
            }
            else
            {
                for (int x = AG_MIN; x < AG_MAX; x++)
                {
                    //Logger.Log("AG {0}: {1}", x, actionGroupList[x]);
                    renderGroupButton(x);

                }
            }
            /*}*/
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(240f));
            GUILayout.BeginHorizontal();

            isPartTriggered = !GUILayout.Toggle(!isPartTriggered, "Stage(s)", _buttonStyle);
            isPartTriggered = GUILayout.Toggle(isPartTriggered, "Part", _buttonStyle);

            GUILayout.EndHorizontal();
            if (isPartTriggered)
            {
                if (linkPart != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Part: " + linkPart.name + "_" + linkPart.craftID, _labelStyle);
                    if (GUILayout.Button("X", _buttonStyle, GUILayout.MaxWidth(30f)))
                        linkPart = null;
                    if (GUILayout.Button(new GUIContent("C", "Change part"), _buttonStyle, GUILayout.MaxWidth(30f)))
                    {
                        linkPart = null;
                        AGOSPartSelectionHandler.Instance.enterPartSelectionMode();
                        this.isPartTriggered = true; // Sanity
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    bool selectingPart = AGOSPartSelectionHandler.Instance.partSelectModeActive;
                    string buttonText = selectingPart ? "Cancel" : "Select part from scene";
                    if (GUILayout.Button(buttonText, _buttonStyle))
                    {
                        if (selectingPart)
                            AGOSPartSelectionHandler.Instance.exitPartSelectionMode();
                        else
                            AGOSPartSelectionHandler.Instance.enterPartSelectionMode();
                        selectingPart = !selectingPart;
                    }
                }
            }
            else
            {
                GUILayout.Label("Stages: ", _labelStyle);
                stageList = GUILayout.TextField(stageList, _textFieldStyle);
                GUILayout.Label("Separate multiple by a comma (,)", _labelStyle);
            }
            GUILayout.Space(4);
            if (actionGroupSettings[actionGroupList.First(a => a.Value.Contains("Throttle")).Key] || actionGroupSettings[actionGroupList.First(a => a.Value.Contains("Time-delayed")).Key])
            {
                bool isThrottle = actionGroupSettings[actionGroupList.First(a => a.Value.Contains("Throttle")).Key];

                GUILayout.Label((isThrottle ? "Throttle control:" : "Time delay"), _labelStyle);
                GUILayout.BeginHorizontal(GUILayout.Width(240f));
                if (isThrottle)
                {
                    throttleLevel = GUILayout.HorizontalSlider(throttleLevel, 0f, 1f, _sliderSliderStyle, _sliderThumbStyle);
                    GUILayout.Label(String.Format("{0:P0}", throttleLevel), _labelStyle);
                }
                else
                {
                    GUILayout.BeginVertical();
                    GUILayout.BeginHorizontal();

                    timerDelay = GUILayout.HorizontalSlider(timerDelay, 1f, Settings.get<float>("MaxGroupTimeDelay"), _sliderSliderStyle, _sliderThumbStyle);
                    GUILayout.Label(String.Format("Delay: {0:N0}s", timerDelay), _labelStyle);

                    GUILayout.EndHorizontal();


                    if (useAGXConfig && !isThrottle)
                    {
                        useAGXGroup = GUILayout.Toggle(useAGXGroup, "Fire an AGX group", _toggleStyle);
                    }

                    if (useAGXGroup)
                    {

                        GUILayout.BeginVertical();
                        GUILayout.Label("Enter which AGX group to fire:", _labelStyle);
                        manualGroup = GUILayout.TextField(manualGroup, _textFieldStyle);

                        /*if (manualGroup.toInt32() < 1 && manualGroup != "")
                            manualGroup = "1";
                        else if (manualGroup.toInt32() > 250)
                            manualGroup = "250";*/

                        string groupLabel = "INVALID";
                        if (manualGroup.isInt32())
                            groupLabel = manualGroup.toInt32() + (AGXInterface.getAGXGroupName(manualGroup.toInt32()) != "" ? " [" + AGXInterface.getAGXGroupName(manualGroup.toInt32()) + "]" : "");

                        GUILayout.Label(String.Format("Group: {0}", groupLabel), _labelStyle);
                        GUILayout.EndVertical();

                    }
                    else
                    {

                        int[] groupBoundries = getMinMaxGroupIds();
                        float max = (useAGXConfig ? 7f : Convert.ToSingle(groupBoundries[1] - 1f));

                        delayedGroup = Convert.ToInt32(GUILayout.HorizontalSlider(Convert.ToSingle(delayedGroup), 2f, max, _sliderSliderStyle, _sliderThumbStyle));
                        GUILayout.Label(String.Format("Group: {0}", actionGroupList[delayedGroup]), _labelStyle);

                    }

                    GUILayout.EndVertical();



                }
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }

            if (actionGroupSettings[-10]) // SAS control
            {
                GUILayout.BeginHorizontal();
                sasMode = GUILayout.SelectionGrid(sasMode, sasModeNames, 2, _buttonStyle);
                GUILayout.EndHorizontal();
            }
            bool hasStageList = !string.IsNullOrEmpty(stageList);
            bool hasLinkedPart = !(linkPart == null);
            bool hasGroups = !(actionGroupSettings.Values.Count(a => a) == 0);
            if ((!hasStageList && !hasLinkedPart) || !hasGroups) // Not configued
            {
                GUILayout.Label("This group is not correctly configured!", _labelStyleRed);
            }
            else
            {
                if (GUILayout.Button("Commit group(s)", _buttonStyle))
                {
                    commitGroups();
                }
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            //IActionGroup[] groups = actionGroups.ToArray();
            List<AGOSActionGroup> groups = new List<AGOSActionGroup>(); //                              2.0.8-dev2: Filter action groups to the current flightID only.
            groups.AddRange(actionGroups.FindAll(g => AGOSUtils.isLoadedCraftID(g.FlightID))); // ^

            List<ActionGroupTimer> timers = new List<ActionGroupTimer>();
            if (HighLogic.LoadedSceneIsFlight)
                timers.AddRange(AGOSActionGroupTimerManager.Instance.activeTimersForVessel(FlightGlobals.fetch.activeVessel.rootPart.flightID));


            if (groups.Count > 0 || timers.Count > 0)
                _scrollPosConfig = GUILayout.BeginScrollView(_scrollPosConfig, _scrollStyle);


            if (timers.Count > 0)
            {
                foreach (ActionGroupTimer t in timers)
                    GUILayout.Label(String.Format("Firing group {0} in {1} seconds", t.Group, t.RemainingDelay), _labelStyle);
            }

            if (groups.Count > 0)
            {

                foreach (AGOSActionGroup ag in groups)
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
                    else if (ag.FireType == AGOSActionGroup.FireTypes.STAGE)
                    {
                        int[] stages = ag.Stages;
                        stagesString = AGOSUtils.intArrayToString(stages, ", ");
                    }
                    else
                    {
                        stagesString = (ag.FireType == AGOSActionGroup.FireTypes.DOCK ? "DOCK" : "UNDOCK");
                        /*if (AGOSDebug.isDebugBuild() || Settings.get<bool>("EnableDebugOptions"))
                            stagesString += ": " + AGOSUtils.isGroupValidForVessel(ag);*/
                    }

                    GUILayout.BeginHorizontal();
                    GUIStyle __labelStyle = _labelStyle;
                    if (!AGOSUtils.isGroupValidForVessel(ag))
                        __labelStyle = _labelStyleRed;
                    string groupName;
                    string groupDescription = "";
                    if (ag.GetType() == typeof(TimeDelayedActionGroup))
                        groupName = String.Format("'{0}' after {1}s", (useAGXConfig && ag.fireGroupID >= 8 ? "" + (ag.fireGroupID - 7) : actionGroupList[ag.fireGroupID]), String.Format("{0:N0}", ag.timerDelay));
                    else if (ag.GetType() == typeof(SASModeChangeGroup))
                        groupName = String.Format("Set SAS to '{0}'", ((VesselAutopilot.AutopilotMode)ag.fireGroupID).ToString());
                    else if (useAGXConfig && ag.Group >= 8)
                        groupName = ag.Group - 7 + (AGXInterface.getAGXGroupName(ag.Group - 7) != "" ? ": " + AGXInterface.getAGXGroupName(ag.Group - 7) : "");
                    else
                        groupName = actionGroupList[ag.Group];
                    if (ag.IsTester)
                        groupName = "* " + groupName;
                    if (ag.GetType() == typeof(ThrottleControlActionGroup))
                        groupDescription = String.Format(" ({0:P0})", ag.ThrottleLevel);
                    GUILayout.Label(groupName + (groupDescription.Length > 0 ? " " + groupDescription : ""), __labelStyle, GUILayout.MinWidth(150f));

                    GUILayout.Label(stagesString, __labelStyle);
                    if (GUILayout.Button("Edit", _buttonStyle, GUILayout.MaxWidth(40f)))
                    {
                        if (linkPart != null)
                            linkPart = null;
                        actionGroupSettings[ag.Group] = true;
                        throttleLevel = ag.ThrottleLevel;
                        delayedGroup = ag.fireGroupID;
                        timerDelay = ag.timerDelay;
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
                    if (ag.IsTester)
                        if (GUILayout.Button("S", _buttonStyle, GUILayout.MaxWidth(30f)))
                            ag.IsTester = false;

                    GUILayout.EndHorizontal();
                }

            }

            if (groups.Count > 0 || timers.Count > 0)
                GUILayout.EndScrollView();

            if (groups.Count == 0 && (!HighLogic.LoadedSceneIsFlight || timers.Count == 0))
            {
                GUILayout.Label((AGOSUtils.getVesselPartsList().Count == 0 ? "No vessel is loaded!" : "There are no groups or timers configured for this vessel"), _labelCenteredYellow, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            if (AGOSDebug.isDebugBuild() || Settings.get<bool>("EnableDebugOptions"))
            {
                if (debugButtonsVisible)
                {
                    GUILayout.BeginHorizontal();
                    /*if (GUILayout.Button("DEBUG: Dump groups", _buttonStyle))
                        AGOSGroupManager.dumpActionGroupConfig();*/

                    if (GUILayout.Button("DEBUG: Toggle debug window", _buttonStyle))
                        AGOSDebug.toggleGUI();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    /*if (GUILayout.Button("DEBUG: Show AGs", _buttonStyle))
                    {
                        GroupManager.toggleGUI();
                    }*/
                    if (GUILayout.Button("DEBUG: Display active lock list", _buttonStyle))
                    {
                        AGOSInputLockManager.DEBUGListActiveLocks();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("DEBUG: Dump all possible control locks", _buttonStyle))
                    {
                        AGOSInputLockManager.DEBUGListAllPossibleLocks();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(string.Format("DEBUG: {0} debug options", debugButtonsVisible ? "Hide" : "Show")))
                {
                    debugButtonsVisible = !debugButtonsVisible;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close", _buttonStyle))
                toggleGUI();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow(); // Make window dragable

        }

        private void commitGroups()
        {
            Logger.Log("Commiting current action group configuration...");

            // Check if the config is valid (IE no fields are blank)
            // Shouldn't actually be possible to get this far with a misconfigured group, but I like my sanity.

            Logger.Log("Checking configuration parameters:");
            bool hasStageList = !string.IsNullOrEmpty(stageList);
            bool hasLinkedPart = !(linkPart == null);
            bool hasGroups = !(actionGroupSettings.Values.Count(a => a) == 0);
            Logger.Log("{0} / {1} ({2}), {3} | {4}", hasStageList ? "PASS" : "FAIL", hasLinkedPart ? "PASS" : "FAIL", !hasStageList && !hasLinkedPart ? "FAIL" : "PASS", hasGroups ? "PASS" : "FAIL", (!hasStageList && !hasLinkedPart) || !hasGroups ? "FAIL" : "PASS");
            if ((!hasStageList && !hasLinkedPart) || !hasGroups)
            {

                Logger.LogWarning("Action group is not configured properly, aborting.");
                return;

            }

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
                    CAMERA_LOCKED = -8,
                    TIMED_ACTION_GROUP = -9
                    SAS_MODE_SWITCH = -10
                    */

                    AGOSActionGroup ag;
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
                    else if (x == -9)
                    {
                        ag = new TimeDelayedActionGroup();
                        ag.timerDelay = Convert.ToInt32(Math.Floor(timerDelay));
                        ag.fireGroupID = delayedGroup;
                    }
                    else if (x == -10)
                    {
                        ag = new SASModeChangeGroup();
                        ag.fireGroupID = sasMode;
                    }
                    else
                    {
                        ag = new BasicActionGroup();
                    }
                    if (linkPart != null && isPartTriggered)
                    {
                        ag.linkedPart = linkPart;
                        ag.isPartLocked = true;
                        ag.partRef = linkPart.savedPartName();
                    }
                    else
                    {
                        List<int> stages = new List<int>();
                        string[] sList = stageList.Split(',');
                        foreach(string stage in sList) {
                            try {
                                stages.Add(Convert.ToInt32(stage));
                            }
                            catch
                            {
                                Logger.LogWarning("Couldn't parse stage number '{0}'. Skipping", stage);
                            }
                        }
                        if (stages.Count > 0) // 2.0.10-dev4: Fix for groups containing only non-numeric invalid stages being added to the list with no stages configured
                            ag.Stages = stages.ToArray();
                        else
                            continue;
                    }

                    if (useAGXGroup)
                    {
                        if (!manualGroup.isInt32())
                        {
                            Logger.LogWarning("Action group was configured to use an AGX group ID, but provided ID ('{0}') was invalid. Skipping.", manualGroup);
                            continue;
                        }

                        ag.Group = manualGroup.toInt32();

                    }
                    else
                    {
                        ag.Group = x;
                    }

                    ag.FlightID = ag.OriginalFlightID = AGOSUtils.getFlightID();

                    //Logger.Log("\t{0}", ag.ToString());
                    actionGroups.Add(ag);
                    actionGroupSettings[x] = false;

                }
            }
            throttleLevel = 0f;
            delayedGroup = 2;
            timerDelay = 1f;
            stageList = "";
            manualGroup = "";
            sasMode = 0;
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

        public GUISkin setUpStyles()
        {
            Logger.Log("Setting up GUI styles");
            hasSetupStyles = true;
            GUISkin skin = AGOSUtils.getBestAvailableSkin()/*HighLogic.Skin*/;
            Logger.LogDebug("Skin name: {0}", skin.name);
            _windowStyle = new GUIStyle(skin.window);
            _windowStyle.stretchHeight = true;
            _windowStyle.stretchWidth = true;
            _buttonStyle = new GUIStyle(skin.button);
            _labelStyle = new GUIStyle(skin.label);
            _labelStyle.stretchWidth = true;
            _labelStyleRed = new GUIStyle(skin.label);
            _labelStyleRed.stretchWidth = true;
            _labelStyleRed.normal.textColor = XKCDColors.Red;
            _toggleStyle = new GUIStyle(skin.toggle);
            _sliderStyle = new GUIStyle(skin.horizontalSlider);
            _sliderSliderStyle = skin.horizontalSlider;
            _sliderThumbStyle = skin.horizontalSliderThumb;
            _sliderStyle.stretchWidth = true;
            _scrollStyle = new GUIStyle(skin.scrollView);
            _scrollStyle.stretchHeight = true;
            _textFieldStyle = new GUIStyle(skin.textField);
            _textFieldStyle.fixedWidth = 235f;
            _labelCenteredYellow = new GUIStyle(skin.label);
            _labelCenteredYellow.normal.textColor = Color.yellow;
            _labelCenteredYellow.stretchWidth = true;
            _labelCenteredYellow.alignment = TextAnchor.MiddleCenter;
            //_labelCenteredYellow.stretchHeight = true;
            _tinyButtonStyle = new GUIStyle(skin.button);
            _tinyButtonStyle.clipping = TextClipping.Overflow;
            //_tinyButtonStyle.alignment = TextAnchor.MiddleCenter;
            _tinyButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            _tinyButtonStyle.margin = new RectOffset();
            Logger.Log("Done setting up GUI styles");
            return skin;
        }

        #endregion

        #region saving and loading

        // These aren't actually used.
        /*public void OnSave(ConfigNode node)
        {
            Logger.Log("AGOS.Main.AGOSMain.OnSave()");
            Logger.Log("Vessel name is '{0}'", (HighLogic.LoadedSceneIsEditor ? EditorLogic.fetch.shipNameField.Text : FlightGlobals.fetch.activeVessel.vesselName));
        }

        public void OnLoad(ConfigNode node)
        {
            Logger.Log("AGOS.Main.AGOSMain.OnLoad()");
            Logger.Log("Vessel name is '{0}'", (HighLogic.LoadedSceneIsEditor ? EditorLogic.fetch.shipNameField.Text : FlightGlobals.fetch.activeVessel.vesselName));
        }*/

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

        public List<AGOSModule> getAllMasterAGOSModules(Vessel v)
        {
            List<Part> parts = new List<Part>();
            if (HighLogic.LoadedSceneIsEditor)
                parts = EditorLogic.fetch.ship.parts;
            else
                parts = v.parts;
            return (from p in parts from m in p.Modules.OfType<AGOSModule>() where m.isRoot select m).ToList();
        }

        [Obsolete("Use findHomesForPartLockedGroups() instead", true)]
        public void updatePartLockedStages(bool suppressLog = false)
        {
            List<AGOSActionGroup> toUpdate = actionGroups.FindAll(a => a.linkedPart != null);
            if (toUpdate.Count == 0)
            {
                if (!suppressLog)
                    Logger.Log("No part locked groups to update");
                return;
            }
            foreach (AGOSActionGroup ag in toUpdate)
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
            List<AGOSActionGroup> partLinkedGroups = actionGroups.FindAll(a => a.isPartLocked && a.linkedPart == null);
            Logger.Log("{0} homeless group(s)", partLinkedGroups.Count());
            foreach (AGOSActionGroup g in partLinkedGroups)
            {
                Part part = AGOSUtils.findPartByReference(g.partRef, vessel);
                if (part == null)
                {
                    Logger.LogWarning("Action group '{1}' supplied invalid part reference '{0}', skipping.", g.partRef, g.Group);
                    continue;
                }
                g.linkedPart = part;
                Logger.Log("Action group '{2}' and part '{0}' ({1}) have been paired", part.partInfo.title, part.savedPartName(), g.Group);
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
                    //AGOSMain.Instance.restoreBackedUpActionGroups(false); // 2.0.6-dev1: Changed to false to prevent duping if player reverts multiple times (-> launch [-> launch [-> ...]] -> editor)
                    removeDuplicateActionGroups(); // 2.0.8-dev2: Updated code for removal of duplicated action groups
                    //AGOSMain.Instance.removeInvalidActionGroups(); // 2.0.8-dev2: Fix for invalid (0 flightID) groups from polluting nearby vessels
                    findHomesForPartLockedGroups(AGOSUtils.getVesselPartsList());
                    //AGOSDebug.printAllActionGroups();
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
            AGOSToolbarManager.enableToolbarButton(); // 2.0.11-dev1: Force enable toolbar buttons on scene change (Fixes #24)
            if (guiVisible)
                toggleGUI();
            if (Settings.guiVisible && !scene.ToString().Equals("SPACECENTER"))
                Settings.toggleGUI();
            /*if (!FlightDriver.CanRevert)
                Logger.Log("Player cannot revert, no group backup will be taken.");*/
            /*if (HighLogic.LoadedSceneIsEditor) // No longer required as of 2.0.8-dev2
            {
                backupActionGroups.Clear(); // 2.0.6-dev1 fix for yet another dupe (I hope)
                backupActionGroupList();
            }*/
            if (HighLogic.LoadedSceneIsFlight)
            {
                //getMasterAGOSModule(FlightGlobals.fetch.activeVessel).setFlightID(0, false, FlightGlobals.fetch.activeVessel.rootPart.flightID); // Reset action groups for active vessel to id 0 so they show uo properly in the editor.
                AGOSUtils.resetActionGroupConfig("Main->onSceneLoadRequested", true);
            }
        }

        private void onLevelWasLoaded(GameScenes level)
        {

            /*if (HighLogic.LoadedSceneIsFlight && revertState != null && possibleRevertDetected)
            {
                if (FlightGlobals.ActiveVessel.Equals(revertState.Vessel) && FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
                {

                    Logger.Log("A flight revert has been detected, resetting action group list to that of the revertState");
                    this.actionGroups = new List<IActionGroup>(revertState.Groups);
                    revertState = null;

                }
            }*/

        }

        public static void ResetSettings()
        {
            if (Settings.guiVisible)
                Settings.toggleGUI();
            Settings.removeFile();
            Settings = new AGOSSettings(Settings.configPath);
        }

        [Obsolete("Use AGOSActionGroup.removeIfNoTriggers() instead", true)]
        public void removeGroupsWithNoTriggers()
        {
            List<AGOSActionGroup> toRemove = actionGroups.FindAll(a => !a.stillHasTriggers);
            Logger.Log("Removing {0} group(s) that no longer have valid triggers", toRemove.Count);
            actionGroups.RemoveRange(toRemove);
        }
    }
}
