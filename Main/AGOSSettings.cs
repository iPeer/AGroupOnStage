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
        public bool INSTANT_CAMERA_TRANSITIONS = true;
        public bool SHOW_DRAGONS_DIALOG = true;
        public float WIN_POS_X = 0, WIN_POS_Y = 0f;

        public AGOSSettings(string path) {
            this.configPath = path;
        }

        public void load()
        {

            ConfigNode node = ConfigNode.Load(configPath);
            if (node == null || node.CountValues == 0) { return; }
            INSTANT_CAMERA_TRANSITIONS = Convert.ToBoolean(node.GetValue("InstantCameraTransitions"));
            WIN_POS_X = Convert.ToSingle(node.GetValue("wPosX"));
            WIN_POS_Y = Convert.ToSingle(node.GetValue("wPosY"));
            SHOW_DRAGONS_DIALOG = Convert.ToBoolean(node.GetValue("HereBeDragons"));
        }

        public void save()
        {
            Logger.Log("Saving AGOS config");
            ConfigNode node = new ConfigNode();
            node.AddValue("InstantCameraTransitions", INSTANT_CAMERA_TRANSITIONS);
            node.AddValue("wPosX", WIN_POS_X);
            node.AddValue("wPosY", WIN_POS_Y);
            node.AddValue("HereBeDragons", SHOW_DRAGONS_DIALOG);
            node.Save(configPath);
        }

    }
}
