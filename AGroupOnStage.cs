/* Let me just get this out of the way:
*	This is my first time really properly programming in C# so this code is probably going to be pretty bad.
*	Hopefully, over time I'll get better at it, who knows!
*
*		- iPeer
*
*/

/* PERSONAL NOTE: CHANGE BACK TO STABLE LIBRARIES AFTER EXPERIMENTALS ARE OVER */

using System.Collections.Generic;
using UnityEngine;
using KSP.IO;
using System;
using AGroupOnStage.ActionGroups;
using iPeerLib.Logging;
using iPeerLib.Utility;
using System.Globalization;

namespace AGroupOnStage
{

    [KSPModule("Action Group On Stage")]
    public class AGroupOnStage : PartModule
    {

        private static readonly int configVersion = 3;
        private Rect _windowPosAddGroup = new Rect(), _windowPos = new Rect();
        private bool guiOpen = false, addGuiOpen = false;
        private bool isPartHighlighted = false;
        private static GUIStyle _windowStyle, _labelStyle, _labelStyleCentre, _toggleStyle, _buttonStyle, _scrollStyle, _groupButtonStyle, _addWindowStyle, _labelStyleModeLabel, _sliderStyle, _sliderStyleSlider, _sliderStyleThumb;
        private static bool hasInitStyles = false;
        public static Dictionary<int, GUISkin> guiSkins = new Dictionary<int, GUISkin>();
        public static int highlightedParts = 0;
        private bool hasSetColourID = false;
        private int colourID = 0;
        public static List<ActionGroup> groupList = new List<ActionGroup>();
        private Vector2 scrollPos = /*What's your */Vector2/*, Victor?*/.zero;
        private ActionGroupFireStyle groupMode = ActionGroupFireStyle.ACTIVE_VESSEL;
        private bool hideMainDialogue = false;
        private float throttleLevel = 0f;

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
			{ 15, KSPActionGroup.SAS },
            { 16, KSPActionGroup.Stage }

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
			{ "sas", false },
            { "stage", false }, // Never true
            { "throttle", false }

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
        public void toggleGUI()
        {
            if (guiOpen)
            {
                RenderingManager.RemoveFromPostDrawQueue(+this.part.GetInstanceID(), OnDraw);
                if (isSceneVABOrSPH())
                    EditorLogic.fetch.Unlock("AGOS_INPUT_LOCK");
                // TODO: Fix in 0.90

                /* if (isPartHighlighted)
                {
                    isPartHighlighted = false;
                    if (hasSetColourID)
                        highlightedParts--;
                    hasSetColourID = false;
                    this.part.SetHighlightDefault();
                }*/
            }
            else
            {
                RenderingManager.AddToPostDrawQueue(+this.part.GetInstanceID(), OnDraw);
#if DEBUG
                Log("VABorSPH: " + isSceneVABOrSPH());
#endif
                if (isSceneVABOrSPH())
                {
                    EditorTooltip.Instance.HideToolTip();
                    EditorLogic.fetch.Lock(true, true, true, "AGOS_INPUT_LOCK");
                }
                // TODO: Find alternative for 0.90
                /*isPartHighlighted = true;
                if (!hasSetColourID)
                {
                    colourID = highlightedParts;
                    highlightedParts++;
                    if (colourID >= colourIndex.Count)
                        colourID = (colourID - (int)Math.Floor((double)(highlightedParts * colourIndex.Count))) - 1; // I have to cast this to double apparently...
                    hasSetColourID = true;
#if DEBUG
                    Log("ColourID: " + colourID);
#endif
                }
try
                {
                    this.part.SetHighlightColor(colourIndex[colourID]);
                    this.part.SetHighlight(true);
                }
                catch (Exception e) { LogError(e); }*/
            }


            guiOpen = !guiOpen;

        }

        public void toggleAddGUI()
        {
            if (addGuiOpen)
                RenderingManager.RemoveFromPostDrawQueue(+this.part.GetInstanceID(), OnDrawAddGroup);
            else
                RenderingManager.AddToPostDrawQueue(+this.part.GetInstanceID(), OnDrawAddGroup);
            addGuiOpen = !addGuiOpen;
            hideMainDialogue = !hideMainDialogue;
        }

