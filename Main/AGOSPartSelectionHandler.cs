using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{

    public enum PartSelectionMode
    {
        FOR_LINK,
        FOR_TASK
    }

    //[KSPAddon(KSPAddon.Startup.EditorAny, true)]
    //[KSPAddon(KSPAddon.Startup.Flight, true)]
    /// This class controls the selection of parts in the editors and flight mode from the user actually clicking them in a specific mode, rather than using AGOS' standard right click -> "Action group control"
    [KSPAddon(KSPAddon.Startup.EveryScene, false)] // Seeing as I can't make it just start in Editors and Flight (error CS0579: Duplicate 'KSPAddon' attribute) :/
    //[KSPAddon(KSPAddon.Startup.EditorAny | KSPAddon.Startup.Flight, false)] // Doesn't work, sadly
    public class AGOSPartSelectionHandler : MonoBehaviour
    {

        public bool partSelectModeActive { get; private set; }
        private Part selectedPart = null;
        private Part hoveredPart = null;
        private Part lastHoveredPart = null;
        private ScreenMessage selectMessage;
        private PartSelectionMode mode;
        private Color32 partHighlightColour;
        
        public static AGOSPartSelectionHandler Instance { get; protected set; }

        public void Start()
        {
            if (!AGOSUtils.isLoadedSceneOneOf(GameScenes.EDITOR, GameScenes.FLIGHT))
                return;
            if (Instance == null)
                Instance = this;
            Logger.Log("Part Selection Manager Startup");
            selectMessage = new ScreenMessage("Click which part you want to assign to the group configuration.\nPress 'Cancel' in the AGOS GUI or 'P' to cancel.", float.MaxValue, ScreenMessageStyle.UPPER_CENTER, AGOSMain.Instance._labelStyleRed);
            partHighlightColour = new Color32(AGOSMain.Settings.get<byte>("PartPickerColour-R"), AGOSMain.Settings.get<byte>("PartPickerColour-G"), AGOSMain.Settings.get<byte>("PartPickerColour-B"), 255);
        }

        public void OnDestroy()
        {
            if (Instance == null) { return; }
            Instance = null;
            Logger.Log("Part Selection Manager destroy");
        }

        public Part getHoveredPart()
        {
            return this.hoveredPart;
        }

        public Part getLastSelectedPart()
        {
            return this.selectedPart;
        }

        public void enterPartSelectionMode(PartSelectionMode mode = PartSelectionMode.FOR_LINK)
        {
            ScreenMessages.PostScreenMessage(selectMessage);
            this.mode = mode;
            this.partSelectModeActive = true;
        }

        public void exitPartSelectionMode()
        {
            ScreenMessages.RemoveMessage(selectMessage);
            this.partSelectModeActive = false;
            if (lastHoveredPart)
                this.lastHoveredPart.SetHighlightDefault();
            if (hoveredPart)
                this.hoveredPart.SetHighlightDefault();
        }

        public void Update()
        {
            if (this.partSelectModeActive)
            {
                // Update the highlighting
                Part currentlyHovered = AGOSUtils.getPartUnderCursor();
                if (currentlyHovered != this.hoveredPart)
                {
                    this.lastHoveredPart = this.hoveredPart;
                    this.hoveredPart = currentlyHovered;
                }
                if (Input.GetKey(KeyCode.P))
                {
                    exitPartSelectionMode();
                    return;
                }
                updatePartHighlight();
                clearHighlightsOnOtherParts();
                if (Input.GetKey(KeyCode.Mouse0)) // It's weird referring to the mouse buttons as "keys"
                {
                    if (currentlyHovered == null) { return; }
                    if (this.mode == PartSelectionMode.FOR_LINK)
                    {
                        AGOSMain.Instance.linkPart = currentlyHovered;
                        AGOSMain.Instance.isPartTriggered = true;
                        exitPartSelectionMode();
                    }
                    Logger.Log("{0} / {1}", currentlyHovered.name, currentlyHovered.partInfo.name);
                }
            }
        }

        private void clearHighlightsOnOtherParts()
        {
            (HighLogic.LoadedSceneIsEditor ? EditorLogic.fetch.ship.parts : FlightGlobals.fetch.activeVessel.parts).ForEach(
                p =>
                {
                    if (p != this.hoveredPart && p != this.lastHoveredPart)
                    {
                        p.SetHighlightDefault();
                    }
                });
        }

        private void updatePartHighlight()
        {
            if (this.lastHoveredPart != null)
            {
                this.lastHoveredPart.SetHighlightDefault();
            }
            if (this.hoveredPart != null)
            {
                //this.hoveredPart.SetHighlightType(Part.HighlightType.OnMouseOver);
                this.hoveredPart.SetHighlightColor(partHighlightColour);
                this.hoveredPart.SetHighlight(true, false);
            }
        }

    }
}
