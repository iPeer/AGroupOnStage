using AGroupOnStage.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups
{
    public class FineControlActionGroup : AGOSActionGroup
    {
        public override void fire()
        {
            this.fireOnVessel(FlightGlobals.fetch.activeVessel);
        }

        public override void fireOnVessel(Vessel v)
        {
            AGOSFlight.Instance.toggleFineControls();
        }

        public override void fireOnVesselID(uint vID)
        {
            this.fireOnVessel(null);
        }

    }
}
