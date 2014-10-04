using System;

namespace AGroupOnStage.ActionGroups {
	public class ActionGroup {

		private Part part;
		private int aGroup;
		private ActionGroupFireStyle mode;

		public ActionGroup(Part part, int aGroup, ActionGroupFireStyle fireMode) {

			this.part = part;
			this.aGroup = aGroup;
			this.mode = fireMode;

		}

		public ActionGroup setPart(Part part) {
			this.part = part;
			return this;
		}

		public ActionGroup setGroup(int group) {
			this.aGroup = aGroup;
			return this;
		}

		public ActionGroup setMode(ActionGroupFireStyle mode) {
			this.mode = mode;
			return this;
		}

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
			return this.aGroup.ToString() + "," + this.getMode().ToString();
		}

	}
}

