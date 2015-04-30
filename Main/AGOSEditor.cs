using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class AGOSEditor : MonoBehaviour
    {

        private int UPDATE_LIST_AFTER = 150; // ~5 seconds, 30 = 1 sec
        private int CURRENT_TICK_COUNT = 0;

        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSEditor.Start()");
            if (AGOSMain.Instance.EditorEventsRegistered)
                Logger.LogWarning("GameEvents for Editor are already registered (harmless)");
            else
            {
                /*GameEvents.onGameStateSave.Add(onGameStateSave);
                GameEvents.onGameStateLoad.Add(onGameStateLoad);
                /*GameEvents.onNewVesselCreated.Add(onNewVesselCreated);*/
                //GameEvents.onVesselLoaded.Add(AGOSMain.Instance.onVesselLoaded);
                //AGOSUtils.resetActionGroupConfig();
                GameEvents.onEditorRestart.Add(onEditorRestart);
                GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded); // Do you catch reverts? Yes you do! <3
                GameEvents.onEditorLoad.Add(onEditorLoad); // Does this do what I think (hope) it does?

                AGOSMain.Instance.EditorEventsRegistered = true;
                Logger.Log("Registered for Editor related GameEvents");
            }

        }

        private void onEditorRestart()
        {
            Logger.Log("Editor restart.");
            AGOSUtils.resetActionGroupConfig();
        }

        private void onEditorLoad(ShipConstruct data0, CraftBrowser.LoadType data1)
        {
            Logger.Log("AGOS.Main.AGOSEditor.onEditorLoad()");
            if (HighLogic.LoadedSceneIsEditor)
                AGOSMain.Instance.findHomesForPartLockedGroups(data0.parts);
        }

        private void onLevelWasLoaded(GameScenes data)
        {
            if (HighLogic.LoadedSceneIsEditor)
                AGOSMain.Instance.handleLevelLoaded(data);
        }

        private void onVesselLoaded(Vessel data)
        {
            Logger.Log("Vessel loaded!");
            AGOSMain.Instance.findHomesForPartLockedGroups(data);
            /*AGOSModule masterModule = AGOSMain.Instance.getMasterAGOSModule(data);
            if (masterModule == null)
            {
                Logger.LogWarning("There is no master AGOS Module for this craft!");
                AGOSModule agm = data.parts.First().Modules.GetModules<AGOSModule>().First().setRoot();
                Logger.Log("Root AGOSModule is on part {0} ({1}/{2})", agm.part.ToString(), agm.part.partInfo.title, agm.part.partInfo.name);
            }*/
        }

        /*private void onNewVesselCreated(Vessel data)
        {
            AGOSMain.Instance.actionGroups.Clear();
        }

        private void onGameStateLoad(ConfigNode data)
        {
            Logger.Log("Game load called");
        }

        private void onGameStateSave(ConfigNode data)
        {
            Logger.Log("Game save called");
        }

        public void OnSave(ConfigNode c) { AGOSMain.Instance.OnSave(c); }
        public void OnLoad(ConfigNode c) { AGOSMain.Instance.OnLoad(c); }*/

        public void OnUpdate()
        {
            /*CURRENT_TICK_COUNT++;
            if (CURRENT_TICK_COUNT >= UPDATE_LIST_AFTER)
            {
                CURRENT_TICK_COUNT = 0;
                AGOSMain.Instance.updatePartLockedStages(true);
            }*/
        }
    }
}
