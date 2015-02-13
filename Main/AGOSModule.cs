using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.Main
{
    [KSPModule("AGOS Controller")]
    public class AGOSModule : PartModule
    {

        private bool isRoot = false;

        public override void OnAwake()
        {
            if (!AGOSUtils.isLoadedSceneOneOf(GameScenes.FLIGHT, GameScenes.EDITOR)) { return; } // Invalid scene
            try // FIXME: Temp fix for exception on first part placement (InvalidOperationException) (Is this even fixable?)
            {
                AGOSModule am = AGOSMain.Instance.getMasterAGOSModule(this.part.vessel);
                if (am == null || this == am)
                    this.isRoot = true;
            }
            catch { Logger.logWarning("Catch Exception on part awake!"); this.isRoot = true; }
        }

        public override void OnSave(ConfigNode node)
        {
            node.AddValue("isRoot", isRoot);
            if (!this.isRoot) { return; } // Only the root module can save
            node.AddValue("testData", "sometestdata");
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!AGOSUtils.isLoadedSceneOneOf(GameScenes.FLIGHT, GameScenes.EDITOR)) { return; } // Invalid scene
            if (node.CountValues == 0 && node.CountNodes == 0) { return; } // No config to load
            this.isRoot = Convert.ToBoolean(node.GetValue("isRoot"));
            if (!this.isRoot) { return; } // Only the root module can load
        }

        public override string GetInfo()
        {
            return "Able to fire action groups on stage.";
        }
    }
}
