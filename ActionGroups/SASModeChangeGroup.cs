using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups
{
    class SASModeChangeGroup : AGOSActionGroup
    {

        public override void fire()
        {
            this.fireOnVessel(FlightGlobals.fetch.activeVessel);
        }

        public override void fireOnVessel(Vessel v)
        {
            int sasMode = this.fireGroupID;
            VesselAutopilot.AutopilotMode mode = AGroupOnStage.Main.AGOSUtils.getSASModeForID(sasMode);
            if (!v.ActionGroups.groups[16]) // SAS disabled
            {

                v.ActionGroups.SetGroup(KSPActionGroup.SAS, true);

            }
            if (v.Autopilot.CanSetMode(mode))
            {
                Logger.Log("Setting SAS mode to '{0}' on vessel '{1}' ({2}, {3})", mode.ToString(), v.vesselName, v.rootPart.flightID, v.id);
                v.Autopilot.SetMode(mode);
            }
            else
            {
                Logger.LogWarning("Vessel '{1}' ({2}, {3}) cannot be set into SAS mode '{0}'", sasMode.ToString(), v.vesselName, v.rootPart.flightID, v.id);
            }
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
