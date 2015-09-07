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
        }

        public void OnDestroy()
        {
            Logger.Log("Timer manager destroy");
            GameEvents.onFlightReady.Remove(onFlightReady);
            GameEvents.onGamePause.Remove(onGamePause);
            GameEvents.onGameUnpause.Remove(onGameUnpause);
            unregisterAllTimers();
            Instance = null;
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
            this.flightReady = true;
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
