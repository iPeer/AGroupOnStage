using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups
{
    class CameraControlActionGroup : IActionGroup
    {
        public Vessel Vessel { get; set; }
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
    }
}
