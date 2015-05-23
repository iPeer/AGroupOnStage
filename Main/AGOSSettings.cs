using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{
    public class AGOSSettings : MonoBehaviour
    {

        public bool guiVisible = false;

        public string configPath;
        /*public bool INSTANT_CAMERA_TRANSITIONS = true;
        public bool SHOW_DRAGONS_DIALOG = true;
        public float WIN_POS_X = 0, WIN_POS_Y = 0f;
        public bool LOG_NODE_SAVE = false;
        public bool ALLOW_EE = true;*/

        private Dictionary<string, object> SETTINGS_MAP;
        private Rect _winPos = new Rect();
        private Rect _winPosOther = new Rect();
        private int otherWinID = 0;
        private GUIStyle _windowStyle;
        private Dictionary<string, string> configPrettyNames = new Dictionary<string, string> // Even though non-boolean items aren't displayed, still list them here for future-proofing.
        {
            {"InstantCameraTransitions", "Use instant camera transitions"},
            {"HereBeDragons", "Display 'Here Be Dragons' dialog at start up (pre-releases only)"},
            {"wPosX", "Main window X pos"},
            {"wPosY", "Main window Y pos"},
            {"wPosSX", "Settings window X pos"},
            {"wPosSY", "Settings window Y pos"},
            {"LogNodeSaving", "Show AGOS' node saving in the debug log"},
            {"AllowEE", "Allow AGOS' Easter Eggs to activate"},
            {"LockInputsOnGUIOpen", "Enable anti-clickthrough input locks while AGOS' GUI is open"},
            {"SilenceWhenUIHidden", "Don't show notifications when the game's GUI is hidden"},
            {"UseStockToolbar", "Use the stock game's toolbar (recommended)"},
            {"MaxGroupTimeDelay", "Maximum time (in seconds) an action group can be delayed for"},
            {"AddAGOSKerbals", "Add AGOS-related Kerbals to the applicants list"},
            {"TacosAllDayErrDay", "Always use the 'shimmyTaco' AGOS button image"},
            {"AGOSGroupsLast", "Show AGOS' custom groups last in the group list in the group config window"},
            {"EnableDebugOptions", "Display debug options within AGOS - MAY BREAK YOUR GAME! USE AT YOUR OWN RISK!"},
            {"DEBUGForceSpecialOccasion", "DEBUG: Forces special occasion events to fire"}

        };
        private bool lastAGOSKSetting;
        private Dictionary<string, ProtoCrewMember.RosterStatus> failedKerbalRemovals;

        public AGOSSettings(string path) {
            this.configPath = path;

            this.SETTINGS_MAP = new Dictionary<string, object> {
                {"InstantCameraTransitions", true},
                {"HereBeDragons", true},
                {"wPosX", 0f},
                {"wPosY", 0f},
                {"wPosSX", 0f},
                {"wPosSY", 0f},
                {"wPosGX", 0f},
                {"wPosGY", 0f},
                {"LogNodeSaving", false},
                {"AllowEE", true},
                {"LockInputsOnGUIOpen", true},
                {"SilenceWhenUIHidden", true},
                {"UseStockToolbar", true},
                {"MaxGroupTimeDelay", 10f},
                {"AddAGOSKerbals", true},
                {"TacosAllDayErrDay", false},
                {"AGOSGroupsLast", false},
                {"EnableDebugOptions", false},
                {"DEBUGForceSpecialOccasion", false}
            };

        }

        public void copyTo(Dictionary<string, object> target)
        {
            if (target == null)
                target = new Dictionary<string, object>();
            foreach (string s in this.SETTINGS_MAP.Keys)
                target.Add(s, this.SETTINGS_MAP[s]);
        }

        public void setTo(Dictionary<string, object> newSettings)
        {
            foreach (string s in newSettings.Keys)
                this.SETTINGS_MAP[s] = newSettings[s];
        }

        public Dictionary<string, object> getCopy()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            this.copyTo(ret);
            return ret;
        }

        public void set(string s, object v)
        {
            if (this.SETTINGS_MAP.ContainsKey(s))
                this.SETTINGS_MAP[s] = v;
            else
                Logger.LogWarning("Something attempted to write undefined setting '{0}' to '{1}'!", s, v);
        }

        public T get<T>(string setting)
        {
            if (this.SETTINGS_MAP.ContainsKey(setting))
            {
                if (this.SETTINGS_MAP[setting] is T)
                    return (T)this.SETTINGS_MAP[setting];
                else
                {
                    try
                    {
                        return (T)Convert.ChangeType(this.SETTINGS_MAP[setting], typeof(T));
                    }
                    catch (InvalidCastException)
                    {
                        return default(T);
                    }
                }
            }
            Logger.LogWarning("Attempted to read invalid setting '{0}'!", setting);
            return default(T);
        }

        public string get(string setting)
        {
            if (this.SETTINGS_MAP.ContainsKey(setting))
                return this.SETTINGS_MAP[setting].ToString();
            Logger.LogWarning("Attempted to read invalid setting '{0}'!", setting);
            return null;
        }

        public void load()
        {

            Logger.Log("AGOS is loading settings");
            ConfigNode node = ConfigNode.Load(this.configPath);
            if (node == null || node.CountValues == 0) { Logger.Log("No settings to load!"); return; }

            Dictionary<string, object> _new = new Dictionary<string, object>();

            List<string> keys = new List<String>(this.SETTINGS_MAP.Keys);

            foreach (string s in keys)
                if (node.HasValue(s))
                    _new.Add(s, node.GetValue(s));
            this.setTo(_new);
            Logger.Log("Done loading settings!");

        }

        public void save() 
        {

            Logger.Log("AGOS is saving config...");
            ConfigNode node = new ConfigNode();
            foreach (string s in this.SETTINGS_MAP.Keys)
                node.AddValue(s, this.SETTINGS_MAP[s]);
            node.Save(this.configPath);
            if (get<bool>("LogNodeSaving"))
                Logger.Log("{0}", node.ToString());
            Logger.Log("Done saving settings!");

        }

        public void toggleGUI() 
        {

            if (guiVisible)
            {
                RenderingManager.RemoveFromPostDrawQueue(AGOSMain.AGOS_SETTINGS_GUI_WINDOW_ID, OnDraw);
                this.guiVisible = false;
                if (!AGOSMain.ToolbarManager.Instance.using000Toolbar)
                    AGOSMain.ToolbarManager.Instance.agosButton.SetFalse(false);
                if (this.lastAGOSKSetting && !get<bool>("AddAGOSKerbals"))
                    RenderingManager.AddToPostDrawQueue(this.otherWinID = AGOSMain.AGOS_SETTINGS_CONFIRM_GUI_WINDOW_ID, OnDrawOther);
                else if (!this.lastAGOSKSetting && get<bool>("AddAGOSKerbals"))
                    AGOSMain.Instance.addAGOSKerbals();
            }
            else
            {
                this.lastAGOSKSetting = get<bool>("AddAGOSKerbals");
                this.guiVisible = true;
                _winPos.x = get<float>("wPosSX");
                _winPos.y = get<float>("wPosSY");
                RenderingManager.AddToPostDrawQueue(AGOSMain.AGOS_SETTINGS_GUI_WINDOW_ID, OnDraw);
            }

        }

        public void OnDrawOther()
        {

            if (!AGOSMain.Instance.hasSetupStyles)
            {
                AGOSMain.Instance.setUpStyles();
                GUISkin skin = AGOSUtils.getBestAvailableSkin();
                _windowStyle = new GUIStyle(skin.window);
            }

            _winPosOther = GUILayout.Window(this.otherWinID, _winPosOther, OnWindow, "AGOS: User Input", _windowStyle);
            if (_winPosOther.x == 0 && _winPosOther.y == 0)
            {
                _winPosOther.x = (Screen.width/* + _winPosOther.width*/ / 2);
                _winPosOther.y = (Screen.height/* + _winPosOther.height*/ / 2);
            }
        }

        public void OnDraw()
        {

            if (!AGOSMain.Instance.hasSetupStyles)
            {
                AGOSMain.Instance.setUpStyles();
                GUISkin skin = AGOSUtils.getBestAvailableSkin();
                _windowStyle = new GUIStyle(skin.window);
            }

            _winPos = GUILayout.Window(AGOSMain.AGOS_SETTINGS_GUI_WINDOW_ID, _winPos, OnWindow, "AGOS: Settings", _windowStyle);
        }

        public void OnWindow(int id)
        {

            // Did someone say multi-purpose GUIs?!
            if (id == AGOSMain.AGOS_SETTINGS_GUI_WINDOW_ID)
            {

                GUILayout.BeginVertical(GUILayout.MinWidth(300f), GUILayout.MaxWidth(300f));


                List<string> keys = new List<string>(this.SETTINGS_MAP.Keys);

                foreach (string s in keys)
                {
                    if (s == null) { Logger.LogError("Settings string is null."); continue; }
                    if (s.StartsWith("wPos")) // "Uneditable" settings
                        continue;

                    if (s.Equals("UseStockToolbar") && !_000Toolbar.ToolbarManager.ToolbarAvailable) // Don't display the toolbar option if the player doesn't have 000toolbar installed
                        continue;
#if !DEBUG
                if (s.Equals("HereBeDragons")) // Hide HBD dialog option if this is NOT a debug build
                    continue;
#endif

                    if (s.StartsWith("Taco") && !get<bool>("AllowEE")) // Don't show the shimmyTaco option if AGOS' EEs are disabled.
                        continue;

                    if (s.Equals("AddAGOSKerbals") && HighLogic.CurrentGame.Mode == Game.Modes.CAREER) // Don't show Kerbal options on Career saves.
                        continue;

                    if (s.StartsWith("DEBUG") && (!AGOSDebug.isDebugBuild() && !get<bool>("EnableDebugOptions"))) // Skip debug options if this isn't a debug build or debug options are disabled
                        continue;

                    bool __;
                    if (Boolean.TryParse(get(s), out __))
                    {
                        set(s, GUILayout.Toggle(get<bool>(s), configPrettyNames[s], AGOSMain.Instance._toggleStyle));
                    }
                    else
                    {
                        continue; // Do not display non-boolean settings
                        /*GUILayout.BeginHorizontal(GUILayout.MinWidth(300f), GUILayout.MaxWidth(300f));

                        GUILayout.Label(configPrettyNames[s] + ": ", AGOSMain.Instance._labelStyle, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(200f));
                        this.SETTINGS_MAP[s] = GUILayout.TextField(get<string>(s), AGOSMain.Instance._textFieldStyle);

                        GUILayout.EndHorizontal();*/
                    }

                }

                if (GUILayout.Button("Reset GUI positions", AGOSMain.Instance._buttonStyle))
                {
                    this.toggleGUI();
                    foreach (string s in this.SETTINGS_MAP.Keys.ToList())
                    {
                        if (s.StartsWith("wPos"))
                            set(s, 0f);
                    }
                    this.toggleGUI();
                }

                if (GUILayout.Button("Save & Close", AGOSMain.Instance._buttonStyle))
                {
                    set("wPosSX", _winPos.x);
                    set("wPosSY", _winPos.y);
                    this.save();
                    this.toggleGUI();
                    AGOSMain.ToolbarManager.Instance.switchToolbarsIfNeeded();
                }

                GUILayout.EndVertical();

                GUI.DragWindow();
            }
            else if (id == AGOSMain.AGOS_SETTINGS_CONFIRM_GUI_WINDOW_ID)
            {
                GUILayout.BeginVertical(GUILayout.MinWidth(250f));

                string dialogMessage = "Do you want to remove AGOS-related Kerbals from the roster?\n\nOnly Kerbals that are not assigned, missing or dead can be removed.";
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    dialogMessage += "\n\nYou will NOT be refunded hirings costs for any of these Kerbals!";
                GUILayout.Label(dialogMessage);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Yes", AGOSMain.Instance._buttonStyle))
                {
                    this.failedKerbalRemovals = AGOSMain.Instance.removeAGOSKerbals();
                    if (failedKerbalRemovals.Count > 0)
                    {
                        RenderingManager.RemoveFromPostDrawQueue(AGOSMain.AGOS_SETTINGS_CONFIRM_GUI_WINDOW_ID, OnDrawOther);
                        RenderingManager.AddToPostDrawQueue(this.otherWinID = AGOSMain.AGOS_SETTINGS_ERROR_GUI_WINDOW_ID, OnDrawOther);
                    }
                }
                if (GUILayout.Button("No", AGOSMain.Instance._buttonStyle))
                {
                    RenderingManager.RemoveFromPostDrawQueue(AGOSMain.AGOS_SETTINGS_CONFIRM_GUI_WINDOW_ID, OnDrawOther);
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

            }
            else if (id == AGOSMain.AGOS_SETTINGS_ERROR_GUI_WINDOW_ID)
            {
                GUILayout.BeginVertical(GUILayout.MinWidth(250f));
                foreach (string s in this.failedKerbalRemovals.Keys)
                {
                    string kerbalName = s;
                    ProtoCrewMember.RosterStatus kerbalStatus = this.failedKerbalRemovals[s];
                    string error = s + " could not be removed because";
                    if (kerbalStatus == ProtoCrewMember.RosterStatus.Assigned)
                        error += " they are on a mission.";
                    else if (kerbalStatus == ProtoCrewMember.RosterStatus.Missing)
                        error += " they are missing.";
                    else
                        error += " they are dead.";
                    GUILayout.Label(error, AGOSMain.Instance._labelStyle);
                }
                if (GUILayout.Button("Close", AGOSMain.Instance._buttonStyle))
                    RenderingManager.RemoveFromPostDrawQueue(AGOSMain.AGOS_SETTINGS_ERROR_GUI_WINDOW_ID, OnDrawOther);
                GUILayout.EndVertical();
            }

        }

    }
}
