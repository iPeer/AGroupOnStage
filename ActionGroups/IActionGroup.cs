using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups
{
    public interface IActionGroup
    {

        Vessel Vessel { get; set; }
        int[] Stages { get; set; }
        int Group { get; set; }
        float ThrottleLevel { get; set; }
        FlightCamera.Modes cameraMode { get; set; }
        Part linkedPart { get; set; }
        bool isPartLocked { get; set; }
        string partRef { get; set; }
        int timerDelay { get; set; }
        int fireGroupID { get; set; }
        uint FlightID { get; set; }
        string StagesAsString { get; }

    }
}
