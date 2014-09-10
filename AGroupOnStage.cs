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
		//private bool isPartHighlighted = false;
		private GUIStyle _labelStyle;
		private bool hasInitStyles = false;
		public static Dictionary<int, KSPActionGroup> aGroups = new Dictionary<int, KSPActionGroup>() {

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

		// Do not make static!
		public Dictionary<string, bool> actionGroups = new Dictionary<string, bool>() {

			{ "custom01", false },
			{ "custom02", false },
			{ "custom03", false },
			{ "custom04", false },
			{ "custom05", false },
			{ "custom06", false },
			{ "custom07", false },
			{ "custom08", false },
			{ "custom09", false },
			{ "custom10", false },
			{ "gear", false },
			{ "light", false },
			{ "brakes", false },
			{ "abort", false },
			{ "rcs", false },
			{ "sas", false }

		};

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
			// if the window is open, close it.
			if (guiOpen)
				toggleGUI();
			for (int x = 0; x < 16; x++) {
				if (actionGroups[aGroups[x].ToString().ToLower()]) {
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

			for (int x = 0; x < aGroups.Count; x++) {
				try {
					node.AddValue(aGroups[x].ToString().ToLower(), actionGroups[aGroups[x].ToString().ToLower()]);
				} catch (Exception e) { 
					Log("Couldn't save setting for '" + aGroups[x] + "' (" + e.Message + ")");
				}
			}

		}

		public override void OnLoad(ConfigNode node) {
			/* Do we really need to save and load the position?
			PluginConfiguration cfg = PluginConfiguration.CreateForType<AGroupOnStage> ();
			cfg.load ();
			_windowPos = cfg.GetValue<Rect> ("winPos");
			*/

			actionGroups["custom01"] = Convert.ToBoolean(node.GetValue("custom01"));
			actionGroups["custom02"] = Convert.ToBoolean(node.GetValue("custom02"));
			actionGroups["custom03"] = Convert.ToBoolean(node.GetValue("custom03"));
			actionGroups["custom04"] = Convert.ToBoolean(node.GetValue("custom04"));
			actionGroups["custom05"] = Convert.ToBoolean(node.GetValue("custom05"));
			actionGroups["custom06"] = Convert.ToBoolean(node.GetValue("custom06"));
			actionGroups["custom07"] = Convert.ToBoolean(node.GetValue("custom07"));
			actionGroups["custom08"] = Convert.ToBoolean(node.GetValue("custom08"));
			actionGroups["custom09"] = Convert.ToBoolean(node.GetValue("custom09"));
			actionGroups["custom10"] = Convert.ToBoolean(node.GetValue("custom10"));
			actionGroups["gear"] = Convert.ToBoolean(node.GetValue("gear"));
			actionGroups["light"] = Convert.ToBoolean((node.HasNode("lights") ? node.GetValue("lights") : node.GetValue("light")));
			// I'm an idiot and have been typoing this all along...
			actionGroups["brakes"] = Convert.ToBoolean((node.HasNode("breaks") ? node.GetValue("breaks") : node.GetValue("brakes")));
			actionGroups["abort"] = Convert.ToBoolean(node.GetValue("abort"));
			actionGroups["rcs"] = Convert.ToBoolean(node.GetValue("rcs"));
			actionGroups["sas"] = Convert.ToBoolean(node.GetValue("sas"));

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
			if (!hasInitStyles) {
				GUI.skin = null;
				_labelStyle = new GUIStyle(GUI.skin.label);
				_labelStyle.stretchWidth = hasInitStyles = true;

				// Detective work to find what mod was breaking the GUI's style :/

				Log("Theme: " + GUI.skin);
				Log("Toggle: " + GUI.skin.toggle);
				Log("Toggle border: " + GUI.skin.toggle.border);
				Log("Toggle offset: " + GUI.skin.toggle.contentOffset);
				Log("Toggle margin: " + GUI.skin.toggle.margin);
				Log("Toggle alignment: " + GUI.skin.toggle.alignment);
				Log("Toggle Font: " + GUI.skin.toggle.font);
				Log("Toggle Font size: " + GUI.skin.toggle.fontSize);
				Log("Toggle style: " + GUI.skin.toggle.fontStyle);

			}
			GUILayout.BeginHorizontal(GUILayout.Width(250f));
			GUILayout.Label("Action group control for '" + this.part.partInfo.title + "'", _labelStyle);
			GUILayout.EndHorizontal();

			/* This doesn't really work how I wanted it to do (it highlights all parts in the heriarchy)
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Highlight"))
				isPartHighlighted = !isPartHighlighted;
			if (isPartHighlighted) {
				this.part.highlightRecurse = false;
				this.part.SetHighlightType(Part.HighlightType.AlwaysOn);
			}
			else
				this.part.SetHighlightDefault();
			this.part.SetHighlight(isPartHighlighted);
			GUILayout.EndHorizontal(); 
			*/

			GUILayout.BeginHorizontal();
			GUILayout.Label("Check which groups you want to fire when this part is staged.", _labelStyle);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			actionGroups["custom01"] = GUILayout.Toggle(actionGroups["custom01"], "Custom01", GUILayout.ExpandWidth(true));
			actionGroups["custom02"] = GUILayout.Toggle(actionGroups["custom02"], "Custom02", GUILayout.ExpandWidth(true));
			actionGroups["custom03"] = GUILayout.Toggle(actionGroups["custom03"], "Custom03", GUILayout.ExpandWidth(true));
			actionGroups["custom04"] = GUILayout.Toggle(actionGroups["custom04"], "Custom04", GUILayout.ExpandWidth(true));
			actionGroups["custom05"] = GUILayout.Toggle(actionGroups["custom05"], "Custom05", GUILayout.ExpandWidth(true));

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			actionGroups["custom06"] = GUILayout.Toggle(actionGroups["custom06"], "Custom06", GUILayout.ExpandWidth(true));
			actionGroups["custom07"] = GUILayout.Toggle(actionGroups["custom07"], "Custom07", GUILayout.ExpandWidth(true));
			actionGroups["custom08"] = GUILayout.Toggle(actionGroups["custom08"], "Custom08", GUILayout.ExpandWidth(true));
			actionGroups["custom09"] = GUILayout.Toggle(actionGroups["custom09"], "Custom09", GUILayout.ExpandWidth(true));
			actionGroups["custom10"] = GUILayout.Toggle(actionGroups["custom10"], "Custom10", GUILayout.ExpandWidth(true));

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			actionGroups["gear"] = GUILayout.Toggle(actionGroups["gear"], "Gear", GUILayout.ExpandWidth(true));
			actionGroups["light"] = GUILayout.Toggle(actionGroups["light"], "Lights", GUILayout.ExpandWidth(true));
			actionGroups["brakes"] = GUILayout.Toggle(actionGroups["brakes"], "Brakes", GUILayout.ExpandWidth(true));
			actionGroups["abort"] = GUILayout.Toggle(actionGroups["abort"], "Abort", GUILayout.ExpandWidth(true));
			actionGroups["rcs"] = GUILayout.Toggle(actionGroups["rcs"], "RCS", GUILayout.ExpandWidth(true));
			actionGroups["sas"] = GUILayout.Toggle(actionGroups["sas"], "SAS", GUILayout.ExpandWidth(true));

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

