using System;

namespace AGroupOnStage.ActionGroups {
	public class ActionGroup {

		private Part part;
		private int aGroup;
		private ActionGroupFireStyle mode;
        /*private bool isThrottle;
        private float throttleLevel = 0f;*/

        public ActionGroup(Part p, int a, ActionGroupFireStyle f) : this(p, a, f, false) { }
		public ActionGroup(Part part, int aGroup, ActionGroupFireStyle fireMode, bool isControllingThrottle) {

			this.part = part;
			this.aGroup = aGroup;
			this.mode = fireMode;
            this.isThrottle = isControllingThrottle;

		}

        public ActionGroup setThrottleLevel(float t)
        {
            this.isThrottle = true;
            this.throttleLevel = t;
            return this;
        }

		public ActionGroup setPart(Part part) {
			this.part = part;
			return this;
		}

		public ActionGroup setGroup(int group) {
			this.aGroup = group;
			return this;
		}

		public ActionGroup setMode(ActionGroupFireStyle mode) {
			this.mode = mode;
			return this;
		}

        public float throttleLevel { get; set; }
        public bool isThrottle { get; set; }

		public Part getPart() {
			return this.part;
		}

		public int getGroup() {
			return this.aGroup;
		}

		public ActionGroupFireStyle getMode() {
			return this.mode;
		}

		// Convenience methods

		public int getPartIID() {
			return this.part.GetInstanceID();
		}

		public string getPartName() {
			return this.part.partInfo.title;
		}

		public string toSavableString() {
			return this.aGroup.ToString()+(this.isThrottle ? ","+this.throttleLevel : "");
		}

        public float getRequiredUpgradeLevel()
        {

            if (this.isThrottle || this.aGroup.ToString().ToLower().StartsWith("custom"))
                return 1f;
            else
                return 0.5f;

        }

	}
}

