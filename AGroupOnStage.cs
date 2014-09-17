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
		private bool isPartHighlighted = false;
		private static GUIStyle _windowStyle, _labelStyle, _toggleStyle, _buttonStyle;
		private static int skinID = -1;
		private static bool hasInitStyles = false, loadedSkins = false;
		public static Dictionary<int, GUISkin> guiSkins = new Dictionary<int, GUISkin>();
		public static int highlightedParts = 0;
		private bool hasSetColourID = false;
		private int colourID = 0;

		// Some modded installs edit or add skins, these are the skins we prefer to use where available, in order of preference
		public static Dictionary<int, string> preferredSkins = new Dictionary<int, string>() {

			{ 0, "GameSkin(Clone)" },
			{ 1, "GameSkin" }

		};

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

		public static Dictionary<int, Color> colourIndex = new Dictionary<int, Color>() {

//			Quoted colours don't work very well (low contrast) or at all.

//			{ 0, Color.black },
			{ 0, Color.blue },
//			{ 2, Color.clear },
			{ 1, Color.cyan },
//			{ 2, Color.gray },
			{ 2, Color.green },
			{ 3, Color.magenta },
			{ 4, Color.red },
			{ 5, Color.white },
			{ 6, Color.yellow }

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
					// Make sure we fire the AG on the active vessel, not the stage(s) we just dropped.
					// TODO: Make this configurable?
					FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(aGroups[x]);
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

		//		public override void OnUpdate() {
		//
		//			if (isPartHighlighted) {
		//
		//				this.part.SetHighlightColor(Color.blue); // Why would you not have Colour as an alias :c
		//				this.part.SetHighlight(true);
		//
		//			}
		//
		//		}



		// GUI STUFF

		private void OnDraw() {
			if (this.vessel == FlightGlobals.ActiveVessel) {
				if (!hasInitStyles) {
					hasInitStyles = true;
					if (!loadedSkins) {
						loadedSkins = true;
						GUISkin[] skins = Resources.FindObjectsOfTypeAll(typeof(GUISkin)) as GUISkin[];
						int _skinID = 0;
						foreach (GUISkin _skin in skins) {
							guiSkins.Add(_skinID++, _skin);
							#if DEBUG
							Log("Skin: " + _skin.name);
							#endif
						}
					}
					//GUI.skin = Resources.Load("KSP window 2") as GUISkin;
					//GUI.skin = HighLogic.Skin;

					if (skinID == -1) {
						int __skin = -1;
						while (skinID == -1 && __skin++ < preferredSkins.Count)
							skinID = getSkinIDForName(preferredSkins[__skin]);
						if (skinID > -1)
							Log("Skin has been set to " + preferredSkins[__skin]);
					}
					GUISkin skinRef = guiSkins[skinID];
					if (skinRef == null || skinID == -1) {
						Log("skinRef == null or skinID == -1, defaulting to HighLogic.Skin");
						skinRef = HighLogic.Skin;
					}

					_windowStyle = new GUIStyle(skinRef.window);
					_windowStyle.fixedWidth = 500f;
					_labelStyle = new GUIStyle(skinRef.label);
					_labelStyle.stretchWidth = true;
					_toggleStyle = new GUIStyle(skinRef.toggle);
					//_toggleStyle.fixedWidth = 50f;
					_toggleStyle.stretchWidth = true;
					_buttonStyle = new GUIStyle(skinRef.button);
					_buttonStyle.stretchWidth = true;

					/*#if DEBUG
					Log("Theme: " + GUI.skin);
					Log("Toggle: " + GUI.skin.toggle);
					Log("Toggle border: " + GUI.skin.toggle.border);
					Log("Toggle offset: " + GUI.skin.toggle.contentOffset);
					Log("Toggle margin: " + GUI.skin.toggle.margin);
					Log("Toggle alignment: " + GUI.skin.toggle.alignment);
					Log("Toggle Font: " + GUI.skin.toggle.font);
					Log("Toggle Font size: " + GUI.skin.toggle.fontSize);
					Log("Toggle style: " + GUI.skin.toggle.fontStyle);
					#endif*/

				}
				// Use this.part.GetInstanceID() to (hopefully) prevent the GUI from getting stuck open if you open one from another part.
				// Edit: Didn't work
				// TODO: Allow users to somehow highlight the part (or something) the window belongs to in the event of multiple parts with the same name.
				_windowPos = GUILayout.Window(+this.part.GetInstanceID(), _windowPos, OnWindow, "Action Group Control", _windowStyle);
				// Center the GUI if it is at 0,0
				if (_windowPos.x == 0f && _windowPos.y == 0f) {
					_windowPos.x = Screen.width / 2 - _windowPos.width / 2;
					_windowPos.y = Screen.height / 2 - _windowPos.height / 2;
				}

			}
		}

		private void OnWindow(int winID) {

			GUILayout.BeginHorizontal();
			GUILayout.Label("Action group control for '" + this.part.partInfo.title + "'", _labelStyle);
//			GUILayout.EndHorizontal();
//
//			GUILayout.BeginHorizontal();

			isPartHighlighted = GUILayout.Toggle(isPartHighlighted, "Highlight", _buttonStyle);
			if (!isPartHighlighted) {
				if (hasSetColourID)
					highlightedParts--;
				hasSetColourID = false;
				this.part.SetHighlightDefault();
			}
			else {
				if (!hasSetColourID) {
					colourID = highlightedParts;
					highlightedParts++;
					if (colourID >= colourIndex.Count)
						colourID = (colourID - (int)Math.Floor((double)(highlightedParts * colourIndex.Count))) - 1; // I have to cast this to double apparently...
					hasSetColourID = true;
					#if DEBUG
					Log("ColourID: " + colourID);
					#endif
				}
				try {
					this.part.SetHighlightColor(colourIndex[colourID]);
				} catch (Exception e) {
				}
				this.part.SetHighlight(true);
			}

			GUILayout.EndHorizontal(); 


			GUILayout.BeginHorizontal();
			GUILayout.Label("Check which groups you want to fire when this part is staged.", _labelStyle);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			actionGroups["custom01"] = GUILayout.Toggle(actionGroups["custom01"], "Custom01", _toggleStyle);
			actionGroups["custom02"] = GUILayout.Toggle(actionGroups["custom02"], "Custom02", _toggleStyle);
			actionGroups["custom03"] = GUILayout.Toggle(actionGroups["custom03"], "Custom03", _toggleStyle);
			actionGroups["custom04"] = GUILayout.Toggle(actionGroups["custom04"], "Custom04", _toggleStyle);
			actionGroups["custom05"] = GUILayout.Toggle(actionGroups["custom05"], "Custom05", _toggleStyle);

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			actionGroups["custom06"] = GUILayout.Toggle(actionGroups["custom06"], "Custom06", _toggleStyle);
			actionGroups["custom07"] = GUILayout.Toggle(actionGroups["custom07"], "Custom07", _toggleStyle);
			actionGroups["custom08"] = GUILayout.Toggle(actionGroups["custom08"], "Custom08", _toggleStyle);
			actionGroups["custom09"] = GUILayout.Toggle(actionGroups["custom09"], "Custom09", _toggleStyle);
			actionGroups["custom10"] = GUILayout.Toggle(actionGroups["custom10"], "Custom10", _toggleStyle);

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			actionGroups["gear"] = GUILayout.Toggle(actionGroups["gear"], "Gear", _toggleStyle);
			actionGroups["light"] = GUILayout.Toggle(actionGroups["light"], "Lights", _toggleStyle);
			actionGroups["brakes"] = GUILayout.Toggle(actionGroups["brakes"], "Brakes", _toggleStyle);
			actionGroups["abort"] = GUILayout.Toggle(actionGroups["abort"], "Abort", _toggleStyle);
			actionGroups["rcs"] = GUILayout.Toggle(actionGroups["rcs"], "RCS", _toggleStyle);
			actionGroups["sas"] = GUILayout.Toggle(actionGroups["sas"], "SAS", _toggleStyle);

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Close", _buttonStyle)) {
				isPartHighlighted = false;
				this.part.SetHighlightDefault();
				highlightedParts--;
				hasSetColourID = false;
				toggleGUI();
			}
			#if DEBUG

			if (GUILayout.Button(skinID + ": " + guiSkins[skinID].name, _buttonStyle)) {
				skinID++;
				if (skinID >= guiSkins.Count)
					skinID = 0;
				hasInitStyles = false;
			}

			#endif
			GUILayout.EndHorizontal();

			GUI.DragWindow();
		}

		public int getSkinIDForName(string name) {

			int id = 0;
			foreach (GUISkin _skin in guiSkins.Values) {
				if (_skin.name == name)
					return id;
				id++;
			}
			return -1;

		}

	}
}

