using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups
{
    public class StageLockActionGroup : AGOSActionGroup
    {
        public override void fire()
        {
            this.fireOnVessel(FlightGlobals.fetch.activeVessel);
        }

        public override void fireOnVessel(Vessel v)
        {
            FlightInputHandler.fetch.stageLock = !FlightInputHandler.fetch.stageLock;
        }

        public override void fireOnVesselID(uint vID)
        {
            this.fireOnVessel(null);
        }
    }
}
