/* Let me just get this out of the way:
*	This is my first time really properly programming in C# so this code is probably going to be pretty bad.
*	Hopefully, over time I'll get better at it, who knows!
*
*		- iPeer
*
*/

using System;

namespace AGroupOnStage
{
	public class AGroupOnStage : PartModule
	{



		public override void OnStart (StartState state)
		{
			//Stuff goes here, or something
		}

		public override string GetInfo() {
			return "Fire action groups unpon staging.";
		}

		public override void OnAwake() {
			//Log ("I'm awake!");
		}

		public override void OnActive() {
			Log("Decoupled!");
			// Fire the group here(?)
			// 		this.part.vessel.ActionGroups.ToggleGroup
		}

		public void log(String msg) {
			Log(msg);
		} 

		public void Log(String msg) {
			PDebug.Log("[AGroupOnStage]: "+msg);
		}
	}
}

