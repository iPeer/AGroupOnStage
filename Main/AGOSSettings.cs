using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{
    public class AGOSSettings : MonoBehaviour
    {

        public bool guiVisible = false;

        public string configPath;
        public Texture buttonTex = (Texture)AGOSToolbarManager.createButtonTexture(AGOSToolbarManager.ButtonType.SETTINGS);
        public GUIStyle _scrollStyle;
        private GUISkin guiSkin;
        /*public bool INSTANT_CAMERA_TRANSITIONS = true;
        public bool SHOW_DRAGONS_DIALOG = true;
        public float WIN_POS_X = 0, WIN_POS_Y = 0f;
        public bool LOG_NODE_SAVE = false;
        public bool ALLOW_EE = true;*/

        private Dictionary<string, object> SETTINGS_MAP;
        private Rect _winPos = new Rect();
        private bool hasChanged = false;
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
            {"AddAGOSKerbals", "Add AGOS-related Kerbals to the roster list"},
            {"TacosAllDayErrDay", "Always use the 'shimmyTaco' AGOS button image"},
            {"AGOSGroupsLast", "Show AGOS' custom groups last in the group list in the group config window"},
            {"EnableDebugOptions", "Display debug options within AGOS - MAY BREAK YOUR GAME! USE AT YOUR OWN RISK!"},
            {"DEBUGForceSpecialOccasion", "DEBUG: Force special occasion events to fire"},
            {"LogControlLocks", "Log all control locks or unlocks. Can be spammy."},
            {"DisplaySpecialOccasions", "Show a notification in AGOS' main window when the day is a special occasion"},
            {"ShowUndoWarning", "Show a warning about part configurations when undo or redoing craft modifications in the editor"},
            {"LockRemovalDelay", "The delay between AGOS' last GUI closing and the removal of the control locks"},
            {"TacoButtonChance", "1-in-N chance of the Taco AGOS button being used"},
            {"FineControlsEEChance", "1-in-N chance of the Fine Controls easter egg firing"},
            {"UnloadUnusedAssets", "Unload AGOS assets that are in memory but don't need to be on game load"}

        };
        private bool lastAGOSKSetting;
        private Vector2 scrollPos = Vector2.zero;

        public AGOSSettings(string path)
        {
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
                {"AddAGOSKerbals", false},
                {"TacosAllDayErrDay", false},
                {"AGOSGroupsLast", false},
                {"EnableDebugOptions", false},
                {"DEBUGForceSpecialOccasion", false},
                {"LogControlLocks", false},
                {"DisplaySpecialOccasions", true},
                {"ShowUndoWarning", true},
                {"LockRemovalDelay", 250d},
                {"TacoButtonChance", 5},
                {"FineControlsEEChance", 10},
                {"UnloadUnusedAssets", true}
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
            this.hasChanged = true;
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
            {
                if (!get(s).Equals(v.ToString()))
                    this.hasChanged = true;
                this.SETTINGS_MAP[s] = v;
            }
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
            if (!this.hasChanged)
                return;
            this.hasChanged = false;
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
                scrollPos = Vector2.zero;
                this.guiVisible = false;
                if (!AGOSToolbarManager.using000Toolbar)
                    AGOSToolbarManager.agosButton.SetFalse(false);
                if (this.lastAGOSKSetting && !get<bool>("AddAGOSKerbals"))
                {
                    DialogOption[] options = new DialogOption[] { 
                        new DialogOption("Yes", () => removeKerbalsClick(0)), 
                        new DialogOption("No", () => removeKerbalsClick(1)) 
                    };

                    MultiOptionDialog mod = new MultiOptionDialog("Do you want to remove AGOS related Kerbals from your game? Only Kerbals who have a status of \"Available\" will be able to be removed.", "AGroupOnStage", HighLogic.Skin, options);
                    PopupDialog.SpawnPopupDialog(mod, false, HighLogic.Skin);
                }
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

        private void removeKerbalsClick(int opt)
        {
            if (opt == 0)
            {
                AGOSUtils.runVoidMethodDelayed(doKerbalRemoval, 250d);
            }
        }

        private void doKerbalRemoval()
        {
            Dictionary<string, ProtoCrewMember.RosterStatus> fails = AGOSMain.Instance.removeAGOSKerbals();
            int removed = AGOSMain.agosKerbalNames.Count - fails.Count;
            StringBuilder message = new StringBuilder(String.Format("{0} AGOS related Kerbal(s) have been removed.", removed));
            if (fails.Count > 0)
            {
                message.AppendLine();
                message.AppendLine(String.Format("{0} Kerbals were not removed:", fails.Count));
                message.AppendLine();
                message.AppendLine();
                foreach (string s in fails.Keys)
                {
                    ProtoCrewMember.RosterStatus status = fails[s];
                    string failMessage = "They are dead.";
                    if (status == ProtoCrewMember.RosterStatus.Assigned)
                        failMessage = "They are on a mission.";
                    else if (status == ProtoCrewMember.RosterStatus.Missing)
                        failMessage = "They are missing.";
                    /* else 
                        leave it alone because it, by default, says they're dead.*/

                    message.AppendLine(String.Format("{0} - {1}", s, failMessage));

                }

            }
            MultiOptionDialog mod = new MultiOptionDialog(message.ToString(), "AGroupOnStage", HighLogic.Skin, new DialogOption("Ok", () => removeKerbalsClick(2)));
            PopupDialog.SpawnPopupDialog(mod, false, HighLogic.Skin);
        }

        public void OnDraw()
        {
            // TODO: Fix GUI scaling for long option descriptions
            if (!AGOSMain.Instance.hasSetupStyles)
            {
                guiSkin = AGOSMain.Instance.setUpStyles();
                _scrollStyle = new GUIStyle(guiSkin.scrollView);
                _scrollStyle.stretchWidth = true;
            }
            _winPos = GUILayout.Window(AGOSMain.AGOS_SETTINGS_GUI_WINDOW_ID, _winPos, OnWindow, "AGOS: Settings", AGOSMain.Instance._windowStyle);
        }

        public void OnWindow(int id)
        {


            GUILayout.BeginVertical(GUILayout.MinWidth(300f), GUILayout.MaxWidth(300f));


            List<string> keys = new List<string>(this.SETTINGS_MAP.Keys);

            scrollPos = GUILayout.BeginScrollView(scrollPos, _scrollStyle, GUILayout.MaxHeight(300f), GUILayout.MinHeight(300f), GUILayout.MinWidth(500f));

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
                    float min, max;
                    guiSkin.toggle.CalcMinMaxWidth(new GUIContent(configPrettyNames[s]), out min, out max);
                    if ((max + 5f) > _winPos.width)
                        _winPos.width = (max + 5f);
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

            GUILayout.EndScrollView();

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

            if (GUILayout.Button("Reset settings to defaults", AGOSMain.Instance._buttonStyle))
            {
                DialogOption[] options = new DialogOption[] { 
                        new DialogOption("Yes", () => resetSettingsCallback(1)), 
                        new DialogOption("No", () => resetSettingsCallback(0)) 
                    };
                MultiOptionDialog mod = new MultiOptionDialog("Are you sure you want to reset your AGOS settings? This will reset all AGOS' settings back to their defaults. This process cannot be undone!", "AGroupOnStage", HighLogic.Skin, options);
                PopupDialog.SpawnPopupDialog(mod, false, HighLogic.Skin);
            }

            if (GUILayout.Button("Save & Close", AGOSMain.Instance._buttonStyle))
            {
                set("wPosSX", _winPos.x);
                set("wPosSY", _winPos.y);
                this.save();
                this.toggleGUI();
                AGOSToolbarManager.switchToolbarsIfNeeded();
            }

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        private void resetSettingsCallback(int opt)
        {
            if (opt == 1)
            {
                AGOSMain.ResetSettings();
            }
        }

        public void removeFile()
        {
            try
            {
                File.Delete(this.configPath);
            }
            catch (Exception e)
            {
                Logger.LogError("Couldn't delete config file: {0}", e.ToString());
            }
        }

    }
}