        public override string GetInfo()
        {
            return "Can trigger action groups when staged.";
        }

        public override void OnActive()
        {
            // if the window is open, close it.
            if (guiOpen)
                toggleGUI();
            if (addGuiOpen)
                toggleAddGUI();


            foreach (ActionGroup ag in groupList)
            {
                /*#if DEBUG 
                Log(this.part + " / " + ag.getPart());
                Log("{0}", this.part == ag.getPart());
                #endif*/
                if (ag.getPart() == this.part)
                {

                    if (ag.isThrottle)
                    {
                        FlightInputHandler.state.mainThrottle = ag.throttleLevel;
                        Log("Set throttle to {0} ({0:P2})", ag.throttleLevel);
                    }
                    else
                    {

                        if (ag.getMode() == ActionGroupFireStyle.ACTIVE_VESSEL || ag.getMode() == ActionGroupFireStyle.BOTH)
                            FlightGlobals.ActiveVessel.ActionGroups.ToggleGroup(aGroups[ag.getGroup()]);

                        if (ag.getMode() == ActionGroupFireStyle.CONNECTED_STAGE || ag.getMode() == ActionGroupFireStyle.BOTH)
                            this.part.vessel.ActionGroups.ToggleGroup(aGroups[ag.getGroup()]);
                        Log("Toggled group '" + aGroups[ag.getGroup()] + "' for part '" + this.part.name + "' in stage " + this.part.inverseStage);
                    }

                }

            }

        }

        public override void OnAwake()
        {
            if (isSceneVABOrSPH())
                InputLockManager.RemoveControlLock("AGOS_INPUT_LOCK");
        }

        public void OnDestroy()
        {
            if (guiOpen)
                toggleGUI();
            if (addGuiOpen)
                toggleAddGUI();
            clearGroupsForPart(this.part);
            // Remove locks on editor, if any
            if (isSceneVABOrSPH())
                EditorLogic.fetch.Unlock("AGOS_INPUT_LOCK");
        }

        public void Log(String message, params object[] data)
        {
            iPeerLib.Logging.Logger.Log(message, data);
        }

        public void LogWarning(String message, params object[] data)
        {
            iPeerLib.Logging.Logger.logWarning(message);
        }

        public void LogError(Exception e)
        {
            iPeerLib.Logging.Logger.LogError(e);
        }


        public override void OnSave(ConfigNode node)
        {

            node.AddValue("version", configVersion.ToString());

            foreach (ActionGroup ag in groupList)
            {
                if (ag.getPart() == this.part)
                {

                    string groupName;
                    if (ag.isThrottle)
                        groupName = "throttle";
                    else
                        groupName = aGroups[ag.getGroup()].ToString().ToLower();

                    if (ag.getMode() == ActionGroupFireStyle.ACTIVE_VESSEL)
                    {

                        if (!node.HasNode("AGOS_ACTIVE_VESSEL"))
                            node.AddNode("AGOS_ACTIVE_VESSEL");
                        node.GetNode("AGOS_ACTIVE_VESSEL").AddValue(groupName, ag.toSavableString());

                    }
                    else if (ag.getMode() == ActionGroupFireStyle.CONNECTED_STAGE)
                    {

                        if (!node.HasNode("AGOS_CONNECTED_STAGE"))
                            node.AddNode("AGOS_CONNECTED_STAGE");
                        node.GetNode("AGOS_CONNECTED_STAGE").AddValue(groupName, ag.toSavableString());

                    }
                    else
                    {

                        if (!node.HasNode("AGOS_BOTH"))
                            node.AddNode("AGOS_BOTH");
                        node.GetNode("AGOS_BOTH").AddValue(groupName, ag.toSavableString());

                    }

                }
            }

        }

