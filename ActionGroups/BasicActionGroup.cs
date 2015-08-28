using AGroupOnStage.Logging;
using AGroupOnStage.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups
{
    public class BasicActionGroup : AGOSActionGroup
    {
        /*public Vessel Vessel { get; set; }
        public int[] Stages { get; set; }
        public int Group { get; set; }
        public float ThrottleLevel { get; set; }
        public FlightCamera.Modes cameraMode { get; set; }
        public Part linkedPart { get; set; }
        public bool isPartLocked { get; set; }
        public string partRef { get; set; }
        public int timerDelay { get; set; }
        public int fireGroupID { get; set; }
        public uint FlightID { get; set; }
        public uint OriginalFlightID { get; set; }
        public string StagesAsString
        {
            get
            {
                return AGroupOnStage.Main.AGOSUtils.intArrayToString(Stages);
            }
        }
        public bool onDock { get; set; }*/

        public override void fire()
        {
            this.fireOnVessel(FlightGlobals.fetch.activeVessel);
        }

        public override void fireOnVessel(Vessel v)
        {
            if (AGOSMain.Instance.useAGXConfig && this.Group > 7)
            {
                int group = this.Group - 7;
                Logger.Log("Firing AGX action group #{0} on vessel '{1}' ({2}, {3})", group, v.vesselName, v.rootPart.flightID, v.id);
                AGX.AGXInterface.AGExtToggleGroupOnFlightID(v.rootPart.flightID, group);
            }
            else
            {
                KSPActionGroup group = AGOSMain.Instance.stockAGMap[this.Group];
                v.ActionGroups.ToggleGroup(group);
                Logger.Log("Activated action group '{0}' on vessel '{1}' ({2}, {3})", group.ToString(), v.vesselName, v.rootPart.flightID, v.id);
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
