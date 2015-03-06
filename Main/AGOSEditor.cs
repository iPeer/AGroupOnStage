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
            /*GameEvents.onGameStateSave.Add(onGameStateSave);
            GameEvents.onGameStateLoad.Add(onGameStateLoad);
            /*GameEvents.onNewVesselCreated.Add(onNewVesselCreated);*/
            //GameEvents.onVesselLoaded.Add(AGOSMain.Instance.onVesselLoaded);
            //AGOSUtils.resetActionGroupConfig();
            GameEvents.onLevelWasLoaded.Add(onLevelWasLoaded);

        }

        private void onLevelWasLoaded(GameScenes data)
        {
            if (AGOSUtils.isLoadedSceneOneOf(GameScenes.EDITOR))
                AGOSMain.Instance.findHomesForPartLockedGroups(EditorLogic.fetch.ship.parts);
        }

        private void onVesselLoaded(Vessel data)
        {
            Logger.Log("Vessel loaded!");
            AGOSMain.Instance.findHomesForPartLockedGroups(data);
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
