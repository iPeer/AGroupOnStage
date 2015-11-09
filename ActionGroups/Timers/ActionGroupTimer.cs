using AGroupOnStage.Logging;
using AGroupOnStage.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups.Timers
{
    public class ActionGroupTimer
    {

        private double _lastUpdate = 0d;
        private uint _flightID = 0;
        private int _delay = 10;
        private int _remainingDelay = 10;
        private int _group = 0;
        private Guid _guid;
        private bool _initialised = false;

        public bool Enabled { get; set; }
        public bool Initialised { get { return _initialised; } }
        public int Group { get { return _group; } }
        public double LastUpdate { get { return _lastUpdate; } }
        public uint FlightID { get { return _flightID; } }
        public int Delay { get { return _delay; } }
        public int RemainingDelay { get { return _remainingDelay; } }
        public Guid Guid { get { return this._guid; } }

        public ActionGroupTimer() { }

        public ActionGroupTimer(int group, uint flightID, int delay)
        {
            this._group = group;
            this._flightID = flightID;
            this._delay = this._remainingDelay = delay;
            this._guid = System.Guid.NewGuid();
            this._initialised = true;
        }

        // Note: LastUpdate shouldn't be zero EXEPT when the timer is first created. 
        // It should be updated to the correct time on flight ready or immediately if the flight is already ready
        public bool ShouldUpdate { get { return this.Initialised && this.Enabled && (Planetarium.GetUniversalTime() - this._lastUpdate) >= 1 || this._lastUpdate == 0; } }

        public void tickIfShouldUpdate() // I'm lazy.
        {
            if (ShouldUpdate)
                tick();
        }

        public void tick()
        {

            this._lastUpdate = Planetarium.GetUniversalTime();

            if (AGOSActionGroupTimerManager.Instance.isGamePaused || AGOSUtils.getVesselForFlightID(this._flightID).HoldPhysics)
                return;

            if (--this._remainingDelay == 0) // Fire the group
            {
                AGOSActionGroup ag = new BasicActionGroup();
                ag.Group = this._group;
                ag.FlightID = ag.OriginalFlightID = this._flightID;
                ag.fireOnVesselID(this._flightID);
                this.Enabled = false; // Disable so we can't fire this group again
                AGOSActionGroupTimerManager.Instance.unregisterTimer(this); // Tell the manager to unregister this timer
                return;
            }
        }

        public void flightReady()
        {
            Logger.Log("Updating timer '{0}' to flight ready state", this._guid);
            this._lastUpdate = Planetarium.GetUniversalTime();
            this.Enabled = true;
        }

        public void createFromConfigNode(ConfigNode node)
        {
            Logger.Log("Trying to create timer from node: {0}", node.ToString());
            try
            {
                this._guid = new Guid(node.GetValue("Guid"));
            }
            catch
            {
                Logger.LogWarning("Could not parse Guid for timer, assigning it a new one");
                this._guid = Guid.NewGuid();
            }
            this._delay = Convert.ToInt32(node.GetValue("Delay"));
            this._remainingDelay = Convert.ToInt32(node.GetValue("RemainingDelay"));
            this._flightID = Convert.ToUInt32(node.GetValue("FlightID"));
            this._group = Convert.ToInt32(node.GetValue("Group"));
            this._lastUpdate = Convert.ToDouble(node.GetValue("LastUpdate"));
            this._initialised = true;
        }

    }
}