        public override void OnLoad(ConfigNode node)
        {

            // Fix for Nullref when reloading parts via the debug menu
            if (!Utils.isLoadedSceneOneOf(GameScenes.FLIGHT, GameScenes.EDITOR/*, GameScenes.SPH*/))
                return;

            clearGroupsForPart(this.part);

            int saveVersion = 0;

            if (node.HasValue("version"))
                saveVersion = Convert.ToInt32(node.GetValue("version"));

#if DEBUG
            Log("Save Version is " + saveVersion);
#endif

            if (saveVersion >= 2)
            { // New saving style

                for (int x = 0; ; x++)
                {

                    string nodeName = "AGOS_BOTH";

                    if (x == 0)
                        nodeName = "AGOS_ACTIVE_VESSEL";
                    else if (x == 1)
                        nodeName = "AGOS_CONNECTED_STAGE";
                    else if (x == 2)
                        nodeName = "AGOS_BOTH";
                    else
                        break;

                    if (!node.HasNode(nodeName))
                        continue;

#if DEBUG
                    Log("Parsing entries for " + nodeName + " (" + Utils.ParseEnum<ActionGroupFireStyle>(nodeName) + ")");
#endif

                    ConfigNode n = node.GetNode(nodeName);
                    float throttleValue = 0f;
                    bool isThrottleController = false;
                    int numEntries = n.values.Count;
                    if (numEntries < 1)
                    {
                        Log("{0} has no entries, skipping.", nodeName);
                        continue;
                    }
                    for (int i = 0; i < numEntries; i++)
                    {
                        string valueName = n.values[i].name;
                        int group = 0;
                        string valueValue = n.GetValue(valueName);
#if DEBUG
                        Log("Node '{0}' = {1}", valueName, valueValue);
#endif
                        if (saveVersion == 2)
                        {
                            string[] values = valueValue.Split(',');
#if DEBUG
                            Log("0 = {0}", values[0]);
#endif
                            try
                            {
                                group = Convert.ToInt32(values[0].ToString().Trim());
                            }
                            catch (Exception e)
                            {
                                Logger.Log("An error has occured when trying to parse a version 2 node:", LogLevel.ERROR);
                                LogError(e);
                            }
                        }
                        else
                        {
                            if (valueName == "stage")
                                LogWarning("Usage of the 'Stage' action group is not recommended!");
                            if (valueName == "throttle")
                            {
                                Log("Parsing throttle control value");
                                isThrottleController = true;
                                throttleValue = Convert.ToSingle(valueValue.Split(',')[1], CultureInfo.InvariantCulture);
                                group = Convert.ToInt32(valueValue.Split(',')[0]);
                                if (throttleValue > 1f)
                                {
                                    LogWarning("Loaded throttle value is >1f, resetting it to 1f.");
                                    throttleValue = 1f;
                                }
                                if (throttleValue < 0f)
                                {
                                    LogWarning("Loaded throttle value is <0f, resetting it to 0f.");
                                    throttleValue = 0f;
                                }
                            }
                            else
                            {
                                isThrottleController = false;
                                group = Convert.ToInt32(valueValue);
                            }
                        }
                        ActionGroupFireStyle fireStyle = Utils.ParseEnum<ActionGroupFireStyle>(nodeName);
                        ActionGroup g = new ActionGroup(this.part, group, fireStyle, isThrottleController);
                        if (isThrottleController)
                            g.throttleLevel = throttleValue;
                        if (!isDuplicate(g))
                            groupList.Add(g);
                    }

                }

            }
            else
            { // Old saving style

                Log("Old config version detected");
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
                commitActionGroups(); // Convert to new style after loading older vessels

            }

            Log("Loaded Action Group config for part '" + this.part.name + "' ('" + this.part.partInfo.title + "'/" + this.part.GetInstanceID() + ") in stage " + this.part.inverseStage);

        }

        // GUI STUFF

        #region GUI

