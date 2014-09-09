/* Let me just get this out of the way:
*	This is my first time really properly programming in C# so this code is probably going to be pretty bad.
*	Hopefully, over time I'll get better at it, who knows!
*
*		- iPeer
*
*/

using System.Collections.Generic;
using UnityEngine;
using KSP.IO;
using System;

namespace AGroupOnStage {
	public class AGroupOnStage : PartModule {

		private Rect _windowPos = new Rect();
		private bool guiOpen = false;
		private GUIStyle _labelStyle;
		private bool initStyles = false;
		public Dictionary<int, KSPActionGroup> aGroups = new Dictionary<int, KSPActionGroup>() {

			{ 0, KSPActionGroup.Custom01 },
			{ 1, KSPActionGroup.Custom02 },
			{ 2, KSPActionGroup.Custom03 },
			{ 3, KSPActionGroup.Custom04 },
			{ 4, KSPActionGroup.Custom05 },
			{ 5, KSPActionGroup.Custom06 },
			{ 6, KSPActionGroup.Custom07 },
			{ 7, KSPActionGroup.Custom08 },
			{ 8, KSPActionGroup.Custom09 },
			{ 9, KSPActionGroup.Custom10 },
			{ 10, KSPActionGroup.Gear },
			{ 11, KSPActionGroup.Light },
			{ 12, KSPActionGroup.Brakes },
			{ 13, KSPActionGroup.Abort },
			{ 14, KSPActionGroup.RCS },
			{ 15, KSPActionGroup.SAS }

		};
			
		public bool custom01 = false;
		public bool custom02 = false;
		public bool custom03 = false;
		public bool custom04 = false;
		public bool custom05 = false;
		public bool custom06 = false;
		public bool custom07 = false;
		public bool custom08 = false;
		public bool custom09 = false;
		public bool custom10 = false;
		public bool gear = false;
		public bool lights = false;
		public bool breaks = false;
		public bool abort = false;
		public bool rcs = false;
		public bool sas = false;

		[KSPEvent(active = true, guiActive = true, guiActiveEditor = true, guiName = "Action group control")]
		public void toggleGUI() {
			if (guiOpen)
				RenderingManager.RemoveFromPostDrawQueue(+this.part.GetInstanceID(), OnDraw);
			else
				RenderingManager.AddToPostDrawQueue(+this.part.GetInstanceID(), OnDraw);
			guiOpen = !guiOpen;
		}

		public override string GetInfo() {
			return "Can trigger action groups when staged.";
		}

		public override void OnActive() {
			bool[] groups = new bool[aGroups.Count];
			groups[0] = custom01;
			groups[1] = custom02;
			groups[2] = custom03;
			groups[3] = custom04;
			groups[4] = custom05;
			groups[5] = custom06;
			groups[6] = custom07;
			groups[7] = custom08;
			groups[8] = custom09;
			groups[9] = custom10;
			groups[10] = gear;
			groups[11] = lights;
			groups[12] = breaks;
			groups[13] = abort;
			groups[14] = rcs;
			groups[15] = sas;

			for (int x = 0; x < 16; x++) {
				if (groups[x]) {
					this.part.vessel.ActionGroups.ToggleGroup(aGroups[x]);
					Log("Toggled group '" + aGroups[x] + "' for part '" + this.part.name + "' in stage " + this.part.inverseStage);
				}
			}

		}

		public void Log(string msg) {
			PDebug.Log("[AGroupOnStage]: " + msg);
		}


		public override void OnSave(ConfigNode node) {
			/* Do we really need to save and load the position?
			PluginConfiguration cfg = PluginConfiguration.CreateForType<AGroupOnStage> ();
			cfg.SetValue ("winPos", _windowPos);
			cfg.save ();
			*/

			bool[] groups = new bool[aGroups.Count];
			groups[0] = custom01;
			groups[1] = custom02;
			groups[2] = custom03;
			groups[3] = custom04;
			groups[4] = custom05;
			groups[5] = custom06;
			groups[6] = custom07;
			groups[7] = custom08;
			groups[8] = custom09;
			groups[9] = custom10;
			groups[10] = gear;
			groups[11] = lights;
			groups[12] = breaks;
			groups[13] = abort;
			groups[14] = rcs;
			groups[15] = sas;

			for (int x = 0; x < aGroups.Count; x++)
				node.AddValue(aGroups[x].ToString().ToLower(), groups[x]);


		}

