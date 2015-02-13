using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class AGOSEditor : MonoBehaviour
    {
        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSEditor.Start()");
            GameEvents.onGameStateSave.Add(onGameStateSave);
            GameEvents.onGameStateLoad.Add(onGameStateLoad);
        }

        private void onGameStateLoad(ConfigNode data)
        {
            Logger.Log("Game load called");
        }

        private void onGameStateSave(ConfigNode data)
        {
            Logger.Log("Game save called");
        }

        public void OnSave(ConfigNode c) { AGOSMain.Instance.OnSave(c); }
        public void OnLoad(ConfigNode c) { AGOSMain.Instance.OnLoad(c); }
    }
}
