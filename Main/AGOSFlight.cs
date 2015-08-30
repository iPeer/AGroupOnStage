using AGroupOnStage.ActionGroups;
using AGroupOnStage.Logging;
using AGroupOnStage.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Timers;

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
        private List<Timer> groupTimers = new List<Timer>();

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
                GameEvents.onCrewOnEva.Add(onCrewOnEVA); //             2.0.10-dev3: Disable AGOS toolbar buttons when EVAing a Kerbal
                GameEvents.onCrewBoardVessel.Add(onCrewBoardVessel); // ^
                //GameEvents.onUndock.Add(onUndock);
                //GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);
                //GameEvents.onVesselGoOffRails.Add(onVesselUnpack);
                AGOSMain.Instance.FlightEventsRegistered = true;
                Logger.Log("Registered for Flight related GameEvents");
            }
            //GameEvents.onVesselLoaded.Add(AGOSMain.Instance.onVesselLoaded);
            //AGOSUtils.resetActionGroupConfig();
        }

        private void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> data)
        {
            AGOSToolbarManager.enableToolbarButton();
        }

        private void onCrewOnEVA(GameEvents.FromToAction<Part, Part> data)
        {
            if (AGOSMain.Instance.guiVisible)
                AGOSMain.Instance.toggleGUI(); 
            AGOSToolbarManager.disableToolbarButton();
        }

        private void OnPartUndock(Part data)
        {
            //Logger.Log("Vessel undock: {0}", data.vessel.name);
            /*activateDockingUndockingActionGroups(data.vessel, AGOSActionGroup.FireTypes.UNDOCK);

            ModuleDockingNode mdn = (ModuleDockingNode)data.Modules["ModuleDockingNode"];
            if (mdn != null)
            {
                uint otherVesselID = mdn.vesselInfo.rootPartUId;

                StartCoroutine(finishPartUndocProcess(otherVesselID));
            }
            else
            {
                Logger.LogError("Unable to acquire ModuleDockingNode for modified docking node!");
            }*/

            AGOSMain.Instance.getMasterAGOSModule(data.vessel).resetFlightID(data.vessel.rootPart.flightID);
        }

        public IEnumerator<WaitForSeconds> finishPartUndocProcess(uint id)
        {
            yield return null;

            List<Vessel> vessels = new List<Vessel>(FlightGlobals.fetch.vessels); // Safe list to iterate over
            Vessel undocked = vessels.Find(v => v.rootPart != null && v.rootPart.flightID == id);//vessels.Find(v => v.parts.FindAll(p => p.flightID == id).Any());

            foreach (Vessel __v in FlightGlobals.fetch.vessels)
                if (__v == undocked)
                    Logger.Log("Undocked vessel is in loaded vessel pool");

            if (undocked == null)
                Logger.LogError("No undocked vessel found with ID '{0}'", id);
            else
            {
                //Logger.Log("UNDOCKED: {0}", undocked.vesselName);
                activateDockingUndockingActionGroups(undocked, AGOSActionGroup.FireTypes.UNDOCK);
            }

        }

        /*public void delayedNonControlledVesselUndockFire(uint flightID) 
        {
            this.undockingTimer.Stop();
            //this.undockingTimer.Enabled = false;
            this.undockingTimer.Dispose();

            Vessel v = FlightGlobals.Vessels.Find(a => a.rootPart.flightID == flightID);
            try { Logger.Log("{0}", v.vesselName); }
            catch (NullReferenceException) { Logger.Log("Vessel == null"); }

        }*/

        private void OnPartCouple(GameEvents.FromToAction<Part, Part> data)
        {

            Logger.Log("Vessel '{0}' ({1}) docked to vessel '{2}' ({3})", data.from.vessel.vesselName, data.from.vessel.rootPart.flightID, data.to.vessel.vesselName, data.to.vessel.rootPart.flightID);
            activateDockingUndockingActionGroups(data.to.vessel, AGOSActionGroup.FireTypes.DOCK);
            activateDockingUndockingActionGroups(data.from.vessel, AGOSActionGroup.FireTypes.DOCK);
            AGOSMain.Instance.getMasterAGOSModule(data.to.vessel).setFlightID(data.to.vessel.rootPart.flightID, true, data.from.vessel.rootPart.flightID);

        }

        private void onVesselDestroy(Vessel data)
        {
            Logger.Log("Vessel destroy");
        }

        private void onVesselChange(Vessel data)
        {

            if (data.isEVA) // 2.0.10-dev2: Don't run on Kerbals on EVA.
            {
                if (AGOSMain.Instance.guiVisible)
                    AGOSMain.Instance.toggleGUI();
                // 2.0.10-dev3: Don't allow AGOS' toolbar button to be clicked if it's an EVA.
                AGOSToolbarManager.disableToolbarButton();
                return; 
            }
            AGOSToolbarManager.enableToolbarButton(); // 2.0.10-dev3: Make sure the button is enabled if the active vessel IS NOT an EVA.
            if (data != this.lastVessel)
            {
                Logger.Log("Vessel changed");
                onFlightReady();
                /*if (lastVessel != null && isVesselInFlight())
                {
                    Logger.Log("Player switched vessel; reverts are now invalid. Removing action group backups.");
                    AGOSMain.backupActionGroups.Clear();
                }*/
                AGOSMain.Instance.linkPart = null; // 2.0.10-dev3: Fix for https://github.com/iPeer/AGroupOnStage/issues/18
                this.lastVessel = data;
            }
        }

        private void onVesselUnpack(Vessel v)
        {

            if (v.isEVA) // 2.0.10-dev2: Don't run on Kerbals on EVA.
            {
                if (AGOSMain.Instance.guiVisible)
                    AGOSMain.Instance.toggleGUI(); 
                return;
            } 

            Logger.Log("Vessel unpack");
            AGOSMain.Instance.removeDuplicateActionGroups();
            //AGOSMain.Instance.removeInvalidActionGroups();
            AGOSMain.Instance.getMasterAGOSModule(v).setFlightID(v.rootPart.flightID);
            AGOSMain.Instance.findHomesForPartLockedGroups(v);
        }

        private void onFlightReady()
        {

            if (FlightGlobals.fetch.activeVessel.isEVA) // 2.0.10-dev2: Don't run on Kerbals on EVA.
            {
                if (AGOSMain.Instance.guiVisible)
                    AGOSMain.Instance.toggleGUI();
                // 2.0.10-dev3: Don't allow AGOS' toolbar button to be clicked if it's an EVA.
                AGOSToolbarManager.disableToolbarButton();
                return;
            }

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

            if (data.isEVA) // 2.0.10-dev2: Don't run on Kerbals on EVA.
            {
                if (AGOSMain.Instance.guiVisible)
                    AGOSMain.Instance.toggleGUI();
                return;
            }

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
            List<AGOSActionGroup> toFire = new List<AGOSActionGroup>();
            List<AGOSActionGroup> thisVesselsGroups = new List<AGOSActionGroup>();
            thisVesselsGroups.AddRange(AGOSMain.Instance.actionGroups.FindAll(a => AGOSUtils.isLoadedCraftID(a.FlightID)));
            int stageNum = (AGOSMain.Instance.useAGXConfig && s >= 8 ? s - 7 : s);
            toFire.AddRange(thisVesselsGroups.FindAll(a => a.Stages != null && a.Stages.Length > 0 && a.Stages.Contains(stageNum)/* && a.Vessel == FlightGlobals.fetch.activeVessel*/)); // Regular action groups
            toFire.AddRange(thisVesselsGroups.FindAll(a => a.isPartLocked && a.linkedPart.inverseStage == stageNum/* && a.Vessel == FlightGlobals.fetch.activeVessel*/)); // Part locked groups
            //List<IActionGroup> toFire = AGOSMain.Instance.actionGroups.FindAll(a => (a.Stages.Length > 0 && a.Stages.Contains(s)) || (a.isPartLocked && a.linkedPart.inverseStage == s));
            Logger.Log("{0} group(s) to fire", toFire.Count);
            foreach (AGOSActionGroup ag in toFire) 
            {
                ag.fire();
                ag.removeIfNoTriggers();
            }
            //AGOSMain.Instance.removeGroupsWithNoTriggers();
            processingStageEvent = false;
        }

        public void activateDockingUndockingActionGroups(Vessel v, AGOSActionGroup.FireTypes fireType = AGOSActionGroup.FireTypes.DOCK)
        {
            List<AGOSActionGroup> toFire = new List<AGOSActionGroup>();
            List<AGOSActionGroup> vesselGroups = new List<AGOSActionGroup>();

            //Logger.LogDebug("DOCK/UNDOCK: {0}", v.vesselName);

            vesselGroups.AddRange(AGOSMain.Instance.actionGroups.FindAll(a => a.FlightID == v.rootPart.flightID));
            toFire.AddRange(vesselGroups.FindAll(b => b.FireType == fireType));

            Logger.Log("{0} group(s) to fire", toFire.Count);

            foreach (AGOSActionGroup ag in toFire)
            {
                ag.fireOnVessel(v);
                ag.removeIfNoTriggers();
            }

        }

        [Obsolete("Use AGOSActionGroup.fire[OnVessel]([vessel]) instead", false)]
        public void processGroup(AGOSActionGroup ag, bool onDock = false, Vessel v = null)
        {
            if (ag.GetType() == typeof(StageLockActionGroup))
            {
                toggleStageLock();
                Logger.Log("Stage lock toggled");
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
                //this.groupTimers.Add(ag, dt);

            }
            else
            {
                if (onDock)
                    fireActionGroup(ag.GetType() == typeof(TimeDelayedActionGroup) ? ag.fireGroupID : ag.Group, v);
                else
                    fireActionGroup(ag.GetType() == typeof(TimeDelayedActionGroup) ? ag.fireGroupID : ag.Group);
            }
            if ((ag.isPartLocked || ag.Stages.Count(a => a < FlightGlobals.fetch.activeVessel.currentStage) == 1) && ag.FireType == AGOSActionGroup.FireTypes.STAGE)
            {
                Logger.Log("Removing action group from config as it has no more triggers");
                AGOSMain.Instance.actionGroups.Remove(ag);
            }
        }

        [Obsolete("Use AGOSActionGroup.fire[OnVessel]([vessel]) instead", false)]
        public void fireActionGroup(int g)
        {
            fireActionGroup(g, null);
        }

        [Obsolete("Use AGOSActionGroup.fire[OnVessel]([vessel]) instead", false)]
        public void fireActionGroup(int g, Vessel v)
        {

            if (AGOSMain.Instance.useAGXConfig && g >= 8)
            {
                // TODO: AGX support for (un)docking vessels
                AGX.AGXInterface.AGExtToggleGroup(g - 7);
                Logger.Log("Firing AGX Action Group #{0}", g - 7);
            }
            else
            {
                //Logger.Log("{0}", ag.Group);
                KSPActionGroup _g = AGOSMain.Instance.stockAGMap[g];
                Logger.Log("Firing action group {0} ({1}) on vessel '{2}'", g, AGOSMain.Instance.actionGroupList[g], (v == null ? FlightGlobals.fetch.activeVessel : v).vesselName);
                if (v == null)
                    FlightGlobals.fetch.activeVessel.ActionGroups.ToggleGroup(_g);
                else
                {
                    Vessel ves = v;
                    foreach (Vessel _v in FlightGlobals.fetch.vessels) // Work around for groups not firing on this vessel?
                        if (_v == v)
                            ves = _v;
                    ves.ActionGroups.ToggleGroup(_g);
                }
                //(v == null ? FlightGlobals.ActiveVessel : v).ActionGroups.ToggleGroup(_g);
            }

        }

        public void toggleStageLock()
        {
            /*if (this.stageLockTimer != null)
            {
                this.stageLockTimer.Stop();
                this.stageLockTimer.Dispose();
            }*/

            FlightInputHandler.fetch.stageLock = !FlightInputHandler.fetch.stageLock;
            Logger.Log("StageLock = {0}", FlightInputHandler.fetch.stageLock);


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

        /*public void Update()
        {
            if (this.groupTimers.Count > 0) // TODO: Replace this with timers
            {
                List<AGOSActionGroup> groups = new List<AGOSActionGroup>(this.groupTimers.Keys);

                foreach (AGOSActionGroup a in groups)
                {
                    if (DateTime.Compare(DateTime.Now, this.groupTimers[a]) >= 0)
                    {
                        Logger.Log("Delay for group '{0}' ('{1}') has expired.", a.Group, a.fireGroupID);
                        fireActionGroup(a.fireGroupID);
                        this.groupTimers.Remove(a);
                    }
                }

            }
        }*/

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
                ScreenMessages.PostScreenMessage((ra.NextBoolOneIn(AGOSMain.Settings.get<int>("FineControlsEEChance")) && AGOSMain.Settings.get<bool>("AllowEE") ? "fINE cONTROLS" : "Fine Controls") + " have been " + (FlightInputHandler.fetch.precisionMode ? "enabled" : "disabled") + ".", 5f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public List<Timer> registerTimer(Timer timer)
        {
            if (!this.groupTimers.Contains(timer))
                this.groupTimers.Add(timer);
            return this.groupTimers;
        }

        public List<Timer> unregisterTimer(Timer timer)
        {
            if (this.groupTimers.Contains(timer))
            {
                timer.Stop();
                timer.Dispose();
                this.groupTimers.Remove(timer);
            }
            return this.groupTimers;
        }

        public void unregisterAllTimers()
        {
            foreach (Timer t in this.groupTimers)
            {
                t.Stop();
                t.Dispose();
            }
            this.groupTimers.Clear();
        }

        public bool isVesselInFlight()
        {
            Vessel vessel = FlightGlobals.ActiveVessel;
            return (vessel.situation != Vessel.Situations.PRELAUNCH && vessel.situation != Vessel.Situations.LANDED && vessel.situation != Vessel.Situations.SPLASHED && vessel.GetHeightFromSurface() > 10f);
        }
    }
}
