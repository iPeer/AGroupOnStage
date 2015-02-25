using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups
{
    class FineControlActionGroup : IActionGroup
    {
        public Vessel Vessel { get; set; }
        public int[] Stages { get; set; }
        public int Group { get; set; }
        public float ThrottleLevel { get; set; }
        public FlightCamera.Modes cameraMode { get; set; }
        public Part linkedPart { get; set; }
        public bool isPartLocked { get; set; }
        public string partRef { get; set; }
    }
}
