using AGroupOnStage._000Toolbar;
using AGroupOnStage.ActionGroups;
using AGroupOnStage.AGX;
using AGroupOnStage.Logging;
using AGroupOnStage.Extensions;
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
        public static readonly Dictionary<string, string> specialOccasionDates = new Dictionary<string, string>() 
        {

            {"66", "Today is iPeer's birthday!"},
            {"244", "Today is Roxy's birthday!"},
            {"89", "Today is AGroupOnStage's birthday!"},
            {"2412", "Santa Claus is coming to town!"},
            {"2515", "Merry Christmas!"},
            {"11", "Happy New Year!"}


        };
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
        public bool guiVisible = false;
        public bool settingsGUIVisible = false;
        public bool useAGXConfig = false;
        /*public bool using000Toolbar = false;
        public bool launcherButtonAdded = false;*/
        public List<IActionGroup> actionGroups = new List<IActionGroup>();
        public Dictionary<int, string> actionGroupList = new Dictionary<int, string>();
        public Dictionary<int, bool> actionGroupSettings = new Dictionary<int, bool>();
        public Dictionary<int, KSPActionGroup> stockAGMap;
        public static AGOSSettings Settings { get; protected set; }
        public static AGOSGroupManager GroupManager { get; protected set; }
        public static AGOSToolbarManager ToolbarManager { get; protected set; }
        public bool FlightEventsRegistered { get; set; }
        public bool EditorEventsRegistered { get; set; }
        public static readonly int AGOS_GUI_WINDOW_ID = 03022007;
        //                                              ^ pointless 0 is pointless
        public static readonly int AGOS_DRAGONS_GUI_WINDOW_ID = 13022007;
        public static readonly int AGOS_SETTINGS_GUI_WINDOW_ID = 23022007;
        public static readonly int AGOS_SETTINGS_CONFIRM_GUI_WINDOW_ID = 33022007;
        public static readonly int AGOS_SETTINGS_ERROR_GUI_WINDOW_ID = 43022007;
        public static readonly int AGOS_GROUP_LIST_WINDOW_ID = 53022007;
        public static readonly int AGOS_DEBUG_GUI_WINDOW_ID = 63022007;

        public bool hasSetupStyles = false;
        /*public ApplicationLauncherButton agosButton = null;
        public IButton _000agosButton = null;*/
        public bool isGameGUIHidden = false;
        public static readonly List<string> agosKerbalNames = new List<string>() { "iPeer", "Roxy", "Shimmy", "Addle", "Gav", "Kofeyh" }; // You have to be super awesome to make it into this list


        #endregion

        #region GUI control vars

        private float throttleLevel = 0f;
        private float timerDelay = 1f;
        private int delayedGroup = 2;
        private string manualGroup = "";
        private string stageList = "";
        private bool useAGXGroup = false;

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
            TIMED_ACTION_GROUP = -9
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
            {"TIMED_ACTION_GROUP", "Time-delayed Action Group"}

        };

        #endregion

        #region initialization

        public void Start()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
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
            if (Settings.get<bool>("EnableDebugOptions"))
                Logger.Log("Debug options are enabled.");
            Logger.Log("AGOS' Settings loaded");
            GroupManager = new AGOSGroupManager();
            ToolbarManager = new AGOSToolbarManager();

            if (Settings.get<bool>("AddAGOSKerbals"))
                addAGOSKerbals();

            //GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
            GameEvents.onVesselChange.Add(onVesselLoaded);
            GameEvents.onGameSceneLoadRequested.Add(onSceneLoadRequested);
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
            //GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded); // 2.0.8-dev2: No longer needed.
            GameEvents.onEditorUndo.Add(OnEditorUndo);
            GameEvents.onEditorRedo.Add(OnEditorUndo);
            GameEvents.onShowUI.Add(onShowUI);
            GameEvents.onHideUI.Add(onHideUI);
            ToolbarManager.addToolbarButton();

#if DEBUG
            if (Settings.get<bool>("HereBeDragons"))
                RenderingManager.AddToPostDrawQueue(AGOS_GUI_WINDOW_ID - 1, OnDraw_Dragons);
