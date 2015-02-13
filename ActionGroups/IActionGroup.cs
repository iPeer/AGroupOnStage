using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups
{
    interface IActionGroup
    {

        int Stage { get; set; }
        int Group { get; set; }
        float ThrottleLevel { get; set; }
        FlightCamera.Modes cameraMode { get; set; }

    }
}
