using AGroupOnStage.ActionGroups;
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

        private static readonly int MAX_TICK_COUNT = 60; // 30 = 1 in-game second
        private static int CURRENT_TICK_COUNT = 0;
        bool stageLockScheduled = false;
        private bool handledByActivate = false;

        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSFlight.Start()");
            GameEvents.onStageActivate.Add(onStageActivate);
            GameEvents.onStageSeparation.Add(onStageSeparation);
            GameEvents.onFlightReady.Add(onFlightReady);
            //GameEvents.onVesselLoaded.Add(AGOSMain.Instance.onVesselLoaded);
            //AGOSUtils.resetActionGroupConfig();
        }

        private void onFlightReady()
        {
            AGOSMain.Instance.findHomesForPartLockedGroups(FlightGlobals.fetch.activeVessel);
        }

        private void onVesselLoaded(Vessel data)
        {
            AGOSMain.Instance.findHomesForPartLockedGroups(data);
        }

        private void onStageActivate(int stage)
        {
            handledByActivate = true;
            Logger.Log("OnStageActivate: " + stage);
            activateGroupsForStage(stage);
            handledByActivate = false;
        }

        private void onStageSeparation(EventReport e)
        {
            if (handledByActivate)
            {
                Logger.Log("Staging already handled by onStageActivate()");
                return;
            }
            int stage = e.stage - 1; // We take 1 to get the "real" stage number
            activateGroupsForStage(stage);
            Logger.Log("OnStageSeparation: " + stage);
        }

        public void activateGroupsForStage(int s)
        {
            List<IActionGroup> toFire = new List<IActionGroup>();
            toFire.AddRange(AGOSMain.Instance.actionGroups.FindAll(a => a.Stages != null && a.Stages.Length > 0 && a.Stages.Contains(s))); // Regular action groups
            toFire.AddRange(AGOSMain.Instance.actionGroups.FindAll(a => a.isPartLocked && a.linkedPart.inverseStage == s)); // Part locked groups
            //List<IActionGroup> toFire = AGOSMain.Instance.actionGroups.FindAll(a => (a.Stages.Length > 0 && a.Stages.Contains(s)) || (a.isPartLocked && a.linkedPart.inverseStage == s));
            Logger.Log("{0} group(s) to fire", toFire.Count);
            foreach (IActionGroup ag in toFire) 
            {
                if (ag.GetType() == typeof(StageLockActionGroup))
                {
                    stageLockScheduled = true;
                    Logger.Log("A stage lock has been scheduled");
                }
                else if (ag.GetType() == typeof(ThrottleControlActionGroup))
                {
                    FlightInputHandler.state.mainThrottle = ag.ThrottleLevel;
                    Logger.Log("Set throttle to {0:P0}", ag.ThrottleLevel);
                }
                else if (ag.GetType() == typeof(CameraControlActionGroup))
                {
                    FlightCamera.SetModeImmediate(ag.cameraMode);
                    Logger.Log("Camera mode set to {0}", ag.cameraMode.ToString());
                }
                else if (ag.GetType() == typeof(FineControlActionGroup))
                {
                    toggleFineControls();
                    Logger.Log("Fine controls toggled");
                }
                else
                {
                    if (AGOSMain.Instance.useAGXConfig) { }
                    else
                    {
                        Logger.Log("{0}", ag.Group);
                        KSPActionGroup g = AGOSMain.Instance.stockAGMap[ag.Group];
                        Logger.Log("Firing action group {0} ({1})", ag.Group, AGOSMain.Instance.actionGroupList[ag.Group]);
                        FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(g);
                    }
                }
            }
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
