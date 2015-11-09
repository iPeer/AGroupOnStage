using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.ActionGroups.Timers
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AGOSActionGroupTimerManager : MonoBehaviour
    {

        public static AGOSActionGroupTimerManager Instance { get; protected set; }
        private bool flightReady = false;
        public bool isGamePaused = false;

        public List<ActionGroupTimer> activeTimers = new List<ActionGroupTimer>();

        public void Awake()
        {
            Instance = this;
            Logger.Log("Timer manager startup");
            GameEvents.onFlightReady.Add(onFlightReady);
            GameEvents.onGamePause.Add(onGamePause);
            GameEvents.onGameUnpause.Add(onGameUnpause);
            GameEvents.onVesselDestroy.Add(onVesselDestroy);
            GameEvents.onVesselGoOffRails.Add(onVesselGoOffRails);
        }

        public void OnDestroy()
        {
            Logger.Log("Timer manager destroy");
            GameEvents.onFlightReady.Remove(onFlightReady);
            GameEvents.onGamePause.Remove(onGamePause);
            GameEvents.onGameUnpause.Remove(onGameUnpause);
            GameEvents.onVesselDestroy.Remove(onVesselDestroy);
            GameEvents.onVesselGoOffRails.Remove(onVesselGoOffRails);
            unregisterAllTimers();
            Instance = null;
        }

        private void onVesselGoOffRails(Vessel data)
        {
            if (data.rootPart == null) { return; }
            List<ActionGroupTimer> timers = activeTimersForVessel(data.rootPart.flightID, false);
            foreach (ActionGroupTimer t in timers)
            {
                t.flightReady();
            }
        }

        private void onVesselDestroy(Vessel data)
        {
            if (data.rootPart == null) { return; }
            List<ActionGroupTimer> vesselTimers = this.activeTimers.FindAll(a => a.FlightID == data.rootPart.flightID);
            Logger.Log("Vessel destroyed: {0}. Unregistering timers for this vessel", data.vesselName);
            foreach (ActionGroupTimer t in vesselTimers)
                unregisterTimer(t);
        }

        private void onGamePause()
        {
            this.isGamePaused = true;
        }

        private void onGameUnpause()
        {
            this.isGamePaused = false;
        }

        public void onFlightReady()
        {
            Logger.Log("Timer manager: OnFlightReady");
            this.flightReady = true;
            //this.enableAllLoadedTimers();
        }

        [Obsolete]
        public void enableAllLoadedTimers()
        {
            List<Vessel> loadedVessels = new List<Vessel>();
            loadedVessels.AddRange(FlightGlobals.fetch.vessels.FindAll(a => a.rootPart != null && !a.HoldPhysics));
            /*loadedVessels.ForEach(
                v => 
                {
                    this.activeTimers.FindAll(a => a.FlightID == v.rootPart.flightID).ForEach(
                        t => 
                        { 
                            Logger.Log("Enabling timer '{0}'", t.Guid); 
                            t.Enabled = true; 
                        }); 
                });*/

            Logger.Log("{0} vessel(s) to update timer state for", loadedVessels.Count);

            foreach (Vessel v in loadedVessels)
            {
                List<ActionGroupTimer> timers = new List<ActionGroupTimer>();
                timers.AddRange(activeTimersForVessel(v.rootPart.flightID, false));

                Logger.Log("{0} timer(s) to update to flight state for vessel {1}", timers.Count, v.vesselName);

                foreach (ActionGroupTimer t in timers)
                {
                    t.flightReady();
                }

            }
        }

        public bool areTimersActive(uint flightID = 0)
        {

            if (flightID == 0)
                flightID = FlightGlobals.fetch.activeVessel.rootPart.flightID;

            return activeTimers.Any(a => a.Enabled && a.FlightID == flightID);

        }

        public List<ActionGroupTimer> activeTimersForVessel(Vessel v)
        {
            return activeTimersForVessel(v.rootPart.flightID);
        }

        public List<ActionGroupTimer> activeTimersForVessel(uint flightID, bool enabledOnly = true)
        {
            if (enabledOnly)
                return activeTimers.FindAll(a => a.Enabled && a.FlightID == flightID);
            return activeTimers.FindAll(a => a.FlightID == flightID);
        }

        public void registerTimer(ActionGroupTimer timer, bool activate = false)
        {
            if (this.activeTimers.Contains(timer))
            {
                Logger.Warning("Specified timer is already registered!");
                return;
            }

            this.activeTimers.Add(timer);
            if (HighLogic.LoadedSceneIsFlight && this.flightReady/* && activate*/)
            {
                timer.flightReady();
                /*if (activate)
                    timer.Enabled = true;*/
            }
            Logger.Log("Registered timer '{0}' (activate = {1})", timer.Guid, activate);

        }

        public void unregisterTimer(ActionGroupTimer timer)
        {
            if (timer.Enabled)
                timer.Enabled = false;
            Logger.Log("Unregistering timer '{0}'", timer.Guid);
            this.activeTimers.Remove(timer);
        }

        public void unregisterAllTimers()
        {
            foreach (ActionGroupTimer t in this.activeTimers)
                unregisterTimer(t);
        }

        public ConfigNode getConfigNodeForVessel(Vessel v)
        {
            return getConfigNodeForVessel(v.rootPart.flightID);
        }

        public ConfigNode getConfigNodeForVessel(uint flightID = 0)
        {
            List<ActionGroupTimer> timers = new List<ActionGroupTimer>();
            // If no flight ID is given, return a config node containing *all* timers
            if (flightID == 0)
                timers = this.activeTimers.ToList();
            else // Otherwise, limit it to the ones on the flightID given
                timers.AddRange(this.activeTimers.FindAll(a => a.FlightID == flightID));

            int id = 0; // ID used in saving, not relevant to the actual timer itself.
            ConfigNode @return = new ConfigNode();

            if (timers.Count == 0)
                return null; // Nothing to save
            foreach (ActionGroupTimer t in timers)
            {
                //@return.AddNode(id.ToString());
                ConfigNode timerNode = new ConfigNode();
                //public bool Enabled { get; set; }
                //public bool Initialised { get { return _initialised; } }
                //public int Group { get { return _group; } }
                //public double LastUpdate { get { return _lastUpdate; } }
                //public uint FlightID { get { return _flightID; } }
                //public int Delay { get { return _delay; } }
                //public int RemainingDelay { get { return _remainingDelay; } }
                //public Guid Guid { get { return this._guid; } }
                timerNode.AddValue("Guid", t.Guid);
                timerNode.AddValue("Group", t.Group);
                timerNode.AddValue("LastUpdate", t.LastUpdate);
                timerNode.AddValue("FlightID", t.FlightID);
                timerNode.AddValue("Delay", t.Delay);
                timerNode.AddValue("RemainingDelay", t.RemainingDelay);
                @return.AddNode(id.ToString(), timerNode);
                id++;
            }

            return @return;

        }

        public int loadTimersFromConfigNode(ConfigNode node)
        {
            Logger.Log("Loading from save node:\n");
            Logger.Log("{0}", node.ToString());
            int loaded = 0;
            foreach (ConfigNode n in node.nodes)
            {
                ActionGroupTimer t = new ActionGroupTimer();
                t.createFromConfigNode(n);
                registerTimer(t, false);
                loaded++;
            }
            return loaded;
        }

        public void readyTimers()
        {
            foreach (ActionGroupTimer t in activeTimers)
            {
                t.flightReady();
            }
        }

        public void FixedUpdate()
        {
            this.activeTimers.FindAll(a => a.Enabled && a.Initialised && a.ShouldUpdate).ForEach(b => b.tick());
        }

    }
}
