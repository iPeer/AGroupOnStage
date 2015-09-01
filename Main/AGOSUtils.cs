using AGroupOnStage.ActionGroups;
using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using AGroupOnStage.Extensions;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Diagnostics;

namespace AGroupOnStage.Main
{
    public static class AGOSUtils
    {

        private static GUISkin currentSkin;
        private static FileVersionInfo fvi;

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

            { 0, "GameSkin(Clone)" },
			{ 1, "GameSkin" },
			{ 2, "Unity" }

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
            if (currentSkin != null) { return currentSkin; }
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
                        currentSkin = skinList[id];
                        return currentSkin;
                    }
                }
            }
            Logger.LogWarning("No preferred skin found, defaulting to HighLogic.Skin");
            currentSkin = HighLogic.Skin;
            return currentSkin;
        }

        public static string arrayToString<T>(T[] array, string separator = ", ")
        {
            StringBuilder r = new StringBuilder();
            foreach (T t in array)
            {
                r.Append((r.Length > 0 ? separator : "") + t.ToString());
            }
            return r.ToString();
        }

        public static string intArrayToString(int[] array, string separator = ", ")
        {
            /*if (array == null)
                return "";
            string returnString = "";
            foreach(int o in array)
            {
                returnString = returnString + (returnString.Length > 0 && separator != null ? separator : "") + o.ToString();
            }
            return returnString;*/
            return arrayToString<int>(array, separator);
        }

        public static List<Part> getVesselPartsList()
        {
            return (HighLogic.LoadedSceneIsEditor ? EditorLogic.fetch.ship.parts : FlightGlobals.fetch.activeVessel.parts);
        }

        public static string getActionGroupInfo(AGOSActionGroup ag)
        {
            return String.Format("[{7}] {0}, {1}, {2}, {3}, {4}, {5}, {6}", ag.Group.ToString(), (ag.Stages != null && ag.Stages.Length > 0 ? intArrayToString(ag.Stages, "|") : "none"), (ag.linkedPart != null ? ag.linkedPart.ToString() : "none"), ag.ThrottleLevel.ToString(), ag.cameraMode.ToString(), ag.isPartLocked, ag.partRef, ag.FlightID);
        }

        public static void printActionGroupInfo(AGOSActionGroup ag)
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
            else if (groupType.Equals(typeof(TimeDelayedActionGroup)) && Convert.ToBoolean(node.GetValue("firesDelayed")))
                return (node.HasValue("delay") && node.HasValue("firesGroupID"));
            else if (groupType.Equals(typeof(SASModeChangeGroup)))
                return node.HasValue("SASMode");
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

        /*public static void resetActionGroupConfig()
        {
            resetActionGroupConfig(true);
        }*/


        public static void resetActionGroupConfig(string source, bool clearCommited = false)
        {
            Logger.Log("Clearing action group config data (clear commited: {0}). Requested from '{1}'", clearCommited, source);
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

        public static uint getFlightID()
        {
            if (HighLogic.LoadedSceneIsEditor)
                return 0; // No flight IDs in the editor, for kind of obvious reasons, perhaps.
            else
                return FlightGlobals.ActiveVessel.rootPart.flightID;
        }


        /// <summary>
        /// Return if an action group is valid. Invalid groups are groups that contain stages greater than the vessel has, or references to parts that do not exist.
        /// </summary>
        /// <param name="ag">The Action Group to check</param>
        /// <returns>True if the group is valid, otherwise false</returns>
        public static bool isGroupValidForVessel(AGOSActionGroup ag)
        {

            if (!techLevelEnoughForGroup(ag.Group)) // 2.0.9-dev2: Mark parts that have higher tech requirement than the player currently has.
                return false;

            if (ag.FireType == AGOSActionGroup.FireTypes.DOCK || ag.FireType == AGOSActionGroup.FireTypes.UNDOCK)
                return true;

            List<Part> parts = (HighLogic.LoadedSceneIsEditor ? EditorLogic.fetch.ship.parts : FlightGlobals.fetch.activeVessel.parts);
            if (ag.isPartLocked)
            {

                if (ag.linkedPart == null)
                    return false;
                return parts.Contains(ag.linkedPart);

            }
            else
            {
                int stages = /*(HighLogic.LoadedSceneIsEditor ? Staging.lastStage : FlightGlobals.fetch.activeVessel.currentStage - 1);*/Staging.lastStage;
                if (ag.Stages == null || ag.Stages.Length == 0)
                    return false;
                return ag.Stages.Count(a => a > stages || a < 0) == 0;
            }

        }

        public static bool hasOutOfRangeStageConfig()
        {
            foreach (AGOSActionGroup ag in AGOSMain.Instance.actionGroups)
            {

                if (ag.isPartLocked)
                    continue;
                if (ag.Stages.Count(a => a > Staging.lastStage || a < 0) > 0)
                    return true;

            }
            return false;
        }

        public static bool hasInvalidPartLinkedConfig()
        {
            List<Part> parts = (HighLogic.LoadedSceneIsEditor ? EditorLogic.fetch.ship.parts : FlightGlobals.fetch.activeVessel.parts);
            foreach (AGOSActionGroup ag in AGOSMain.Instance.actionGroups)
            {
                if (!ag.isPartLocked)
                    continue;
                if (!parts.Contains(ag.linkedPart))
                    return true;
            }
            return false;
        }

        public static string getDLLPath()
        {
            return new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
        }

        // I'm amazed that KSP doesn't have an exposed method for this, and if it does, it's well hidden...
        public static Texture2D loadTextureFromDDS(string path, TextureFormat txf = TextureFormat.DXT1)
        {

            if (!(new TextureFormat[] { TextureFormat.DXT1, TextureFormat.DXT5}).Contains(txf))
                throw new InvalidOperationException("Supplied image format is not a DDS format.");

            byte[] imageData = System.IO.File.ReadAllBytes(path);
            if (imageData[4] != 124)
                throw new InvalidOperationException("File is not a valid DDS file");

            int width = imageData[13] * 256 + imageData[12];
            int height = imageData[17] * 256 + imageData[16];

            int HEADER_SIZE = 128; // Header for DDS files is 128 bytes

            byte[] dds = new byte[imageData.Length - HEADER_SIZE]; // Create buffer for the image
            Buffer.BlockCopy(imageData, HEADER_SIZE, dds, 0, imageData.Length - HEADER_SIZE); // Copy DDS' data to an array, without the headers

            // Create a new Texture2D to hold the image
            Texture2D tex = new Texture2D(width, height, txf, false);
            tex.LoadRawTextureData(dds); // Load the data into the image
            tex.Apply(); // Apply changes
            return tex;

        }


        public static bool isLoadedCraftID(uint p)
        {
            if (HighLogic.LoadedSceneIsEditor)
                return true;
            return p == getFlightID();
        }

        public static void runVoidMethodDelayed(Action method, double delay)
        {
            Timer t = new Timer();
            t.AutoReset = false;
            t.Interval = delay;
            t.Elapsed += (sender, e) => delayedVoidMethodTrigger(sender, e, method, t);
            t.Enabled = true;
            t.Start();
        }

        private static void delayedVoidMethodTrigger(object source, ElapsedEventArgs e, Action method, Timer timer)
        {
            timer.Stop();
            timer.Dispose();
            method.Invoke();
        }

        public static string getModVersion()
        {
            if (fvi == null)
            {
                AssemblyLoader.LoadedAssembly agos = AssemblyLoader.loadedAssemblies.First(a => a.name.Equals("AGroupOnStage"));
                fvi = FileVersionInfo.GetVersionInfo(agos.assembly.Location);
            }
            string version = fvi.FileVersion;
            if (version.EndsWith(".0.0"))
                return version.Substring(0, 3);
            else if (version.EndsWith(".0"))
                return version.Substring(0, 5);
            else
                return version;
        }

        // Thanks to KospY and KIS for showing me how to do this!
        // https://github.com/KospY/KIS/blob/86387f0f43ab1c1e6282f90bc76b471c596178ae/Plugins/Source/KIS_Shared.cs#L60
        public static Part getPartUnderCursor()
        {
            Part part = null;
            Camera cam = null;
            RaycastHit hit;

            if (HighLogic.LoadedSceneIsFlight)
                cam = FlightCamera.fetch.mainCamera;
            else if (HighLogic.LoadedSceneIsEditor)
                cam = EditorLogic.fetch.editorCamera;
            else
            {
                Logger.LogError("Current scene ('{0}') is not a valid scene for getting parts!", HighLogic.LoadedScene);
                return null;
            }


            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 1000, 481651))
            {
                part = (Part)UIPartActionController.GetComponentUpwards("Part", hit.transform.gameObject);
            }

            return part;

        }

        public static VesselAutopilot.AutopilotMode getSASModeForID(int id)
        {

            /*
                StabilityAssist = 0,
                Prograde = 1,
                Retrograde = 2,
                Normal = 3,
                Antinormal = 4,
                RadialIn = 5,
                RadialOut = 6,
                Target = 7,
                AntiTarget = 8,
                Maneuver = 9,
             */
            switch (id)
            {
                case 1:
                    return VesselAutopilot.AutopilotMode.Prograde;
                case 2:
                    return VesselAutopilot.AutopilotMode.Retrograde;
                case 3:
                    return VesselAutopilot.AutopilotMode.Normal;
                case 4:
                    return VesselAutopilot.AutopilotMode.Antinormal;
                case 5:
                    return VesselAutopilot.AutopilotMode.RadialIn;
                case 6:
                    return VesselAutopilot.AutopilotMode.RadialOut;
                case 7:
                    return VesselAutopilot.AutopilotMode.Target;
                case 8:
                    return VesselAutopilot.AutopilotMode.AntiTarget;
                case 9:
                    return VesselAutopilot.AutopilotMode.Maneuver;
                default:
                    return VesselAutopilot.AutopilotMode.StabilityAssist;
            }
        }

    }
}
