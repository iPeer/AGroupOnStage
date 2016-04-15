using AGroupOnStage.Logging;
using AGroupOnStage.Extensions;
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

        public static AGOSEditor Instance { get; protected set; }

        public void Start()
        {
            Logger.Log("AGOS.Main.AGOSEditor.Start()");
            if (Instance == null)
                Instance = this;
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
                GameEvents.onEditorUndo.Add(OnEditorUndo);
                GameEvents.onEditorRedo.Add(OnEditorRedo);
                GameEvents.onPartDestroyed.Add(onPartDestroyed);

                AGOSMain.Instance.EditorEventsRegistered = true;
                Logger.Log("Registered for Editor related GameEvents");
            }

        }

        private void onPartDestroyed(Part data)
        {
            if (!HighLogic.LoadedSceneIsEditor) { return; }
            int partsInScene = EditorLogic.fetch.CountAllSceneParts(true);
            //Logger.Log("Parts in scene: {0}", partsInScene); // Spammy on large vessels.
            if (partsInScene == 0)
                AGOSUtils.resetActionGroupConfig("Editor->onPartDestroyed", true);
        }

        private void onEditorRestart()
        {
            Logger.Log("Editor restart.");
            AGOSUtils.resetActionGroupConfig("Editor->onEditorRestart", true);
        }

        private void onEditorLoad(ShipConstruct data0, KSP.UI.Screens.CraftBrowserDialog.LoadType data1)
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

        /*public void updateFlightIDOnLoadComplete(AGOSModule m)
        {
            this.updateFlightID = true;
            this.rootAM = m;
        }*/

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

        /*public void OnUpdate()
        {

        }*/

        private void processUndoRedo(/*ProtoVessel v*/ShipConstruct data)
        {

            Logger.Log("Undo/Redo");

            // What a mess! Ain't nobody got time for that
            /*
            List<Part> parts = data.parts;
            List<Part> parts1 = pv.vesselRef.parts;

            for (int x = 0; x < parts.Count; x++)
            {
                Logger.LogDebug("{0} / {1}", parts[x].savedPartName(), parts1[x].savedPartName());
            }

            /*for (int x = 0; x < parts.Count; x++)
            {
                if (parts1[x].Equals(parts[x]))
                {
                    Logger.LogDebug(parts1)
                }
            }*/


            /* 
             * It's (currently) impossible for AGOS to correctly function through undo/redo for part locked parts due to how the game handles it
             * so we tell the user about this issue, and, if they want it, allow them to get more information on it
             */
            //AGOSUtils.resetActionGroupConfig();

            AGOSMain.Instance.findHomesForPartLockedGroups(data.parts);

            if (AGOSMain.Settings.get<bool>("ShowUndoWarning") && AGOSMain.Instance.actionGroups.Count(a => a.isPartLocked) > 0)
            {
                if (AGOSMain.Settings.get<bool>("LockInputsOnGUIOpen"))
                    AGOSInputLockManager.setControlLocksForScene(HighLogic.LoadedScene);
                DialogGUIButton[] options = new DialogGUIButton[]
                {
                    new DialogGUIButton("Okay", () => undoClick(0)),
                    new DialogGUIButton("Okay - don't show this again", () => undoClick(1)),
                    new DialogGUIButton("More Information", () => undoClick(2), false)
                };
                MultiOptionDialog mod = new MultiOptionDialog(
                    "It looks like you had groups which were locked to parts, but just tried to undo or redo something on your vessel." +
                    "Due to how the game handles undos and redos, it's not currently possible for AGOS to correctly process " +
                    "part locked configurations through these processes. Configurations using manually entered stage numbers will work as expected.\n\n" +
                    "For more information on this issue, please click the \"More Information\" button.", "AGroupOnStage", HighLogic.UISkin, options);
                PopupDialog.SpawnPopupDialog(mod, false, HighLogic.UISkin);
            }

        }

        private void OnGUI() /* 1.1 compatible GUI render code */
        {
            AGOSUtils.renderVisibleGUIs();
        }

        private void OnEditorRedo(ShipConstruct data)
        {

            //processUndoRedo(new ProtoVessel(ShipConstruction.backups.Last(), HighLogic.CurrentGame));
            processUndoRedo(data);

        }

        private void OnEditorUndo(ShipConstruct data)
        {

            OnEditorRedo(data);
            
        }

        private void undoClick(int opt)
        {
            if (opt == 1)
            {
                AGOSMain.Settings.set("ShowUndoWarning", false);
                AGOSMain.Settings.save();
            }
            else if (opt == 2)
            {
                throw new NotImplementedException();
            }
            if (opt < 2)
                AGOSInputLockManager.removeAllControlLocks();

        }
    }
}
