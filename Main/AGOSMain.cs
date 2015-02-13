using AGroupOnStage.ActionGroups;
using AGroupOnStage.AGX;
using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class AGOSMain : MonoBehaviour
    {

        #region public vars

        public static AGOSMain Instance { get; protected set; } // Protected set in case we need/want to extend this class in the future

        #endregion

        #region private vars

        private bool launcherButtonAdded = false;
        private bool useAGXConfig = false;
        private List<IActionGroup> actionGroups = new List<IActionGroup>();
        private string[] stockAGNames;
        private KSPActionGroup[] stockAGList;
        private Dictionary<int, KSPActionGroup> stockAGMap;
        private ApplicationLauncherButton agosButton = null;

        #endregion

        #region initialization

        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSMain.Start()");
            Instance = this;
            useAGXConfig = AGXInterface.isAGXInstalled();
            Logger.Log("This install is " + (useAGXConfig ? "" : "not ") + "running AGX.");
            if (useAGXConfig) { /* DO NAAHTHING! */ } // AGX is installed - use its controller, not stock's
            else // Not installed - fall back to stock controller.
                loadStockActionGroups();
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
        }

        private void OnGUIApplicationLauncherReady()
        {
            setupToolbarButton();
        }

        private void loadStockActionGroups()
        {
            stockAGMap = new Dictionary<int, KSPActionGroup>();
            Logger.Log("Loading stock KSP Action group list");
            stockAGNames = Enum.GetNames(typeof(KSPActionGroup)); // get ag names
            stockAGList = (KSPActionGroup[])Enum.GetValues(typeof(KSPActionGroup));
            for (int x = 0; x < stockAGNames.Length; x++)
            {
                Logger.Log("\tAction group {0}: {1} vs. {2}", x, stockAGNames[x], stockAGList[x]);
                stockAGMap.Add(x, stockAGList[x]);
            }
            Logger.Log("Done loading stock action group list");
        }

        #endregion

        #region GUI

        private void setupToolbarButton()
        {
            if (!launcherButtonAdded)
            {
                Logger.Log("Adding ApplicationLauncher button");
                agosButton = ApplicationLauncher.Instance.AddModApplication(
                    toggleGUI,
                    toggleGUI,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.VAB,
                    (Texture)GameDatabase.Instance.GetTexture("iPeer/AGroupOnStage/Textures/Toolbar", false)
                );
                launcherButtonAdded = true;
            }
            else
                Logger.logWarning("ApplicationLauncher button is already present (harmless)");

        }

        private void removeToolbarButton()
        {
            Logger.Log("Removing ApplicationLauncher button");
            ApplicationLauncher.Instance.RemoveModApplication(agosButton);
            launcherButtonAdded = false;
        }

        private void toggleGUI()
        {
            Logger.Log("AGOS.Main.AGOSMain.toggleGUI()");
            Logger.Log("{0}", EditorLogic.fetch.ship.parts.First().vessel.id);
        }

        private void OnGUI() { }
        private void OnWindow(int windowID) { }

        #endregion

        #region saving and loading

        public void OnSave(ConfigNode node)
        {
            Logger.Log("AGOS.Main.AGOSMain.OnSave()");
            Logger.Log("Vessel name is '{0}'", (HighLogic.LoadedSceneIsEditor ? EditorLogic.fetch.shipNameField.Text : FlightGlobals.fetch.activeVessel.vesselName));
        }

        public void OnLoad(ConfigNode node)
        {
            Logger.Log("AGOS.Main.AGOSMain.OnLoad()");
            Logger.Log("Vessel name is '{0}'", (HighLogic.LoadedSceneIsEditor ? EditorLogic.fetch.shipNameField.Text : FlightGlobals.fetch.activeVessel.vesselName)); // This is scary
        }

        #endregion

        #region misc. methods

        public AGOSModule getMasterAGOSModule(Vessel vessel)
        {
            List<Part> partList;
            if (HighLogic.LoadedSceneIsEditor)
                partList = EditorLogic.fetch.ship.parts;
            else if (vessel == null)
                partList = new List<Part>();
            else
                partList = vessel.Parts;

            return (from p in partList from m in p.Modules.OfType<AGOSModule>() select m).First();

        }

        #endregion

    }
}
