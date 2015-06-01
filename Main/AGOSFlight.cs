using AGroupOnStage.ActionGroups;
using AGroupOnStage.Logging;
using AGroupOnStage.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading;

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
        //private List<Timer> groupTimers = new List<Timer>();
        private Dictionary<IActionGroup, DateTime> groupTimers = new Dictionary<IActionGroup, DateTime>();

        public static AGOSFlight Instance { get; protected set; }

        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSFlight.Start()");
            if (Instance == null)
                Instance = this;
            if (AGOSMain.Instance.FlightEventsRegistered)
                Logger.LogWarning("GameEvents for Flight are already registered (harmless)");
            else
            {
                GameEvents.onStageActivate.Add(onStageActivate);
                //GameEvents.onStageSeparation.Add(onStageSeparation); // 2.0.6-dev1: Possible fix for AGs firing twice.
                GameEvents.onFlightReady.Add(onFlightReady);
                GameEvents.onVesselChange.Add(onVesselChange);
                GameEvents.onVesselWillDestroy.Add(onVesselDestroy);
                GameEvents.onPartCouple.Add(OnPartCouple);
                GameEvents.onPartUndock.Add(OnPartUndock);
                //GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
                //GameEvents.onVesselGoOffRails.Add(onVesselUnpack);
                AGOSMain.Instance.FlightEventsRegistered = true;
                Logger.Log("Registered for Flight related GameEvents");
            }
            //GameEvents.onVesselLoaded.Add(AGOSMain.Instance.onVesselLoaded);
            //AGOSUtils.resetActionGroupConfig();
        }

        private void OnPartUndock(Part data)
        {
            Logger.Log("Vessel undock: {0}", data.vessel.name);
            AGOSMain.Instance.getMasterAGOSModule(data.vessel).resetFlightID(data.vessel.rootPart.flightID);
        }

        private void OnPartCouple(GameEvents.FromToAction<Part, Part> data)
        {

            Logger.Log("Vessel '{0}' ({1}) docked to vessel '{2}' ({3})", data.from.vessel.vesselName, data.from.vessel.rootPart.flightID, data.to.vessel.vesselName, data.to.vessel.rootPart.flightID);
            AGOSMain.Instance.getMasterAGOSModule(data.to.vessel).setFlightID(data.to.vessel.rootPart.flightID, true, data.from.vessel.rootPart.flightID);

        }

        private void onVesselDestroy(Vessel data)
        {
            Logger.Log("Vessel destroy");
        }

        private void onVesselChange(Vessel data)
        {
            if (data != this.lastVessel)
            {
                Logger.Log("Vessel changed");
                onFlightReady();
                /*if (lastVessel != null && isVesselInFlight())
                {
                    Logger.Log("Player switched vessel; reverts are now invalid. Removing action group backups.");
                    AGOSMain.backupActionGroups.Clear();
                }*/
                this.lastVessel = data;
            }
        }

        private void onVesselUnpack(Vessel v)
        {
            Logger.Log("Vessel unpack");
            AGOSMain.Instance.removeDuplicateActionGroups();
            //AGOSMain.Instance.removeInvalidActionGroups();
            AGOSMain.Instance.getMasterAGOSModule(v).setFlightID(v.rootPart.flightID);
            AGOSMain.Instance.findHomesForPartLockedGroups(v);
        }

        private void onFlightReady()
        {
            Logger.Log("Flight ready");
            //AGOSMain.Instance.restoreBackedUpActionGroups();
            AGOSMain.Instance.removeDuplicateActionGroups();
            //AGOSMain.Instance.removeInvalidActionGroups();
            AGOSMain.Instance.getMasterAGOSModule(FlightGlobals.fetch.activeVessel).setFlightID(AGOSUtils.getFlightID());
            AGOSMain.Instance.findHomesForPartLockedGroups(FlightGlobals.fetch.activeVessel);
            //AGOSDebug.printAllActionGroups();

            //AGOSMain.Instance.backupActionGroupList();
        }

        private void onVesselLoaded(Vessel data)
        {
            AGOSMain.Instance.removeDuplicateActionGroups();
            AGOSMain.Instance.getMasterAGOSModule(data).setFlightID(data.rootPart.flightID);
            AGOSMain.Instance.findHomesForPartLockedGroups(data);
            //AGOSDebug.printAllActionGroups();
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
            activateGroupsForStage(stage); 
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
            List<IActionGroup> thisVesselsGroups = new List<IActionGroup>();
            thisVesselsGroups.AddRange(AGOSMain.Instance.actionGroups.FindAll(a => AGOSUtils.isLoadedCraftID(a.FlightID)));
            int stageNum = (AGOSMain.Instance.useAGXConfig && s >= 8 ? s - 7 : s);
            toFire.AddRange(thisVesselsGroups.FindAll(a => a.Stages != null && a.Stages.Length > 0 && a.Stages.Contains(stageNum)/* && a.Vessel == FlightGlobals.fetch.activeVessel*/)); // Regular action groups
            toFire.AddRange(thisVesselsGroups.FindAll(a => a.isPartLocked && a.linkedPart.inverseStage == stageNum/* && a.Vessel == FlightGlobals.fetch.activeVessel*/)); // Part locked groups
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
                    if (AGOSMain.Settings.get<bool>("InstantCameraTransitions"))
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
                else if (ag.GetType() == typeof(TimeDelayedActionGroup) && ag.timerDelay > -1)
                {

                    Logger.Log("Group '{0}' will be fired in {1}'s", ag.fireGroupID, ag.timerDelay);
                    DateTime dt = DateTime.Now.AddSeconds(ag.timerDelay);
                    this.groupTimers.Add(ag, dt);

                }
                else
                {
                    fireActionGroup(ag.GetType() == typeof(TimeDelayedActionGroup) ? ag.fireGroupID : ag.Group);
                }
                if (ag.isPartLocked || ag.Stages.Count(a => a < FlightGlobals.fetch.activeVessel.currentStage) == 1)
                {
                    Logger.Log("Removing action group from config as it has no more triggers");
                    AGOSMain.Instance.actionGroups.Remove(ag);
                }
            }
            processingStageEvent = false;
        }

        public void fireActionGroup(int g)
        {

            if (AGOSMain.Instance.useAGXConfig && g >= 8)
            {
                AGX.AGXInterface.AGExtToggleGroup(g - 7);
                Logger.Log("Firing AGX Action Group #{0}", g - 7);
            }
            else
            {
                //Logger.Log("{0}", ag.Group);
                KSPActionGroup _g = AGOSMain.Instance.stockAGMap[g];
                Logger.Log("Firing action group {0} ({1})", g, AGOSMain.Instance.actionGroupList[g]);
                FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(_g);
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
                    Logger.Log("Locking staging");
                    CURRENT_TICK_COUNT = 0;
                    stageLockScheduled = false;
                    Logger.Log("Stage lock: {0} -> {1}", FlightInputHandler.fetch.stageLock, !FlightInputHandler.fetch.stageLock);
                    FlightInputHandler.fetch.stageLock = !FlightInputHandler.fetch.stageLock;
                }
            }

        }

        public void Update()
        {
            if (this.groupTimers.Count > 0)
            {
                List<IActionGroup> groups = new List<IActionGroup>(this.groupTimers.Keys);

                foreach (IActionGroup a in groups)
                {
                    if (DateTime.Compare(DateTime.Now, this.groupTimers[a]) >= 0)
                    {
                        Logger.Log("Delay for group '{0}' ('{1}') has expired.", a.Group, a.fireGroupID);
                        fireActionGroup(a.fireGroupID);
                        this.groupTimers.Remove(a);
                    }
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
            if (!(AGOSMain.Instance.isGameGUIHidden && AGOSMain.Settings.get<bool>("SilenceWhenUIHidden")))
            {
                System.Random ra = new System.Random();
                ScreenMessages.PostScreenMessage((ra.NextBoolOneIn(10) && AGOSMain.Settings.get<bool>("AllowEE") ? "fINE cONTROLS" : "Fine Controls") + " have been " + (FlightInputHandler.fetch.precisionMode ? "enabled" : "disabled") + ".", 5f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public bool isVesselInFlight()
        {
            Vessel vessel = FlightGlobals.ActiveVessel;
            return (vessel.situation != Vessel.Situations.PRELAUNCH && vessel.situation != Vessel.Situations.LANDED && vessel.situation != Vessel.Situations.SPLASHED && vessel.GetHeightFromSurface() > 10f);
        }
    }
}
