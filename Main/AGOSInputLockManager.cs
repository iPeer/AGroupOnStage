using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.Main
{
    public class AGOSInputLockManager
    {

        const string AGOS_EDITOR_LOCK_NAME  = "AGOS_EDITOR_LOCK";
        const string AGOS_FLIGHT_LOCK_NAME  = "AGOS_FLIGHT_LOCK";
        const string AGOS_KSC_LOCK_NAME     = "AGOS_KSC_LOCK";
        const string AGOS_ASTRO_LOCK_NAME   = "AGOS_STRO_LOCK";
        const string AGOS_TRACKING_LOCK_NAME = "AGOS_TRACKING_LOCK"; // this guy ruined my tabs. I'll remember that for later.

        /*
         * All valid control locks as of version 1.0.2:
         * 
         * None, All, PITCH, ROLL, YAW, THROTTLE, SAS, PAUSE, STAGING, CAMERAMODES, MISC, CAMERACONTROLS, TIMEWARP, MAP, LINEAR, QUICKSAVE, QUICKLOAD, 
         * VESSEL_SWITCHING, CUSTOM_ACTION_GROUPS, GROUP_ABORT, GROUP_GEARS, GROUP_LIGHTS, GROUP_BRAKES, GROUP_STAGE, ACTIONS_SHIP, ACTIONS_EXTERNAL, RCS, 
         * WHEEL_STEER, WHEEL_THROTTLE, EVA_INPUT, EDITOR_ICON_HOVER, EDITOR_ICON_PICK, EDITOR_TAB_SWITCH, EDITOR_SAVE, EDITOR_LOAD, EDITOR_EXIT, EDITOR_NEW, 
         * EDITOR_LAUNCH, EDITOR_PAD_PICK_PLACE, EDITOR_PAD_PICK_COPY, EDITOR_GIZMO_TOOLS, EDITOR_ROOT_REFLOW, EDITOR_SYM_SNAP_UI, EDITOR_EDIT_STAGES, 
         * EDITOR_EDIT_NAME_FIELDS, EDITOR_UNDO_REDO, EDITOR_MODE_SWITCH, TRACKINGSTATION_UI, KSC_FACILITIES, KSC_UI, APPLAUNCHER_BUTTONS, MAIN_MENU, GUI, 
         * ALLBUTCAMERAS, GROUPS_ALL, ACTIONS_ALL, ALL_SHIP_CONTROLS, EDITOR_UI_TOPRIGHT, EDITOR_UI_TOPBAR, EDITOR_UI, EDITOR_LOCK, EDITOR_SOFT_LOCK, KSC_ALL, 
         * TRACKINGSTATION_ALL, TUTORIALWINDOW
         * 
         */

        /*
         * There's no documentation for this stuff anywhere - not even at https://anatid.github.io/XML-Documentation-for-the-KSP-API/_control_types_8cs.html (it 404s), 
         * so this is all just best-guess. I LOVE best-guess...
         * 
         */

        /* 
         * This file needs more of these magical green things.
         * It is a really nice green colour, though.
         * 
         */

        /// <summary>
        /// A list of locks to enforce when AGOS' GUI is open in the Flight scene.
        /// </summary>
        /* 
         * Click-through isn't such a big problem in flight mode (though it still exists); these are locked more for the player's sanity than click-through potential. 
         * Nobody wants to accidentally stage while tweaking their configs.
         */
        static readonly List<ControlTypes> flightLocks = new List<ControlTypes>() 
        {

            ControlTypes.THROTTLE,
            ControlTypes.STAGING,
            ControlTypes.PAUSE,
            ControlTypes.TIMEWARP,
            ControlTypes.CUSTOM_ACTION_GROUPS, // Custom01-10 groups?
            ControlTypes.GROUP_ABORT

        };

        /// <summary>
        /// A list of locks to enforce when AGOS' GUI is open at the Editor scene.
        /// </summary>
        /*
         * Clickthrough is a MASSIVE problem in the editor, so, as you can see, the controls locks are *a lot* more aggressive.
         */
        static readonly List<ControlTypes> editorLocks = new List<ControlTypes>()
        {

            // I could just use ControlTypes.EDITOR_UI, but I want some GUI elements to remain active, such as saving.

            ControlTypes.EDITOR_EDIT_NAME_FIELDS,
            ControlTypes.EDITOR_EDIT_STAGES,
            ControlTypes.EDITOR_EXIT,
            ControlTypes.EDITOR_GIZMO_TOOLS,
            ControlTypes.EDITOR_ICON_HOVER, // What is this?
            ControlTypes.EDITOR_ICON_PICK, // ^
            ControlTypes.EDITOR_LAUNCH,
            ControlTypes.EDITOR_LOAD,
            ControlTypes.EDITOR_NEW,
            ControlTypes.EDITOR_PAD_PICK_COPY,
            ControlTypes.EDITOR_PAD_PICK_PLACE,
            ControlTypes.EDITOR_ROOT_REFLOW,
            ControlTypes.EDITOR_UNDO_REDO

        };

        /// <summary>
        /// A list of locks to enforce when AGOS' (settings) GUI is open at the space centre scene.
        /// </summary>
        // Super simple stuff.
        static readonly List<ControlTypes> kscLocks = new List<ControlTypes>() 
        {
            ControlTypes.KSC_FACILITIES
        };

        /// <summary>
        /// A list of locks to enforce when AGOS' (settings) GUI is open at the astronaut complex scene.
        /// </summary>
        // There's no astronaut complex-specific ones, just just lock ALL the things! (iPeer's "I'm tired" note (05:39AM): This may be a bad idea)
        // TODO: Fix me. Probably.
        static readonly List<ControlTypes> astroLocks = new List<ControlTypes>()
        {

            ControlTypes.All

        };

        /// <summary>
        /// A list of locks to enforce when AGOS' (settings) GUI is open at the tracking station scene.
        /// </summary>
        // Super simple stuff
        static readonly List<ControlTypes> trackingLocks = new List<ControlTypes>()
        {

            ControlTypes.TRACKINGSTATION_UI

        };


        public static void lockFlightControls()
        {
        }

        public static void lockEditorControls() // Placeholder
        {
            if (EditorLogic.fetch == null)
            {
                Logger.LogError("Cannot apply editor locks!");
                return;
            }
            EditorLogic.fetch.Lock(true, true, true, AGOS_EDITOR_LOCK_NAME);
        }

        public static void removeFlightLocks()
        {

        }

        public static void removeEditorLocks() // Placeholder
        {
            if (EditorLogic.fetch == null)
            {
                Logger.LogError("Cannot remove editor locks!");
                return;
            }
            EditorLogic.fetch.Unlock(AGOS_EDITOR_LOCK_NAME);
        }

        public static void removeAllLocks() // Utility method
        {
            removeEditorLocks();
            removeFlightLocks();
        }

        public static void DEBUGListAllPossibleLocks() 
        {

            var type = typeof(ControlTypes);
            foreach (string s in type.GetFields().Select(x => x.Name))
                Logger.Log("{0}", s);

        }

    }
}
