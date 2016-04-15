using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using AGroupOnStage.Logging;

namespace AGroupOnStage.Main
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class AGOSSpaceCentre : MonoBehaviour
    {

        public static AGOSSpaceCentre Instance { get; protected set; }

        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSSpaceCentre.Start()");
            if (Instance == null)
                Instance = this;
        }

        private void OnGUI()
        {
            AGOSUtils.renderVisibleGUIs();
        }

    }
}
