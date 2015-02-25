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

        private static readonly int MAX_TICK_COUNT = 30; // 30 = 1 in-game second
        private static int CURRENT_TICK_COUNT = 0;
        bool stageLockScheduled = false;

        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSFlight.Start()");
            GameEvents.onStageActivate.Add(onStageActivate);
            GameEvents.onStageSeparation.Add(onStageSeparation);
            //GameEvents.onVesselLoaded.Add(AGOSMain.Instance.onVesselLoaded);
            //AGOSUtils.resetActionGroupConfig();
        }

        private void onVesselLoaded(Vessel data)
        {
            AGOSMain.Instance.findHomesForPartLockedGroups(data);
        }

        private void onStageActivate(int stage)
        {
            Logger.Log("OnStageActivate: " + stage);
        }

        private void onStageSeparation(EventReport e)
        {

            Logger.Log("OnStageSeparation: " + e.stage);
        }

        /* 
         * We can't activate stageLock immediately after staging for some reason (it doesn't work), 
         * so we have to wait a second or so, we use FixedUpdate for this so that
         * this delay is consistent and does not fire prematurely
         */
        public void FixedUpdate()
        {
            if (stageLockScheduled)
            {
                CURRENT_TICK_COUNT++;
                if (CURRENT_TICK_COUNT >= MAX_TICK_COUNT)
                {
                    CURRENT_TICK_COUNT = 0;
                    stageLockScheduled = false;
                    FlightInputHandler.fetch.stageLock = !FlightInputHandler.fetch.stageLock;
                }
            }

        }

        public void toggleFineControls()
        {
            FlightInputHandler.fetch.precisionMode = !FlightInputHandler.fetch.precisionMode;
            foreach (var r in FlightInputHandler.fetch.inputGaugeRenderers)
            {
                if (FlightInputHandler.fetch.precisionMode)
                    r.material.color = new Color(0.255f, 0.992f, 0.996f);
                else
                    r.material.color = new Color(0.976f, 0.451f, 0.024f);
            }

        }
    }
}
