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
        public float WIN_POS_X = 0, WIN_POS_Y = 0f;

        public AGOSSettings(string path) {
            this.configPath = path;
        }

        public void load()
        {

            ConfigNode node = ConfigNode.Load(configPath);
            if (node == null || node.CountNodes == 0 || node.CountValues == 0) { return; }
            node = node.GetNode("AGOS_CONFIG");
            INSTANT_CAMERA_TRANSITIONS = Convert.ToBoolean(node.GetValue("InstantCameraTransitions"));
            WIN_POS_X = Convert.ToSingle(node.GetValue("wPosX"));
            WIN_POS_Y = Convert.ToSingle(node.GetValue("wPosY"));

        }

        public void save()
        {
            Logger.Log("Saving AGOS config");
            ConfigNode node = new ConfigNode();
            node.AddNode("AGOS_CONFIG");
            ConfigNode _node = node = node.GetNode("AGOS_CONFIG");
            _node.AddValue("InstantCameraTransitions", INSTANT_CAMERA_TRANSITIONS);
            _node.AddValue("wPosX", WIN_POS_X);
            _node.AddValue("wPosY", WIN_POS_Y);
            node.Save(configPath);
        }

    }
}