#endif
            sw.Stop();
            Logger.Log("AGOS initalised in {0}s", sw.Elapsed.TotalSeconds);
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
                bool kerbalsPresent = roster.Crew.Count(i => i.name.Equals(kName)) > 0;
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
                ToolbarManager.setupToolbarButton();
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
            List<IActionGroup> newList = this.actionGroups.GroupBy(o =>
                new { o.cameraMode, o.fireGroupID, o.FlightID, o.Group, o.isPartLocked, o.linkedPart, o.partRef, o.StagesAsString, o.ThrottleLevel, o.timerDelay, o.Vessel }
                ).Select(n => n.First()).ToList<IActionGroup>();
            int end = newList.Count;
            Logger.Log("Removed {0} duplicate action groups(s)", (start - end));
            this.actionGroups = new List<IActionGroup>(newList);
        }

        public void removeInvalidActionGroups()
        {
            int start = this.actionGroups.Count;
            List<IActionGroup> newList = new List<IActionGroup>(this.actionGroups.RemoveAll(a => a.FlightID == 0));
            int end = newList.Count;
            Logger.Log("Removed {0} invalid action groups", (start - end));
            this.actionGroups = new List<IActionGroup>(newList);
        }

        /*[Obsolete("Use removeDuplicateActionGroups instead", true)]
        public void restoreBackedUpActionGroups()
        {
            restoreBackedUpActionGroups(false);
        }*/

        /*[Obsolete("Use removeDuplicateActionGroups instead", true)]
        public void restoreBackedUpActionGroups(bool clear)
        {
            if (backupActionGroups != null && backupActionGroups.Count > 0)
            {
                int thisVesselsGroups = this.actionGroups.Count(a => a.FlightID == AGOSUtils.getFlightID());
                Logger.Log("B:{0} / L:{1}", backupActionGroups.Count, thisVesselsGroups);
                if (backupActionGroups.Count == this.actionGroups.Count)
                    return;
                this.actionGroups.RemoveAll(a => a.FlightID == AGOSUtils.getFlightID());
                //this.actionGroups.Clear();
                foreach (IActionGroup a in backupActionGroups.FindAll(a => a.FlightID == AGOSUtils.getFlightID()))
                    this.actionGroups.Add(a);
                Logger.Log("Restored {0} group(s)", backupActionGroups.Count(a => a.FlightID == AGOSUtils.getFlightID()));
                if (clear)
                    backupActionGroups.Clear();
            }
        }*/

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

        public void toggleGUI()
        {
            toggleGUI(false);
        }

        public void toggleGUI(bool fromPart)
        {

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
                if (EditorLogic.fetch == null) // 2.0.9-dev3: Fix for NRE when opening GUI without visiting the editor first.
                {
                    Logger.LogWarning("Couldn't remove control locks because the player hasn't visited the Editor yet! (EditorLogic.fetch == null)");
                }
                else
                {
                    EditorLogic.fetch.Unlock("AGOS_INPUT_LOCK");
                }
                guiVisible = false;
                if (!ToolbarManager.using000Toolbar)
                    ToolbarManager.agosButton.SetFalse(false);
                RenderingManager.RemoveFromPostDrawQueue(AGOS_GUI_WINDOW_ID, OnDraw);
                Settings.set("wPosX", _windowPos.x);
                Settings.set("wPosY", _windowPos.y);
                Settings.save();
                if (HighLogic.LoadedSceneIsEditor)
                {
                    if (linkPart != null)
                        linkPart = null;
                    AGOSUtils.resetActionGroupConfig();
                }

            }
            else
            {
                if (fromPart && guiVisible) { return; }
                if (EditorTooltip.Instance != null) // 2.0.9-dev3: Fix for NRE when opening GUI without visiting the editor first.
                    EditorTooltip.Instance.HideToolTip();
                if (Settings.get<bool>("LockInputsOnGUIOpen"))
                    if (EditorLogic.fetch == null) // 2.0.9-dev3: Fix for NRE when opening GUI without visiting the editor first.
                    {
                        Logger.LogWarning("Couldn't apply control locks because the player hasn't visited the Editor yet! (EditorLogic.fetch == null)");
                    }
                    else
                    {
                        EditorLogic.fetch.Lock(true, true, true, "AGOS_INPUT_LOCK");
                    }
                guiVisible = true;
                if (!ToolbarManager.using000Toolbar)
                    ToolbarManager.agosButton.SetTrue(false);
                _windowPos.x = Settings.get<float>("wPosX");
                _windowPos.y = Settings.get<float>("wPosY");
                RenderingManager.AddToPostDrawQueue(AGOS_GUI_WINDOW_ID, OnDraw);
            }
        }

        private void OnDraw()
        {

            if (!hasSetupStyles)
                setUpStyles();
            _windowPos = GUILayout.Window(AGOS_GUI_WINDOW_ID, _windowPos, OnWindow, "Action group control", _windowStyle, GUILayout.MinHeight(500f), GUILayout.MinWidth(500f), GUILayout.MaxWidth(500f));
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
            if (SpecialOccasion)
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
            if (GUI.Button(new Rect(_windowPos.width - 50, 3, 45, 12), "...", _tinyButtonStyle))
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

            if (GUILayout.Button("Commit group(s)", _buttonStyle))
            {
                commitGroups();
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            //IActionGroup[] groups = actionGroups.ToArray();
            List<IActionGroup> groups = new List<IActionGroup>(); //                              2.0.8-dev2: Filter action groups to the current flightID only.
            groups.AddRange(actionGroups.FindAll(g => AGOSUtils.isLoadedCraftID(g.FlightID))); // ^

            if (groups.Count > 0)
            {
                _scrollPosConfig = GUILayout.BeginScrollView(_scrollPosConfig, _scrollStyle);

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
                    GUIStyle __labelStyle = _labelStyle;
                    if (!AGOSUtils.isGroupValidForVessel(ag))
                        __labelStyle = _labelStyleRed;
                    string groupName;
                    string groupDescription = "";
                    if (ag.GetType() == typeof(TimeDelayedActionGroup))
                        groupName = String.Format("Fire group '{0}' after {1}s", (useAGXConfig && ag.fireGroupID >= 8 ? "" + (ag.fireGroupID - 7) : actionGroupList[ag.fireGroupID]), String.Format("{0:N0}", ag.timerDelay));
                    else if (useAGXConfig && ag.Group >= 8)
                        groupName = ag.Group - 7 + (AGXInterface.getAGXGroupName(ag.Group - 7) != "" ? ": " + AGXInterface.getAGXGroupName(ag.Group - 7) : "");
                    else
                        groupName = actionGroupList[ag.Group];
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

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label((AGOSUtils.getVesselPartsList().Count == 0 ? "No vessel is loaded!" : "There are no groups configured for this vessel"), _labelCenteredYellow, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            if (AGOSDebug.isDebugBuild() || Settings.get<bool>("EnableDebugOptions"))
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("DEBUG: Dump groups", _buttonStyle))
                    AGOSGroupManager.dumpActionGroupConfig();

                if (GUILayout.Button("DEBUG: Toggle debug window", _buttonStyle))
                    AGOSDebug.toggleGUI();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("DEBUG: Show AGs", _buttonStyle))
                {
                    GroupManager.toggleGUI();
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
                    else if (x == -9)
                    {
                        ag = new TimeDelayedActionGroup();
                        ag.timerDelay = Convert.ToInt32(Math.Floor(timerDelay));
                        ag.fireGroupID = delayedGroup;
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

                    ag.FlightID = AGOSUtils.getFlightID();

                    Logger.Log("\t{0}", ag.ToString());
                    actionGroups.Add(ag);
                    actionGroupSettings[x] = false;

                }
            }
            throttleLevel = 0f;
            delayedGroup = 2;
            timerDelay = 1f;
            stageList = "";
            manualGroup = "";
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

        public void setUpStyles()
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
            _tinyButtonStyle.padding = new RectOffset(0, 2, 0, 3);
            _tinyButtonStyle.margin = new RectOffset();
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
            if (this.guiVisible)
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
            AGOSUtils.resetActionGroupConfig(true);
        }


        private void OnEditorUndo(ShipConstruct data)
        {
            Logger.Log("Undo/Redo");
            //AGOSUtils.resetActionGroupConfig();
            findHomesForPartLockedGroups(data.parts);
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

        #region herebedragons
        // Here be dragons GUI on startup

        private void OnDraw_Dragons()
        {

            /*if (!hasSetupStyles)
                setUpStyles();*/
            _windowPos = GUILayout.Window(AGOS_GUI_WINDOW_ID - 1, _windowPos, OnWindow_Dragons, "Roar!", HighLogic.Skin.window);

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
            if (GUILayout.Button("Do not anger dragons. Got it."))
            {
                //Settings.set("HereBeDragons", false);
                //Settings.save();
                RenderingManager.RemoveFromPostDrawQueue(AGOS_GUI_WINDOW_ID - 1, OnDraw_Dragons);
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("You can stop this GUI popping up in AGOS' settings!");
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        #endregion
    }
}