        private void OnDraw()
        {
            if (hideMainDialogue)
                return;

            if (!hasInitStyles)
            {
                hasInitStyles = true;

                GUISkin skinRef = Utils.getBestAvailableSkin();

                _windowStyle = new GUIStyle(skinRef.window);
                _windowStyle.fixedWidth = 500f;
                _labelStyle = new GUIStyle(skinRef.label);
                _labelStyle.stretchWidth = true;
                _labelStyleCentre = new GUIStyle(skinRef.label);
                _labelStyleCentre.alignment = TextAnchor.MiddleCenter;
                _toggleStyle = new GUIStyle(skinRef.toggle);
                _toggleStyle.stretchWidth = true;
                _buttonStyle = new GUIStyle(skinRef.button);
                _buttonStyle.stretchWidth = true;
                _scrollStyle = new GUIStyle(skinRef.scrollView);
                _groupButtonStyle = new GUIStyle(skinRef.button);
                _groupButtonStyle.stretchWidth = false;
                _addWindowStyle = new GUIStyle(skinRef.window);
                _addWindowStyle.fixedWidth = 500f;
                _addWindowStyle.fixedHeight = 250f;
                _labelStyleModeLabel = new GUIStyle(skinRef.label);
                _labelStyleModeLabel.stretchWidth = false;
                _sliderStyle = new GUIStyle(skinRef.horizontalScrollbar);
                _sliderStyleSlider = skinRef.horizontalSlider;
                _sliderStyleThumb = skinRef.horizontalSliderThumb;

#if DEBUG

                Log("Skin: " + skinRef.name + " (" + Utils.CURRENT_SKIN_INDEX + ")");

#endif

            }

            _windowPos = GUILayout.Window(+this.part.GetInstanceID(), _windowPos, OnWindow, "Action Group Configuration", _addWindowStyle);
            if (_windowPos.x == 0f && _windowPos.y == 0f)
            {
                _windowPos.x = Screen.width / 2 - _windowPos.width / 2;
                _windowPos.y = Screen.height / 2 - _windowPos.height / 2;
            }
        }

