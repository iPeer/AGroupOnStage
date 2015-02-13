using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups
{
    class FineControlActionGroup : IActionGroup
    {
        public int Stage { get; set; }
        public int Group { get; set; }
        public float ThrottleLevel { get; set; }
        public FlightCamera.Modes cameraMode { get; set; }
    }
}
