using AGroupOnStage.Logging;
using AGroupOnStage.Main;
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

        public virtual bool stillHasTriggers
        {
            get
            {
                if (this.FireType != FireTypes.STAGE) { return true; }
                return this.isPartLocked || this.Stages.Count(a => a < FlightGlobals.fetch.activeVessel.currentStage) > 1;
                //return ((this.isPartLocked || this.Stages.Count(a => a < FlightGlobals.fetch.activeVessel.currentStage) == 1) && this.FireType == AGOSActionGroup.FireTypes.STAGE);
            }
        }

        public virtual void removeIfNoTriggers()
        {

            if (!this.stillHasTriggers)
            {
                Logger.Log("Removing action group from config as it has no more triggers");
                AGOSMain.Instance.actionGroups.Remove(this);
            }
        }

        /// <summary>
        /// (Overridable) Fire this action group on the active vessel
        /// </summary>
        public virtual void fire()
        {
            this.fireOnVessel(FlightGlobals.fetch.activeVessel);
        }

        /// <summary>
        /// (Overridable) Fire this action group on the specified vessel
        /// </summary>
        /// <param name="v">The vessel to fire this group on</param>
        public virtual void fireOnVessel(Vessel v)
        {
            Logger.LogWarning("Action group config for Group {0} is using an unimplemented group type while trying to activate for vessel '{1}'", this.Group, v.vesselName);
        }

        /// <summary>
        /// (Overridable) Fire this group on the vessel with the specified flight ID (if it exists)
        /// </summary>
        /// <param name="vID">The ID of the vessel to fire this group on</param>
        public virtual void fireOnVesselID(uint vID)
        {
            Logger.LogWarning("Action group config for Group {0} is using an unimplemented group type while trying to activate for vessel ID '{1}'", this.Group, vID);
        }

    }
}
