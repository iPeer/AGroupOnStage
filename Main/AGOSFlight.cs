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

        private static readonly int MAX_TICK_COUNT = 90; // 30 = 1 in-game second
        private static int CURRENT_TICK_COUNT = 0;
        bool stageLockScheduled = false;
        private bool handledBySeparation = false;
        private bool processingStageEvent = false;
        private Vessel lastVessel;

        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSFlight.Start()");
            if (AGOSMain.Instance.FlightEventsRegistered)
                Logger.LogWarning("GameEvents for Flight are already registered (harmless)");
            else
            {
                GameEvents.onStageActivate.Add(onStageActivate);
                GameEvents.onStageSeparation.Add(onStageSeparation);
                GameEvents.onFlightReady.Add(onFlightReady);
                GameEvents.onVesselChange.Add(onVesselChange);
                //GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
                //GameEvents.onVesselGoOffRails.Add(onVesselUnpack);
                AGOSMain.Instance.FlightEventsRegistered = true;
                Logger.Log("Registered for Flight related GameEvents");
            }
            //GameEvents.onVesselLoaded.Add(AGOSMain.Instance.onVesselLoaded);
            //AGOSUtils.resetActionGroupConfig();
        }

        private void onVesselChange(Vessel data)
        {
            if (data != this.lastVessel)
            {
                Logger.Log("Vessel changed");
                if (lastVessel != null && isVesselInFlight())
                {
                    Logger.Log("Player switched vessel; reverts are now invalid. Removing action group backups.");
                    AGOSMain.backupActionGroups.Clear();
                }
                this.lastVessel = data;
            }
        }

        private void onVesselUnpack(Vessel v)
        {
            Logger.Log("Vessel unpack");
            AGOSMain.Instance.restoreBackedUpActionGroups();
            AGOSMain.Instance.findHomesForPartLockedGroups(v);
        }

        private void onFlightReady()
        {
            Logger.Log("Flight ready");
            AGOSMain.Instance.restoreBackedUpActionGroups();
            AGOSMain.Instance.findHomesForPartLockedGroups(FlightGlobals.fetch.activeVessel);
            //AGOSMain.Instance.backupActionGroupList();
        }

        private void onVesselLoaded(Vessel data)
        {
            AGOSMain.Instance.findHomesForPartLockedGroups(data);
        }

        private void onStageActivate(int stage)
        {
            if (handledBySeparation)
            {
                Logger.Log("Staging already handled by onStageSeparation()");
                return;
            }
            Logger.Log("OnStageActivate: " + stage);
            activateGroupsForStage(stage);
        }

        private void onStageSeparation(EventReport e)
        {
            handledBySeparation = true;
            int stage = e.stage - 1; // We take 1 to get the "real" stage number
            //activateGroupsForStage(stage); // 2.0.6-dev1: Possible fix for AGs firing twice.
            Logger.Log("OnStageSeparation: " + stage);
            handledBySeparation = false;
        }

        public void activateGroupsForStage(int s)
        {
            if (processingStageEvent)
            {
                Logger.LogWarning("Already processing a staging event! Cannot process multiple at the same time.");
                return;
            }
            processingStageEvent = true;
            List<IActionGroup> toFire = new List<IActionGroup>();
            int stageNum = (AGOSMain.Instance.useAGXConfig && s >= 8 ? s - 7 : s);
            toFire.AddRange(AGOSMain.Instance.actionGroups.FindAll(a => a.Stages != null && a.Stages.Length > 0 && a.Stages.Contains(stageNum)/* && a.Vessel == FlightGlobals.fetch.activeVessel*/)); // Regular action groups
            toFire.AddRange(AGOSMain.Instance.actionGroups.FindAll(a => a.isPartLocked && a.linkedPart.inverseStage == stageNum/* && a.Vessel == FlightGlobals.fetch.activeVessel*/)); // Part locked groups
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
                    if (AGOSMain.Settings.INSTANT_CAMERA_TRANSITIONS)
                        FlightCamera.SetModeImmediate(ag.cameraMode);
                    else
                        FlightCamera.SetMode(ag.cameraMode);
                    Logger.Log("Camera mode set to {0}", ag.cameraMode.ToString());
                }
                else if (ag.GetType() == typeof(FineControlActionGroup))
                {
                    toggleFineControls();
                    Logger.Log("Fine controls toggled");
                }
                else
                {
                    if (AGOSMain.Instance.useAGXConfig && ag.Group >= 8) {
                        AGX.AGXInterface.AGExtToggleGroup(ag.Group - 7);
                        Logger.Log("Firing AGX Action Group #{0}", ag.Group - 7);
                    }
                    else
                    {
                        //Logger.Log("{0}", ag.Group);
                        KSPActionGroup g = AGOSMain.Instance.stockAGMap[ag.Group];
                        Logger.Log("Firing action group {0} ({1})", ag.Group, AGOSMain.Instance.actionGroupList[ag.Group]);
                        FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(g);
                    }
                }
                if (ag.isPartLocked || ag.Stages.Count(a => a < FlightGlobals.fetch.activeVessel.currentStage) == 1)
                {
                    Logger.Log("Removing action group from config as it has no more triggers");
                    AGOSMain.Instance.actionGroups.Remove(ag);
                }
            }
            processingStageEvent = false;
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
                    Logger.Log("Locking staging");
                    CURRENT_TICK_COUNT = 0;
                    stageLockScheduled = false;
                    Logger.Log("Stage lock: {0} -> {1}", FlightInputHandler.fetch.stageLock, !FlightInputHandler.fetch.stageLock);
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
            System.Random ra = new System.Random();
            ScreenMessages.PostScreenMessage((ra.Next(10) == 10 ? "fINE cONTROLS" : "Fine Controls") + " have been "+(FlightInputHandler.fetch.precisionMode ? "enabled" : "disabled")+".", 5f, ScreenMessageStyle.UPPER_CENTER);
        }

        public bool isVesselInFlight()
        {
            Vessel vessel = FlightGlobals.ActiveVessel;
            return (vessel.situation != Vessel.Situations.PRELAUNCH && vessel.situation != Vessel.Situations.LANDED && vessel.situation != Vessel.Situations.SPLASHED && vessel.GetHeightFromSurface() > 10f);
        }
    }
}
