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
        private GUIStyle _windowStyle;
        private Dictionary<string, string> configPrettyNames = new Dictionary<string, string>
        {
            {"InstantCameraTransitions", "Use instant camera transitions"},
            {"HereBeDragons", "Display 'Here Be Dragons' dialog at start up (pre-releases only)"},
            {"wPosX", "Main window X pos"},
            {"wPosY", "Main window Y pos"},
            {"wPosSX", "Settings window X pos"},
            {"wPosSY", "Settings window Y pos"},
            {"LogNodeSaving", "Show AGOS' node saving in the debug log"},
            {"AllowEE", "Allow Easter Eggs to activate"},
            {"LockInputsOnGUIOpen", "Enable anti-clickthrough input locks while AGOS' GUI is open"},
            {"SilenceWhenUIHidden", "Don't show notifications when the game's GUI is hidden"}

        };

        public AGOSSettings(string path) {
            this.configPath = path;

            this.SETTINGS_MAP = new Dictionary<string, object> {
                {"InstantCameraTransitions", true},
                {"HereBeDragons", true},
                {"wPosX", 0f},
                {"wPosY", 0f},
                {"wPosSX", 0f},
                {"wPosSY", 0f},
                {"LogNodeSaving", false},
                {"AllowEE", true},
                {"LockInputsOnGUIOpen", true},
                {"SilenceWhenUIHidden", true}
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
                RenderingManager.RemoveFromPostDrawQueue(AGOSMain.AGOS_GUI_WINDOW_ID + 1, OnDraw);
                this.guiVisible = false;
                AGOSMain.Instance.agosButton.SetFalse(false);
            }
            else
            {
                this.guiVisible = true;
                _winPos.x = get<float>("wPosSX");
                _winPos.y = get<float>("wPosSY");
                RenderingManager.AddToPostDrawQueue(AGOSMain.AGOS_GUI_WINDOW_ID + 1, OnDraw);
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

            _winPos = GUILayout.Window(AGOSMain.AGOS_GUI_WINDOW_ID + 1, _winPos, OnWindow, "AGOS: Settings", _windowStyle);
        }

        public void OnWindow(int id)
        {

            GUILayout.BeginVertical(GUILayout.MinWidth(300f), GUILayout.MaxWidth(300f));


            List<string> keys = new List<string>(this.SETTINGS_MAP.Keys);

            foreach (string s in keys)
            {
                if (s.StartsWith("wPos"))
                    continue;
#if !DEBUG
                if (s.Equals("HereBeDragons"))
                    continue;
#endif
                bool __;
                if (Boolean.TryParse(this.SETTINGS_MAP[s].ToString(), out __))
                {
                    this.SETTINGS_MAP[s] = GUILayout.Toggle(get<bool>(s), configPrettyNames[s], AGOSMain.Instance._toggleStyle);
                }
                else
                {
                    GUILayout.BeginHorizontal(GUILayout.MinWidth(300f), GUILayout.MaxWidth(300f));

                    GUILayout.Label(configPrettyNames[s]+": ", AGOSMain.Instance._labelStyle, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(200f));
                    this.SETTINGS_MAP[s] = GUILayout.TextField(get<string>(s), AGOSMain.Instance._textFieldStyle);

                    GUILayout.EndHorizontal();
                }

            }

            if (GUILayout.Button("Reset GUI positions"))
            {
                this.toggleGUI();
                set("wPosX", 0f);
                set("wPosY", 0f);
                set("wPosSX", 0f);
                set("wPosSY", 0f);
                this.toggleGUI();
            }

            if (GUILayout.Button("Save & Close"))
            {
                set("wPosSX", _winPos.x);
                set("wPosSY", _winPos.y);
                this.toggleGUI();
                this.save();
            }

            GUILayout.EndVertical();

            GUI.DragWindow();

        }

    }
}
