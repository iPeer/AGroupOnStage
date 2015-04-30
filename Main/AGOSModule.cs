using AGroupOnStage.ActionGroups;
using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AGroupOnStage.Main
{
    [KSPModule("AGOS Controller")]
    public class AGOSModule : PartModule
    {

        private bool isRoot = false;

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, guiName = "Action group control")]
        private void toggleGUI() { AGOSMain.Instance.linkPart = this.part; AGOSMain.Instance.toggleGUI(true); }

        public override void OnAwake()
        {
            if (!AGOSUtils.isLoadedSceneOneOf(GameScenes.FLIGHT, GameScenes.EDITOR)) { return; } // Invalid scene
        }

        public AGOSModule setRoot() { this.isRoot = true; return this; }

        public override void OnSave(ConfigNode node)
        {

            try
            {
                AGOSModule am = AGOSMain.Instance.getMasterAGOSModule(this.part.vessel);
                if (am == this)
                    this.isRoot = true;
                else
                    this.isRoot = false;
                if (this.isRoot)
                    Logger.Log("Root AGOSModule is on part {0} ({1}/{2})", this.part.name, this.part.partInfo.title, this.part.partInfo.name);
            }
            catch { Logger.LogWarning("Caught exception on part awake (harmless)"); this.isRoot = true; }

            node.AddValue("isRoot", isRoot);
            if (!this.isRoot) { return; } // Only the root module can save
            if (AGOSMain.Instance.actionGroups.Count > 0)
            {
                node.AddNode("AGOS");
                ConfigNode node_agos = node.GetNode("AGOS");
                node_agos.AddNode("GROUPS");
                foreach (IActionGroup ag in AGOSMain.Instance.actionGroups)
                {
                    ConfigNode node_group = node_agos.GetNode("GROUPS");
                    AGOSUtils.printActionGroupInfo(ag);
                    string agName = "";
                    agName = ag.Group.ToString();
                    if (!node_group.HasNode(agName))
                        node_group.AddNode(agName);
                    ConfigNode node_current = node_group.GetNode(agName);
                    int n = 0;
                    while (node_current.HasNode(n.ToString()))
                        n++;
                    node_current.AddNode(n.ToString());
                    ConfigNode node_n = node_current.GetNode(n.ToString());
                    node_n.AddValue("groupType", ag.GetType().Name);
                    node_n.AddValue("isPartLocked", ag.isPartLocked);
                    if (ag.isPartLocked)
                        node_n.AddValue("partLink", /*String.Format("{0}_{1}", ag.linkedPart.name, ag.linkedPart.craftID)*/ag.partRef);
                    else
                        node_n.AddValue("stages", AGOSUtils.intArrayToString(ag.Stages, ","));
                    if (ag.GetType() == typeof(FineControlActionGroup))
                        node_n.AddValue("togglesFineControls", true);
                    if (ag.GetType() == typeof(StageLockActionGroup))
                        node_n.AddValue("locksStaging", true);
                    if (ag.GetType() == typeof(ThrottleControlActionGroup))
                    {
                        node_n.AddValue("changesThrottle", true);
                        node_n.AddValue("throttleLevel", ag.ThrottleLevel.ToString());
                    }
                    if (ag.GetType() == typeof(CameraControlActionGroup))
                    {
                        node_n.AddValue("changesCamera", true);
                        node_n.AddValue("cameraMode", ag.cameraMode.ToString());
                    }
                    

                }
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!AGOSUtils.isLoadedSceneOneOf(GameScenes.FLIGHT, GameScenes.EDITOR)) { return; } // Invalid scene
            //if (node.CountValues == 0 && node.CountNodes == 0) { return; } // No config to load
            this.isRoot = Convert.ToBoolean(node.GetValue("isRoot"));
            if (!this.isRoot) { return; } // Only the root module can load
            if (!node.HasNode("AGOS"))
            {
                Logger.Log("No config to load for this vessel.");
                return;
            }

            //AGOSUtils.resetActionGroupConfig(); // Clear the list and reset all settings if neccessary

            ConfigNode node_agos = node.GetNode("AGOS");
            ConfigNode node_groups = node_agos.GetNode("GROUPS");
            foreach (ConfigNode group in node_groups.nodes)
            {
                //Logger.Log("{0}, {1}, {2}", group.name, Int32.Parse(group.name), Convert.ToInt32(group.name));
                int groupID = Convert.ToInt32(group.name);
                int _id = 0;
                foreach (ConfigNode id in group.nodes)
                {
                    string groupType = id.GetValue("groupType");
                    IActionGroup ag;
                    if (groupType.Equals("FineControlActionGroup"))
                        ag = new FineControlActionGroup();
                    else if (groupType.Equals("CameraControlActionGroup"))
                        ag = new CameraControlActionGroup();
                    else if (groupType.Equals("StageLockActionGroup"))
                        ag = new StageLockActionGroup();
                    else if (groupType.Equals("ThrottleControlActionGroup"))
                        ag = new ThrottleControlActionGroup();
                    else
                        ag = new BasicActionGroup();

                    // Error checking (for days!)

                    bool isPartLocked = Convert.ToBoolean(id.GetValue("isPartLocked"));

                    if (isPartLocked && !id.HasValue("partLink"))
                    {
                        Logger.LogWarning("Action group {0}:{1} requires a part reference but none has been supplied, skipping.", groupID, _id);
                        continue;
                    }

                    if (!isPartLocked && !id.HasValue("stages"))
                    {
                        Logger.LogWarning("Action group {0}:{1} is saved as type '{2}', is not partLinked and doesn't have a stage list, skipping.", groupID, _id, groupType);
                    }

                    if (!AGOSUtils.checkSavedGroupIsValid(id, groupType))
                    {
                        Logger.LogWarning("Action group {0}:{1} is saves as type '{2}' but is missing one or more flags required for its type, skipping.", groupID, _id, groupType);
                        continue;
                    }

                    //bool togglesFineControls = (id.HasNode("togglesFineControls") ? Convert.ToBoolean(id.GetValue("togglesFineControls")) : false);
                    //bool locksStaging = (id.HasNode("locksStaging") ? Convert.ToBoolean(id.GetValue("locksStaging")) : false);
                    bool changesCamera = (id.HasValue("changesCamera") ? Convert.ToBoolean(id.GetValue("changesCamera")) : false);
                    bool isThrottleControl = (id.HasValue("changesThrottle") ? Convert.ToBoolean(id.GetValue("changesThrottle")) : false);
                    float throttleLevel = (isThrottleControl ? Convert.ToSingle(id.GetValue("throttleLevel"), System.Globalization.CultureInfo.InvariantCulture) : 0f);

                    // Throttle sanity checks

                    if (isThrottleControl && throttleLevel > 1f)
                    {
                        Logger.LogWarning("Saved throttleLevel is over 1f.");
                        throttleLevel = 1f;
                    }
                    else if (isThrottleControl && throttleLevel < 0f)
                    {
                        Logger.LogWarning("Saved throttleLevel is less than 0");
                        throttleLevel = 0f;
                    }

                    if (isPartLocked)
                    {
                        /*Part part = AGOSUtils.findPartByReference(id.GetValue("partLink"), EditorLogic.fetch.ship.parts);
                        if (part == null)
                        {
                            Logger.LogWarning("Action group {0}:{1} supplied invalid part reference '{3}', skipping.", groupID, _id, id.GetValue("partLink"));
                            continue;
                        }
                        ag.linkedPart = part;
                        Logger.Log("Action group {0}:{1} and part '{2}' ({3}) have been reunited and are living happily ever after", groupID, _id, part.partInfo.title, String.Format("{0}_{1}", part.name, part.craftID));*/
                        ag.isPartLocked = isPartLocked;
                        ag.partRef = id.GetValue("partLink");
                    }
                    else
                    {
                        int[] stageList = id.GetValue("stages").Split(',').Select(a => int.Parse(a)).ToArray();
                        ag.Stages = stageList;
                    }

                    if (changesCamera)
                        ag.cameraMode = AGOSUtils.getCameramodeFromName(id.GetValue("cameraMode"));

                    if (isThrottleControl)
                        ag.ThrottleLevel = throttleLevel;

                    ag.Group = groupID;

                    AGOSMain.Instance.actionGroups.Add(ag);

                    _id++;

                }

            }

        }

        public void OnDestroy()
        {
            if (!AGOSUtils.isLoadedSceneOneOf(GameScenes.LOADING, GameScenes.LOADINGBUFFER, GameScenes.MAINMENU) && AGOSMain.Instance.guiVisible)
                AGOSMain.Instance.toggleGUI();
            //Dictionary<int, bool> ags = AGOSMain.Instance.actionGroupSettings
            //Logger.LogClassMethod(this, System.Reflection.MethodBase.GetCurrentMethod());
            //AGOSUtils.resetActionGroupConfig();
            if (!AGOSUtils.isLoadedSceneOneOf(GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPACECENTER))
                return;
            int removed = AGOSMain.Instance.actionGroups.RemoveAll(a => a.linkedPart != null && a.linkedPart == this.part);
            if (removed > 0)
                Logger.Log("Removed {0} action group(s) because its/their linked part was destroyed", removed);
        }

        public override string GetInfo()
        {
            return "Able to fire action groups on stage.";
        }
    }
}
