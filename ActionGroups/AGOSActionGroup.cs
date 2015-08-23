using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups
{

    public abstract class AGOSActionGroup
    {

        public enum FireTypes 
        {
            STAGE,
            DOCK,
            UNDOCK
        }

        private FireTypes fireType = FireTypes.STAGE;
        private int[] _Stages = new int[0];

        public virtual Vessel Vessel { get; set; }
        public virtual int[] Stages 
        {
            get
            {
                return this._Stages;
            }
            set
            {
                this._Stages = value;
            }
        }
        public virtual int Group { get; set; }
        public virtual float ThrottleLevel { get; set; }
        public virtual FlightCamera.Modes cameraMode { get; set; }
        public virtual Part linkedPart { get; set; }
        public virtual bool isPartLocked { get; set; }
        public virtual string partRef { get; set; }
        public virtual int timerDelay { get; set; }
        public virtual int fireGroupID { get; set; }
        public virtual uint FlightID { get; set; }
        public virtual string StagesAsString
        {
            get
            {
                return AGroupOnStage.Main.AGOSUtils.intArrayToString(Stages);
            }
        }
        public virtual uint OriginalFlightID { get; set; }
        public virtual bool onDock { get; set; }

        public virtual FireTypes FireType
        {
            get
            {
                return this.fireType;
            }

            set
            {
                this.fireType = value;
            }
        }

        // The module will not save groups where this is true to the save file.
        public virtual bool IsTester { get; set; }

    }
}
