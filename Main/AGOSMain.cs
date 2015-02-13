using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class AGOSMain : MonoBehaviour
    {


        static AGOSMain Instance { get; private set; }

        public void Start()
        {
            Logger.Log("AGOS Main Start()");
            Instance = this;
        }

        private void setupToolbarButton()
        {

        }

        private void removeToolbarButton()
        {

        }

        private void toggleGUI()
        {

        }

        private void OnGUI() { }
        private void OnWindow(int windowID) { }

    }
}
