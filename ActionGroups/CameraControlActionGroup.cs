using AGroupOnStage.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups
{
    public class CameraControlActionGroup : AGOSActionGroup
    {
        public override void fire()
        {
            this.fireOnVessel(FlightGlobals.fetch.activeVessel);
        }

        public override void fireOnVessel(Vessel v)
        {
            if (AGOSMain.Settings.get<bool>("InstantCameraTransitions"))
                FlightCamera.SetModeImmediate(this.cameraMode);
            else
                FlightCamera.SetMode(this.cameraMode);
        }

        public override void fireOnVesselID(uint vID)
        {
            this.fireOnVessel(null);
        }

    }
}
