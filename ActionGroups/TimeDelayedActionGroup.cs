using AGroupOnStage.Logging;
using AGroupOnStage.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace AGroupOnStage.ActionGroups
{
    public class TimeDelayedActionGroup : AGOSActionGroup
    {

        public override void fire()
        {
            this.fireOnVessel(FlightGlobals.fetch.activeVessel);
        }

        public override void fireOnVessel(Vessel v)
        {
            Timer timer = new Timer();
            timer.Interval = this.timerDelay * 1000d;
            timer.Elapsed += (e, sender) => this.timerVesselCallBack(timer, v);
            timer.Start();
            Logger.Log("Timer for action group {0} has been started for vessel '{1}' ({2}, {3})", this.fireGroupID, v.vesselName, v.rootPart.flightID, v.id);
            AGOSFlight.Instance.registerTimer(timer);
        }

        public override void fireOnVesselID(uint vID)
        {
            Timer timer = new Timer();
            timer.Interval = this.timerDelay * 1000d;
            timer.Elapsed += (e, sender) => this.timerVIDCallBack(timer, vID);
            timer.Start();
            Logger.Log("Timer for action group {0} has been started for vessel '{1}'", this.fireGroupID, vID);
            AGOSFlight.Instance.registerTimer(timer);
        }

        private void timerVIDCallBack(Timer t, uint vID)
        {
            Logger.Log("Timer for action group {0} has expired for vessel '{1}'", this.fireGroupID, vID);
            BasicActionGroup bag = new BasicActionGroup();
            bag.Group = this.fireGroupID;
            bag.fireOnVesselID(vID);
            AGOSFlight.Instance.unregisterTimer(t);
        }

        private void timerVesselCallBack(Timer t, Vessel vessel)
        {
            Logger.Log("Timer for action group {0} has expired for vessel '{1}' ({2}, {3})", this.fireGroupID, vessel.vesselName, vessel.rootPart.flightID, vessel.id);
            BasicActionGroup bag = new BasicActionGroup();
            bag.Group = this.fireGroupID;
            bag.fireOnVessel(vessel);
            AGOSFlight.Instance.unregisterTimer(t);
        }

    }
}
