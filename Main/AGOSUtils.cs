using AGroupOnStage.ActionGroups;
using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using AGroupOnStage.Extensions;

namespace AGroupOnStage.Main
{
    public static class AGOSUtils
    {

        public static bool isLoadedSceneOneOf(params GameScenes[] scenes)
        {
            foreach (GameScenes g in scenes)
            {
                if (HighLogic.LoadedScene == g)
                    return true;
            }
            return false;
        }

        public static Dictionary<int, GUISkin> getSkinList()
        {

            GUISkin[] skins = Resources.FindObjectsOfTypeAll(typeof(GUISkin)) as GUISkin[];
            Dictionary<int, GUISkin> guiSkins = new Dictionary<int, GUISkin>();
            int _skinID = 0;
            foreach (GUISkin _skin in skins)
                guiSkins.Add(_skinID++, _skin);
            return guiSkins;
        }

        public static Dictionary<int, string> preferredSkins = new Dictionary<int, string>() {

            { 0, "Unity" },
			{ 1, "GameSkin(Clone)" },
			{ 2, "GameSkin" }

		};

        public static FlightCamera.Modes getCameraModeForGroupID(int id)
        {
            switch (id)
            {
                case -3:
                    return FlightCamera.Modes.AUTO;
                case -4:
                    return FlightCamera.Modes.ORBITAL;
                case -5:
                    return FlightCamera.Modes.CHASE;
                case -6:
                    return FlightCamera.Modes.FREE;
                case -8:
                    return FlightCamera.Modes.LOCKED;
                default:
                    return FlightCamera.Modes.AUTO;
            }
        }

        public static GUISkin getBestAvailableSkin()
        {
            Dictionary<int, GUISkin> skinList = getSkinList();
            Logger.LogDebug("Skin list:");
            for (int x = 0; x < skinList.Count; x++)
            {
                Logger.LogDebug("\t{0}", skinList[x].name);
            }
            for (int __skinId = 0; __skinId < preferredSkins.Count; __skinId++)
            {
                string skinName = preferredSkins[__skinId];
                for (int id = 0; id < skinList.Count; id++)
                {
                    if (skinList[id].name == skinName)
                    {
                        return skinList[id];
                    }
                }
            }
            Logger.LogWarning("No preferred skin found, defaulting to HighLogic.Skin");
            return HighLogic.Skin;
        }

        public static string intArrayToString(int[] array, string separator)
        {
            string returnString = "";
            foreach(int o in array)
            {
                returnString = returnString + (returnString.Length > 0 && separator != null ? separator : "") + o.ToString();
            }
            return returnString;
        }

        public static List<Part> getVesselPartsList()
        {
            return (HighLogic.LoadedSceneIsEditor ? EditorLogic.fetch.ship.parts : FlightGlobals.fetch.activeVessel.parts);
        }

        public static string getActionGroupInfo(IActionGroup ag)
        {
            return String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}", ag.Group.ToString(), (ag.Stages != null && ag.Stages.Length > 0 ? intArrayToString(ag.Stages, "|") : "none"), (ag.linkedPart != null ? ag.linkedPart.ToString() : "none"), ag.ThrottleLevel.ToString(), ag.cameraMode.ToString(), ag.isPartLocked, ag.partRef);
        }

        public static void printActionGroupInfo(IActionGroup ag)
        {
            Logger.Log(getActionGroupInfo(ag));
        }

        public static bool checkSavedGroupIsValid(ConfigNode node, string groupType)
        {
            if (groupType.Equals(typeof(ThrottleControlActionGroup).Name))
                return (node.HasValue("changesThrottle") && Convert.ToBoolean(node.GetValue("changesThrottle")) && node.HasValue("throttleLevel"));
            else if (groupType.Equals(typeof(FineControlActionGroup).Name))
                return (node.HasValue("togglesFineControls") && Convert.ToBoolean(node.GetValue("togglesFineControls")));
            else if (groupType.Equals(typeof(StageLockActionGroup).Name))
                return (node.HasValue("locksStaging") && Convert.ToBoolean(node.GetValue("locksStaging")));
            else if (groupType.Equals(typeof(CameraControlActionGroup).Name))
                return (node.HasValue("changesCamera") && node.HasValue("cameraMode"));
            else
                return true;
        }

        public static Part findPartByReference(string _ref)
        {
            return findPartByReference(_ref, getVesselPartsList());
        }

        public static Part findPartByReference(string _ref, Vessel ves)
        {
            Logger.Log("{0}", ves == null);
            return findPartByReference(_ref, ves.parts);
        }

        public static Part findPartByReference(string _ref, List<Part> parts)
        {
            // TODO: Fix not returning part after revert to launch/editor redo
            //Logger.Log("{0}", parts.Count == 0 || parts == null);
            string[] refData = _ref.Split('_');
            //printArray<string>(refData);
            Part p = parts.Find(a => a.craftID == Convert.ToUInt32(refData[1]));
            //Part p = parts.Find(a => String.Format("{0}_{1}", a.name, a.craftID).Equals(_ref) || (a.name.Equals(refData[0]) && a.craftID == Convert.ToUInt32(refData[1])));
#if DEBUG
            if (p == null)
                Logger.Log("Part is null.");
            else
                Logger.Log("{0}, {1} / {2} | {3}, {4} / {5}", refData[0], p.name, p.name.Equals(refData[0]), refData[1], p.craftID, p.craftID == Convert.ToUInt32(refData[1]));
#endif
            return p;
        }

        public static FlightCamera.Modes getCameramodeFromName(string name)
        {
            foreach (FlightCamera.Modes a in Enum.GetValues(typeof(FlightCamera.Modes)))
            {
                Logger.Log("{0} / {1}", a.ToString(), name);
                if (a.ToString().Equals(name))
                    return a;
            }
            return FlightCamera.Modes.AUTO;
        }

        public static void resetActionGroupConfig()
        {
            resetActionGroupConfig(true);
        }


        public static void resetActionGroupConfig(bool clearCommited)
        {
            if (AGOSMain.Instance.actionGroups.Count() > 0 && clearCommited)
                AGOSMain.Instance.actionGroups.Clear();
            int[] keys = AGOSMain.Instance.actionGroupSettings.Keys.ToArray();
            foreach (int k in keys)
                AGOSMain.Instance.actionGroupSettings[k] = false;
        }

        public static void printArray<T>(T[] array)
        {
            Logger.Log("{0} item(s) in array:", array.Length);
            foreach (T i in array)
            {
                Logger.Log("\t{0}", i);
            }
        }

        public static float getTechLevel()
        {
            return getTechLevel(SpaceCenterFacility.VehicleAssemblyBuilding);
        }

        public static float getTechLevel(SpaceCenterFacility f)
        {
            return ScenarioUpgradeableFacilities.GetFacilityLevel(f);
        }

        public static bool techLevelEnoughForGroup(int group)
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER || (group >= 8 && AGOSMain.Instance.useAGXConfig))
                return true;
            if (group.isWithinRange(0, 7))
                return getTechLevel(SpaceCenterFacility.VehicleAssemblyBuilding) >= 0.5f;
            if (group > 7 && !AGOSMain.Instance.useAGXConfig)
                return getTechLevel(SpaceCenterFacility.VehicleAssemblyBuilding) >= 1f;
            return false;
        }

    }
}
