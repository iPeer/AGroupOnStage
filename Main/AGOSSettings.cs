using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.Main
{
    public class AGOSSettings
    {

        public string configPath;
        /*public bool INSTANT_CAMERA_TRANSITIONS = true;
        public bool SHOW_DRAGONS_DIALOG = true;
        public float WIN_POS_X = 0, WIN_POS_Y = 0f;
        public bool LOG_NODE_SAVE = false;
        public bool ALLOW_EE = true;*/

        private Dictionary<string, object> SETTINGS_MAP;

        public AGOSSettings(string path) {
            this.configPath = path;

            this.SETTINGS_MAP = new Dictionary<string, object> {
                {"InstantCameraTransitions", false},
                {"HereBeDragons", true},
                {"wPosX", 0f},
                {"wPosY", 0f},
                {"LogNodeSaving", false},
                {"AllowEE", true}
            };

        }

        public void copyTo(Dictionary<string, object> target)
        {
            if (target == null)
                target = new Dictionary<string, object>();
            foreach (string s in this.SETTINGS_MAP.Keys)
                target.Add(s, this.SETTINGS_MAP[s]);
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
                return (T)this.SETTINGS_MAP[setting];
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
            foreach (string s in this.SETTINGS_MAP.Keys)
                if (node.HasValue(s))
                    this.SETTINGS_MAP[s] = node.GetValues(s);
            Logger.Log("Done loading settings!");
        
        }

        public void save() 
        {

            Logger.Log("AGOS is saving config...");
            ConfigNode node = new ConfigNode();
            foreach (string s in this.SETTINGS_MAP.Keys)
                node.AddValue(s, this.SETTINGS_MAP[s]);
            Logger.Log("Done saving settings!");

        }

        /*public void load()
        {

            ConfigNode node = ConfigNode.Load(configPath);
            if (node == null || node.CountValues == 0) { return; }
            INSTANT_CAMERA_TRANSITIONS = Convert.ToBoolean(node.GetValue("InstantCameraTransitions"));
            WIN_POS_X = Convert.ToSingle(node.GetValue("wPosX"));
            WIN_POS_Y = Convert.ToSingle(node.GetValue("wPosY"));
            SHOW_DRAGONS_DIALOG = Convert.ToBoolean(node.GetValue("HereBeDragons"));
            LOG_NODE_SAVE = Convert.ToBoolean(node.GetValue("LogNodeSaving"));
            ALLOW_EE = Convert.ToBoolean(node.GetValue("AllowEE"));
        }

        public void save()
        {
            Logger.Log("Saving AGOS config");
            ConfigNode node = new ConfigNode();
            node.AddValue("InstantCameraTransitions", INSTANT_CAMERA_TRANSITIONS);
            node.AddValue("wPosX", WIN_POS_X);
            node.AddValue("wPosY", WIN_POS_Y);
            node.AddValue("HereBeDragons", SHOW_DRAGONS_DIALOG);
            node.AddValue("LogNodeSaving", LOG_NODE_SAVE);
            node.AddValue("AllowEE", ALLOW_EE);
            node.Save(configPath);
        }*/

    }
}
