using AGroupOnStage.ActionGroups.Timers;
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
            Logger.Log("Creating timer to fire group ID {0} on vessel '{1}' ({2}, {3})", this.Group, v.vesselName, v.rootPart.flightID, v.id);
            ActionGroupTimer agt = new ActionGroupTimer(this.fireGroupID, this.FlightID, this.timerDelay);
            AGOSActionGroupTimerManager.Instance.registerTimer(agt, HighLogic.LoadedSceneIsFlight);
        }

        public override void fireOnVesselID(uint vID)
        {
            Vessel vessel = FlightGlobals.fetch.vessels.Find(v => v.rootPart.flightID == vID);

            if (vessel == null)
            {
                Logger.LogError("Tried to fire action group on vessel ID {0} but no such vessel ID exists", vID);
                return;
            }

            fireOnVessel(vessel);
        }


    }
}
