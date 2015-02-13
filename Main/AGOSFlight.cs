using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class AGOSFlight : MonoBehaviour
    {

        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSFlight.Start()");
            GameEvents.onStageActivate.Add(OnStageActivate);
            GameEvents.onStageSeparation.Add(OnStageSeparation);
            Logger.Log("{0}", FlightGlobals.ActiveVessel.id);
        }

        private void OnStageActivate(int stage)
        {
            Logger.Log("OnStageActivate: " + stage);
        }

        private void OnStageSeparation(EventReport e)
        {

            Logger.Log("OnStageSeparation: "+e.stage);
        }

    }
}
