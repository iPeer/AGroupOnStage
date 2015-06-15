using AGroupOnStage.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{

    public class ActionPart
    {

        public Part Part { get; set; }
        public BaseAction Action { get; set; }

    }

    public class AGOSGroupManager : MonoBehaviour 
    {

        private bool guiVisible = false;
        private Vector2 _scrollPos = Vector2.zero;
        private Rect _winPos = new Rect();
        private GUIStyle _windowStyle;
        private bool hasSetUpStyles = false;

        #region GUI

        public void toggleGUI()
        {
            if (!this.guiVisible)
            {

                _winPos.x = AGOSMain.Settings.get<float>("wPosGX");
                _winPos.y = AGOSMain.Settings.get<float>("wPosGY");
                RenderingManager.AddToPostDrawQueue(AGOSMain.AGOS_GROUP_LIST_WINDOW_ID, OnDraw);
                this.guiVisible = true;

            }
            else
            {
                this.guiVisible = false;
                RenderingManager.RemoveFromPostDrawQueue(AGOSMain.AGOS_GROUP_LIST_WINDOW_ID, OnDraw);
                AGOSMain.Settings.set("wPosGX", _winPos.x);
                AGOSMain.Settings.set("wPosGY", _winPos.y);
                AGOSMain.Settings.save();
                AGOSInputLockManager.removeControlLocksForSceneDelayed(HighLogic.LoadedScene, AGOSMain.AGOS_MANAGER_GUI_NAME);
            }

        }

        public void OnDraw() 
        {

            if (!AGOSMain.Instance.hasSetupStyles)
                AGOSMain.Instance.setUpStyles();
            if (!this.hasSetUpStyles)
            {
                GUISkin skin = AGOSUtils.getBestAvailableSkin();
                _windowStyle = new GUIStyle(skin.window);
            }

            _winPos = GUILayout.Window(AGOSMain.AGOS_GROUP_LIST_WINDOW_ID, _winPos, OnWindow, "Action Group Overview", _windowStyle);

            Vector3d realMousePos = Input.mousePosition;
            realMousePos.y = Screen.height - Input.mousePosition.y; // <- What the hell is this crap? Why is it a thing?

            if (AGOSMain.Settings.get<bool>("LockInputsOnGUIOpen"))
            {
                if (_winPos.Contains(realMousePos))
                    AGOSInputLockManager.setControlLocksForScene(HighLogic.LoadedScene, AGOSMain.AGOS_MANAGER_GUI_NAME);
                else
                {
                    AGOSInputLockManager.removeControlLocksForScene(HighLogic.LoadedScene, AGOSMain.AGOS_MANAGER_GUI_NAME);
                }
            }

        }
        public void OnWindow(int id) 
        {

            GUILayout.BeginVertical();


            _scrollPos = GUILayout.BeginScrollView(_scrollPos, AGOSMain.Instance._scrollStyle, GUILayout.MinWidth(400f), GUILayout.MinHeight(300f));


            Dictionary<string, List<ActionPart>> actionsList = getVesselGroupConfig();
            if (actionsList.Count == 0)
                GUILayout.Label("No action groups configured for this vessel", AGOSMain.Instance._labelStyle);
            else
            {

                GUILayout.BeginVertical();

                foreach (string s in actionsList.Keys)
                {

                    if (GUILayout.Button(String.Format("{0} ({1})", s, actionsList[s].Count), AGOSMain.Instance._buttonStyle))
                    {
                        if (HighLogic.LoadedSceneIsEditor)
                        {
                            highlightPartsFromList(actionsList[s]);
                        }
                        else if (HighLogic.LoadedSceneIsFlight)
                        {
                            toggleGroupForName(s);
                        }
                    }
                    /*int n = 1;
                    string text = String.Empty;
                    string lastString = String.Empty;*/
                    Dictionary<string, int> groups = new Dictionary<string, int>();
                    foreach (ActionPart ap in actionsList[s])
                    {
                        string groupText = String.Format("{0}: {1}", ap.Part.partInfo.title, ap.Action.guiName);
                        if (groups.ContainsKey(groupText))
                            groups[groupText]++;
                        else
                            groups.Add(groupText, 1);
                        /*if (compare.Equals(lastString))
                            n++;
                        else
                        {
                            text = lastString;
                            lastString = compare;
                            n = 1;
                        }

                    }*/

                        //GUILayout.Label(String.Format("\t{0} - {1}", ap.Part.partInfo.title, ap.Action.guiName), AGOSMain.Instance._labelStyle);
                    }
                    foreach (string z in groups.Keys) {
                        string _out = z;
                        if (groups[z] > 1)
                            _out += String.Format(" (x{0})", groups[z]);
                        GUILayout.Label(_out, AGOSMain.Instance._labelStyle);
                    }

                }

                GUILayout.EndVertical();

            }


            GUILayout.EndScrollView();

            if (GUILayout.Button("Close", AGOSMain.Instance._buttonStyle))
                toggleGUI();


            GUILayout.EndVertical();

            GUI.DragWindow();

        }

        public void toggleGroupForName(string name)
        {
            Logger.LogError("NOT YET IMPLEMENTED");

        }

        public void highlightPartsFromList(List<ActionPart> list)
        {


            foreach (ActionPart p in list)
            {

                Part part = p.Part;
                if (part.highlightType == Part.HighlightType.AlwaysOn) // Part is highlighted
                {
                    part.SetHighlightDefault();
                }
                else // Not highlighted
                {
                    part.SetHighlightColor(XKCDColors.Aqua);
                    part.SetHighlightType(Part.HighlightType.AlwaysOn);
                    part.SetHighlight(true, false);
                }

            }

        }

        #endregion

        public static IEnumerable<ActionPart> getPartsWithGroup(KSPActionGroup g)
        {

            List<ActionPart> ret = new List<ActionPart>();
            foreach (Part p in AGOSUtils.getVesselPartsList())
            {
                foreach (BaseAction a in p.Actions)
                    if (actionIsInGroup(a, g))
                        ret.Add(new ActionPart { Part = p, Action = a });
                foreach (PartModule m in p.Modules)
                    foreach (BaseAction a in m.Actions)
                        if (actionIsInGroup(a, g))
                            ret.Add(new ActionPart { Part = p, Action = a });
            }
            return ret;

        }

        public IEnumerable<KSPActionGroup> getGroupsForPart(Part p)
        {

            List<KSPActionGroup> ret = new List<KSPActionGroup>();

            foreach (KSPActionGroup g in Enum.GetValues(typeof(KSPActionGroup)))
            {

                if (g == KSPActionGroup.None)
                    continue;

                foreach (PartModule m in p.Modules)
                    foreach (BaseAction a in m.Actions)
                        if (actionIsInGroup(a, g) && !ret.Contains(g))
                            ret.Add(g);

            }

            return ret;

        }

        public static Dictionary<string, List<ActionPart>> getVesselGroupConfig()
        {

            Dictionary<string, List<ActionPart>> ret = new Dictionary<string, List<ActionPart>>();
            foreach (KSPActionGroup g in Enum.GetValues(typeof(KSPActionGroup)))
            {
                if (g == KSPActionGroup.None)
                    continue;

                List<ActionPart> parts = getPartsWithGroup(g).ToList<ActionPart>();
                if (parts.Count > 0)
                    ret.Add(g.ToString(), parts);

            }
            return ret;

        }

        public static Dictionary<Part, List<BaseAction>> getPartActionList()
        {

            Dictionary<string, List<ActionPart>> groupList = getVesselGroupConfig();

            // Build a sorted (kind of) list of parts

            Dictionary<Part, List<BaseAction>> actionList = new Dictionary<Part, List<BaseAction>>();

            foreach (string group in groupList.Keys)
            {
                List<ActionPart> apL = groupList[group];
                if (apL.Count == 0) // Sanity check
                    continue;
                foreach (ActionPart ap in apL)
                {
                    if (actionList.ContainsKey(ap.Part))
                    {
                        if (!actionList[ap.Part].Contains(ap.Action))
                            actionList[ap.Part].Add(ap.Action);
                    }
                    else
                    {
                        List<BaseAction> a = new List<BaseAction>();
                        a.Add(ap.Action);
                        actionList.Add(ap.Part, a);
                    }
                }
            }

            return actionList;

        }

        public static void dumpActionGroupConfig()
        {

            Dictionary<string, List<ActionPart>> groupList = getVesselGroupConfig();

            if (groupList.Count == 0)
            {
                Logger.Log("No groups for this vessel!");
                return;
            }

            // Build a sorted (kind of) list of parts

            Dictionary<Part, List<BaseAction>> actionList = new Dictionary<Part, List<BaseAction>>();

            foreach (string group in groupList.Keys)
            {
                List<ActionPart> apL = groupList[group];
                if (apL.Count == 0) // Sanity check
                    continue;
                foreach (ActionPart ap in apL)
                {
                    if (actionList.ContainsKey(ap.Part))
                    {
                        if (!actionList[ap.Part].Contains(ap.Action))
                            actionList[ap.Part].Add(ap.Action);
                    }
                    else
                    {
                        List<BaseAction> a = new List<BaseAction>();
                        a.Add(ap.Action);
                        actionList.Add(ap.Part, a);
                    }
                }
            }

            foreach (Part p in actionList.Keys)
            {
                List<BaseAction> apList = actionList[p];
                Logger.Log("{0}", p.partInfo.title);
                foreach (BaseAction b in apList)
                {
                    foreach (string s in b.actionGroup.ToString().Split(','))
                    {
                        Logger.Log("\t`--> {0} - {1} - {2}", s, b.guiName, b.ToString());
                    }

                }

            }

        }

        public static bool actionIsInGroup(BaseAction a, KSPActionGroup g)
        {
            if (a == null)
                return false;
            return (a.actionGroup & g) == g;
        }

    }
}
