/* Let me just get this out of the way:
*	This is my first time really properly programming in C# so this code is probably going to be pretty bad.
*	Hopefully, over time I'll get better at it, who knows!
*
*		- iPeer
*
*/

using System;
using System.Collections.Generic;

namespace AGroupOnStage
{
	public class AGroupOnStage : PartModule
	{

		public Dictionary<int, KSPActionGroup> aGroups = new Dictionary<int, KSPActionGroup>() {

			{0, KSPActionGroup.Custom01},
			{1, KSPActionGroup.Custom02},
			{2, KSPActionGroup.Custom03},
			{3, KSPActionGroup.Custom04},
			{4, KSPActionGroup.Custom05},
			{5, KSPActionGroup.Custom06},
			{6, KSPActionGroup.Custom07},
			{7, KSPActionGroup.Custom08},
			{8, KSPActionGroup.Custom09},
			{9, KSPActionGroup.Custom10},
			{10, KSPActionGroup.Gear},
			{11, KSPActionGroup.Light},
			{12, KSPActionGroup.Brakes},
			{13, KSPActionGroup.Abort},
			{14, KSPActionGroup.RCS},
			{15, KSPActionGroup.SAS}

		};

		// I wonder if there's a cleaner way of doing this without thirdparty libraries...
		// This makes a really big right click menu for decouplers...

		[KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName="Action group control")]
		public string aSuperAwesomeString = "";

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Custom01"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool custom01 = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Custom02"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool custom02 = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Custom03"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool custom03 = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Custom04"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool custom04 = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Custom05"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool custom05 = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Custom06"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool custom06 = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Custom07"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool custom07 = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Custom08"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool custom08 = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Custom09"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool custom09 = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Custom10"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool custom10 = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Gear"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool gear = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Lights"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool lights = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Breaks"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool breaks = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="Abort"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool abort = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="RCS"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool rcs = false;

		[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName="SAS"),
			UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
		public bool sas = false;

		public override void OnStart (StartState state)
		{
			//Stuff goes here, or something
		}

		public override string GetInfo() {
			return "Can trigger action groups when staged.";
		}

		public override void OnAwake() {
			//Log ("I'm awake!");
		}

		public override void OnActive() {
			//Log("Decoupled! Stage: "+this.part.inverseStage);

			// Fire the group here(?)
			// 		this.part.vessel.ActionGroups.ToggleGroup

			// Not needed, but it makes it easier
			bool[] groups = new bool[aGroups.Count];
			groups [0] = custom01;
			groups [1] = custom02;
			groups [2] = custom03;
			groups [3] = custom04;
			groups [4] = custom05;
			groups [5] = custom06;
			groups [6] = custom07;
			groups [7] = custom08;
			groups [8] = custom09;
			groups [9] = custom10;
			groups [10] = gear;
			groups [11] = lights;
			groups [12] = breaks;
			groups [13] = abort;
			groups [14] = rcs;
			groups [15] = sas;

			for (int x = 0; x < 16; x++) {
				if (groups [x]) {
					this.part.vessel.ActionGroups.ToggleGroup (aGroups [x]);
					Log ("Toggled group '" + aGroups [x] + "' for part '" + this.part.name + "' in stage " + this.part.inverseStage);
				}
			}

		}

		public void Log(String msg) {
			PDebug.Log("[AGroupOnStage]: "+msg);
		}
	}
}