		public override void OnLoad(ConfigNode node) {
			/* Do we really need to save and load the position?
			PluginConfiguration cfg = PluginConfiguration.CreateForType<AGroupOnStage> ();
			cfg.load ();
			_windowPos = cfg.GetValue<Rect> ("winPos");
			*/

			custom01 = Convert.ToBoolean(node.GetValue("custom01"));
			custom02 = Convert.ToBoolean(node.GetValue("custom02"));
			custom03 = Convert.ToBoolean(node.GetValue("custom03"));
			custom04 = Convert.ToBoolean(node.GetValue("custom04"));
			custom05 = Convert.ToBoolean(node.GetValue("custom05"));
			custom06 = Convert.ToBoolean(node.GetValue("custom06"));
			custom07 = Convert.ToBoolean(node.GetValue("custom07"));
			custom08 = Convert.ToBoolean(node.GetValue("custom08"));
			custom09 = Convert.ToBoolean(node.GetValue("custom09"));
			custom10 = Convert.ToBoolean(node.GetValue("custom10"));
			gear = Convert.ToBoolean(node.GetValue("gear"));
			lights = Convert.ToBoolean(node.GetValue("lights"));
			breaks = Convert.ToBoolean(node.GetValue("breaks"));
			abort = Convert.ToBoolean(node.GetValue("abort"));
			rcs = Convert.ToBoolean(node.GetValue("rcs"));
			sas = Convert.ToBoolean(node.GetValue("sas"));

			// Turns out we nuke the part catalogue if we don't check this... Who would've thunk it?
			if (HighLogic.LoadedScene != GameScenes.LOADING)
				Log("Loaded Action Group config for part '" + this.part.name + "' ('" + this.part.partInfo.title + "'/" + this.part.GetInstanceID() + ") in stage " + this.part.inverseStage);

		}



		// GUI STUFF

		private void OnDraw() {
			if (this.vessel == FlightGlobals.ActiveVessel) {
				// Use this.part.GetInstanceID() to (hopefully) prevent the GUI from getting stuck open if you open one from another part.
				// Edit: Didn't work
				// TODO: Allow users to somehow highlight the part (or something) the window belongs to in the event of multiple parts with the same name.
				_windowPos = GUILayout.Window(+this.part.GetInstanceID(), _windowPos, OnWindow, "Action Group Control");
				// Center the GUI if it is at 0,0
				if (_windowPos.x == 0f && _windowPos.y == 0f) {
					_windowPos.x = Screen.width / 2 - _windowPos.width / 2;
					_windowPos.y = Screen.height / 2 - _windowPos.height / 2;
				}

			}
		}

		private void OnWindow(int winID) {
			if (!initStyles) {
				//_labelStyle = new GUIStyle();
				_labelStyle = new GUIStyle(GUI.skin.label);
				//_labelStyle.normal.textColor = _labelStyle.active.textColor = _labelStyle.focused.textColor = _labelStyle.hover.textColor = Color.white;
				_labelStyle.stretchWidth = initStyles = true;
			}
			GUILayout.BeginHorizontal(GUILayout.Width(250f));
			GUILayout.Label("Action group control for '" + this.part.partInfo.title + "'\n\nCheck which groups you want to fire when this part is staged.", _labelStyle);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			custom01 = GUILayout.Toggle(custom01, "Custom01", GUILayout.ExpandWidth(false));
			custom02 = GUILayout.Toggle(custom02, "Custom02", GUILayout.ExpandWidth(false));
			custom03 = GUILayout.Toggle(custom03, "Custom03", GUILayout.ExpandWidth(false));
			custom04 = GUILayout.Toggle(custom04, "Custom04", GUILayout.ExpandWidth(false));
			custom05 = GUILayout.Toggle(custom05, "Custom05", GUILayout.ExpandWidth(false));

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			custom06 = GUILayout.Toggle(custom06, "Custom06", GUILayout.ExpandWidth(false));
			custom07 = GUILayout.Toggle(custom07, "Custom07", GUILayout.ExpandWidth(false));
			custom08 = GUILayout.Toggle(custom08, "Custom08", GUILayout.ExpandWidth(false));
			custom09 = GUILayout.Toggle(custom09, "Custom09", GUILayout.ExpandWidth(false));
			custom10 = GUILayout.Toggle(custom10, "Custom10", GUILayout.ExpandWidth(false));

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			gear = GUILayout.Toggle(gear, "Gear", GUILayout.ExpandWidth(false));
			lights = GUILayout.Toggle(lights, "Lights", GUILayout.ExpandWidth(false));
			breaks = GUILayout.Toggle(breaks, "Breaks", GUILayout.ExpandWidth(false));
			abort = GUILayout.Toggle(abort, "Abort", GUILayout.ExpandWidth(false));
			rcs = GUILayout.Toggle(rcs, "RCS", GUILayout.ExpandWidth(false));
			sas = GUILayout.Toggle(sas, "SAS", GUILayout.ExpandWidth(false));

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Close", GUILayout.ExpandWidth(true))) {
				toggleGUI();
			}
			GUILayout.EndHorizontal();

			GUI.DragWindow();
		}


	}
}