        private void OnWindow(int id)
        {

            GUILayout.BeginHorizontal();
            GUILayout.Label("Below is a run down of all the action groups configured to fire with this part.", _labelStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            scrollPos = GUILayout.BeginScrollView(scrollPos, _scrollStyle);

            ActionGroup[] groups = groupList.ToArray();

            for (int x = 0; x < groups.Length; x++)
            {

                ActionGroup ag = groups[x];

                if (ag.getPart() == this.part)
                {

                    GUILayout.BeginHorizontal();

                    if (ag.isThrottle)
                    {
                        GUILayout.Label(String.Format("Set throttle to {0:P2}", ag.throttleLevel), _labelStyle);
                    }
                    else
                        GUILayout.Label(aGroups[ag.getGroup()].ToString(), _labelStyle);
                    GUILayout.Label(ag.getMode() == ActionGroupFireStyle.ACTIVE_VESSEL ? "Vessel" : ag.getMode() == ActionGroupFireStyle.CONNECTED_STAGE ? "Stage" : "All", _labelStyleModeLabel);

                    if (GUILayout.Button("Remove", _groupButtonStyle))
                        groupList.Remove(ag);

                    GUILayout.EndHorizontal();

                }

            }

            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Action Group(s)", _buttonStyle))
            {
                toggleAddGUI();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close", _buttonStyle))
            {
                if (addGuiOpen)
                    toggleAddGUI();
                toggleGUI();
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        private void OnDrawAddGroup()
        {
            if (this.vessel == FlightGlobals.ActiveVessel)
            {

                _windowPosAddGroup = GUILayout.Window(+this.part.GetInstanceID() + 1, _windowPosAddGroup, OnWindowAddGroup, "Action Group Control", _windowStyle);
                // Center the GUI if it is at 0,0
                if (_windowPosAddGroup.x == 0f && _windowPosAddGroup.y == 0f)
                {
                    _windowPosAddGroup.x = Screen.width / 2 - _windowPosAddGroup.width / 2;
                    _windowPosAddGroup.y = Screen.height / 2 - _windowPosAddGroup.height / 2;
                }
            }
        }

        private void OnWindowAddGroup(int winID)
        {

            GUILayout.BeginHorizontal();
            GUILayout.Label("Action group control for '" + this.part.partInfo.title + "'", _labelStyle);

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

            actionGroups["throttle"] = GUILayout.Toggle(actionGroups["throttle"], "Throttle Control", _toggleStyle);
            if (actionGroups["throttle"])
            {
                throttleLevel = GUILayout.HorizontalSlider(throttleLevel, 0f, 1f, _sliderStyleSlider, _sliderStyleThumb);
                GUILayout.Label(String.Format("{0:P2}", throttleLevel), _labelStyle);
            }


            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Toggle(groupMode == ActionGroupFireStyle.ACTIVE_VESSEL, "Active Vessel Only", _buttonStyle))
                groupMode = ActionGroupFireStyle.ACTIVE_VESSEL;

            if (GUILayout.Toggle(groupMode == ActionGroupFireStyle.CONNECTED_STAGE, "Connected Stage Only", _buttonStyle))
                groupMode = ActionGroupFireStyle.CONNECTED_STAGE;

            if (GUILayout.Toggle(groupMode == ActionGroupFireStyle.BOTH, "All Stages & Vessels", _buttonStyle))
                groupMode = ActionGroupFireStyle.BOTH;

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Action Group", _buttonStyle))
            {
                //clearGroupsForPart(this.part);
                commitActionGroups(groupMode, throttleLevel);
                toggleAddGUI();
            }
#if DEBUG

            if (GUILayout.Button(Utils.CURRENT_SKIN_INDEX + ": " + Utils.getSkinList()[Utils.CURRENT_SKIN_INDEX].name))
            {

                Utils.CURRENT_SKIN_INDEX++;
                if (Utils.CURRENT_SKIN_INDEX > Utils.getSkinList().Count)
                    Utils.CURRENT_SKIN_INDEX = -1;
                hasInitStyles = false;

            }
#endif

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        #endregion

        public void commitActionGroups()
        {
            commitActionGroups(0f);
        }

        public void commitActionGroups(float throttleValue)
        {
            commitActionGroups(ActionGroupFireStyle.ACTIVE_VESSEL, throttleValue);
        }


        public void commitActionGroups(ActionGroupFireStyle mode, float throttleValue)
        {

            string[] keys = new string[actionGroups.Keys.Count];
            actionGroups.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; i++)
            {

                if (actionGroups[keys[i]])
                {

                    actionGroups[keys[i]] = false;
                    ActionGroup g;
                    if (keys[i] == "throttle")
                    {
                        g = new ActionGroup(this.part, i, mode, true);
                        g.throttleLevel = throttleValue;
                    }
                    else
                        g = new ActionGroup(this.part, i, mode);
#if DEBUG
                    Log("Adding action group config for part '" + part.partInfo.title + "' (" + g.toSavableString() + ")");
#endif
                    if (!isDuplicate(g))
                        groupList.Add(g);

                }

            }

            groupMode = ActionGroupFireStyle.ACTIVE_VESSEL;

        }

        public void clearGroupsForPart(Part part)
        {

            ActionGroup[] groups = groupList.ToArray();
            for (int x = 0; x < groups.Length; x++)
            {

                ActionGroup ag = groups[x];

#if DEBUG
                Log("Removing action group config for part '" + part.partInfo.title + "' (" + ag.toSavableString() + ")");
#endif

                if (ag.getPart() == part)
                    groupList.Remove(ag);

            }

        }

        public bool isNotDuplicate(ActionGroup g)
        { // So lazy...
            return !isDuplicate(g);
        }


        public bool isDuplicate(ActionGroup g)
        {

            ActionGroup[] groups = groupList.ToArray();
            for (int x = 0; x < groups.Length; x++)
            {

                ActionGroup ag = groups[x];

                if (ag.getPartIID() == g.getPartIID() && ag.getGroup() == g.getGroup() && ag.getMode() == g.getMode())
                    return true;

            }
            return false;

        }

        public bool isSceneVABOrSPH()
        {
            return Utils.isLoadedSceneOneOf(GameScenes.EDITOR/*, GameScenes.SPH*/);

        }

    }
}

