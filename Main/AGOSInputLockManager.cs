using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using UnityEngine;

namespace AGroupOnStage.Main
{
    public class AGOSInputLockManager
    {

        const string AGOS_EDITOR_LOCK_NAME  = "AGOS_EDITOR_LOCK";
        const string AGOS_FLIGHT_LOCK_NAME  = "AGOS_FLIGHT_LOCK";
        const string AGOS_KSC_LOCK_NAME     = "AGOS_KSC_LOCK";
        const string AGOS_ASTRO_LOCK_NAME   = "AGOS_ASTRO_LOCK";
        const string AGOS_TRACKING_LOCK_NAME = "AGOS_TRACKING_LOCK"; // this guy ruined my tabs. I'll remember that for later.
        const string AGOS_DEBUG_LOCK_NAME   = "AGOS_DEBUG_LOCK";

        private static List<string> openGUIs = new List<string>();

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
         * Though on GitHub, it's grey. How boring.
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

            ControlTypes.THROTTLE, // Do I need to explain this one?
            ControlTypes.STAGING, // ^
            ControlTypes.PAUSE, // Not really vital, but it does make AGOS' GUI all wobbly and stuff.
            ControlTypes.TIMEWARP, // Not a big deal, but locked anyway for state of mind.
            ControlTypes.CUSTOM_ACTION_GROUPS, // Don't want to accidentally fire an action group while tweaking.
            ControlTypes.GROUP_ABORT // See the first comment.

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

            ControlTypes.EDITOR_EDIT_NAME_FIELDS, // I don't really know why I lock this
            ControlTypes.EDITOR_EDIT_STAGES, // Might make AGOS act all funny. Foresight!
            ControlTypes.EDITOR_EXIT, // Not really vital as of 1.0 (new confirmation), but locked anyway
            ControlTypes.EDITOR_GIZMO_TOOLS, // Locked to prevent the rootpart from being changed. Might cause funky stuff. -- DOESN'T ACTUALLY WORK. Squad pls.
            ControlTypes.EDITOR_ICON_HOVER, // Preverts the "info" pannel from parts appearing when hovered over in the parts list
            ControlTypes.EDITOR_ICON_PICK, // Stops parts from being picked from the parts list
            ControlTypes.EDITOR_LAUNCH, // I'll just tweak this gro-aw crap.
            ControlTypes.EDITOR_LOAD, // Because the hang is annoying.
            ControlTypes.EDITOR_NEW, // Might freak AGOS out if you're midway through editing a (part locked) group
            ControlTypes.EDITOR_PAD_PICK_COPY, // Guess: prevents accidental part copying with the GUI open.
            ControlTypes.EDITOR_PAD_PICK_PLACE, // Guess: Stops the player breaking everything by clicking their vessel and having the mouse grab a part. Possibly also locks the parts list.
            ControlTypes.EDITOR_ROOT_REFLOW, // I got nothin'
            ControlTypes.EDITOR_UNDO_REDO, // AGOS: Doesn't (can't) handle undo/redo as it is, who knows what will happen if you do it with teh GUI open...

        };

        /// <summary>
        /// A list of locks to enforce when AGOS' (settings) GUI is open at the space centre scene.
        /// </summary>
        // Super simple stuff.
        static readonly List<ControlTypes> kscLocks = new List<ControlTypes>() 
        {
            ControlTypes.KSC_FACILITIES // Because accidentally entering the VAB when trying to change a setting is (really) annoying.
        };

        /// <summary>
        /// A list of locks to enforce when AGOS' (settings) GUI is open at the astronaut complex scene.
        /// </summary>
        // There's no astronaut complex-specific ones, just just lock ALL the things! (iPeer's "I'm tired" note (05:39AM): This may be a bad idea)
        // TODO: Fix me. Probably.
        static readonly List<ControlTypes> astroLocks = new List<ControlTypes>()
        {

            ControlTypes.All // More for career. Prevent accidental hiring (or firing) of Kerbals.

        };

        /// <summary>
        /// A list of locks to enforce when AGOS' (settings) GUI is open at the tracking station scene.
        /// </summary>
        // Super simple stuff
        static readonly List<ControlTypes> trackingLocks = new List<ControlTypes>()
        {

            ControlTypes.TRACKINGSTATION_UI // I wanted to change a setting, not terminate a flight!

        };

        public static void setControlLocksForScene(GameScenes scene, string guiName)
        {
            registerGUIOpen(guiName);
            setControlLocksForScene(scene);
        }

        public static void setControlLocksForScene(GameScenes scene)
        {


            if (scene == GameScenes.EDITOR)
                applyLocks(AGOS_EDITOR_LOCK_NAME, editorLocks);
            else if (scene == GameScenes.FLIGHT)
                applyLocks(AGOS_FLIGHT_LOCK_NAME, flightLocks);
            else if (scene == GameScenes.SPACECENTER)
                applyLocks(AGOS_KSC_LOCK_NAME, kscLocks);
            else if (scene == GameScenes.TRACKSTATION)
                applyLocks(AGOS_TRACKING_LOCK_NAME, trackingLocks);
            else // So there's no scene for the astro complex? This could be problematic.
                applyLocks(AGOS_ASTRO_LOCK_NAME, astroLocks);

        }

        public static void removeControlLocksForScene(GameScenes scene, string guiName)
        {
            registerGUIClosed(guiName);
            removeControlLocksForScene(scene);
        }

        public static void removeControlLocksForScene(GameScenes scene)
        {
            if (scene == GameScenes.EDITOR)
                removeLocks(AGOS_EDITOR_LOCK_NAME, editorLocks);
            else if (scene == GameScenes.FLIGHT)
                removeLocks(AGOS_FLIGHT_LOCK_NAME, flightLocks);
            else if (scene == GameScenes.SPACECENTER)
                removeLocks(AGOS_KSC_LOCK_NAME, kscLocks);
            else if (scene == GameScenes.TRACKSTATION)
                removeLocks(AGOS_TRACKING_LOCK_NAME, trackingLocks);
            else // So there's no scene for the astro complex? This could be problematic.
                removeLocks(AGOS_ASTRO_LOCK_NAME, astroLocks);
        }

        public static void removeControlLocksForSceneDelayed(GameScenes scene, double delay, string guiName)
        {
            registerGUIClosed(guiName);
            removeControlLocksForSceneDelayed(scene, delay);
        }

        public static void removeControlLocksForSceneDelayed(GameScenes scene)
        {
            removeControlLocksForSceneDelayed(scene, AGOSMain.Settings.get<double>("LockRemovalDelay"));
        }

        public static void removeControlLocksForSceneDelayed(GameScenes scene, string guiName)
        {
            removeControlLocksForSceneDelayed(scene, AGOSMain.Settings.get<double>("LockRemovalDelay"), guiName);
        }

        public static void removeControlLocksForSceneDelayed(GameScenes scene, double delay) // Doesn't work - never fires -- 30 minute later "I changed nothing" edit: I STAND CORRECTED. IT WORKS!
        {
            Timer t = new Timer();
            t.Interval = delay;
            t.Elapsed += (sender, e) => delayedRemoveTrigger(sender, e, scene, t);
            t.Enabled = true;
            t.Start();

        }

        private static void delayedRemoveTrigger(object sender, ElapsedEventArgs e, GameScenes scene, Timer timer)
        {

            timer.Stop();
            timer.Dispose();
            removeControlLocksForScene(scene);

        }

        private static void applyLocks(string lockPrefix, List<ControlTypes> locks)
        {


            foreach (ControlTypes ct in locks)
            {
                string lockName = String.Format("{0}_{1}", lockPrefix, ct.ToString());
                if (AGOSMain.Settings.get<bool>("LogControlLocks"))
                    Logger.Log("Activating control lock '{0}'", lockName);
                InputLockManager.SetControlLock(ct, lockName);
            }

        }

        private static void removeLocks(string lockPrefix, List<ControlTypes> locks) 
        {
            if (openGUIs.Count > 1) { return; } // More than one AGOS GUI is open, don't remove the locks
            foreach (ControlTypes ct in locks)
            {
                string lockName = String.Format("{0}_{1}", lockPrefix, ct.ToString());
                if (InputLockManager.GetControlLock(lockName) == ControlTypes.None)
                    continue;
                if (AGOSMain.Settings.get<bool>("LogControlLocks"))
                    Logger.Log("Removing control lock '{0}'", lockName);
                InputLockManager.RemoveControlLock(lockName);
            }

        }

        public static void removeAllControlLocks()
        {
            removeControlLocksForScene(GameScenes.FLIGHT);
            removeControlLocksForScene(GameScenes.EDITOR);
            removeControlLocksForScene(GameScenes.SPACECENTER);
            removeControlLocksForScene(GameScenes.TRACKSTATION);
            removeControlLocksForScene(GameScenes.LOADING); // We don't actually lock anything on loading scenes, but this will remove locks that aren't for any specified scene (in most cases Astronaut complex locks) - tl;dr: failsafe
        }

        public static void registerGUIOpen(string guiName)
        {
            if (!openGUIs.Contains(guiName))
                openGUIs.Add(guiName);
        }

        public static void registerGUIClosed(string guiName)
        {
            if (openGUIs.Contains(guiName))
                openGUIs.Remove(guiName);
        }

        public static void DEBUGListAllPossibleLocks() 
        {

            var type = typeof(ControlTypes);
            foreach (string s in type.GetFields().Select(x => x.Name))
                Logger.Log("{0}", s);

        }

        public static void DEBUGdrawLockButtons()
        {

            var type = typeof(ControlTypes);
            foreach (string s in type.GetFields().Select(x => x.Name))
            {
                if (new string[] { "value__", "None" }.Contains(s))
                    continue;
                bool lockEnabled = InputLockManager.GetControlLock(String.Format("{0}_{1}", AGOS_DEBUG_LOCK_NAME, s)) != ControlTypes.None;
                //DEBUGtoggleDebugLock(s, GUILayout.Toggle(lockEnabled, s + ": " + lockEnabled, AGOSMain.Instance._buttonStyle));
                if (GUILayout.Button(s + ": " + lockEnabled))
                {
                    DEBUGtoggleDebugLock(s);
                }

            }

        }

        static ControlTypes DEBUGgetLockForName(string n)
        {

            foreach (ControlTypes ct in Enum.GetValues(typeof(ControlTypes)))
            {
                if (ct.ToString().Equals(n))
                    return ct;
            }
            return ControlTypes.None;

        }

        static void DEBUGtoggleDebugLock(string s/*, bool ignored*/)
        {

            ControlTypes _lock = DEBUGgetLockForName(s);

            if (InputLockManager.GetControlLock(String.Format("{0}_{1}", AGOS_DEBUG_LOCK_NAME, _lock.ToString())) != ControlTypes.None)
            {
                InputLockManager.RemoveControlLock(String.Format("{0}_{1}", AGOS_DEBUG_LOCK_NAME, _lock.ToString()));
            }
            else
            {
                InputLockManager.SetControlLock(_lock, String.Format("{0}_{1}", AGOS_DEBUG_LOCK_NAME, _lock.ToString()));
            }

        }

        public static void DEBUGListActiveLocks()
        {

            List<string> locks = new List<string>(InputLockManager.lockStack.Where(a => a.Key.StartsWith("AGOS_")).Select(b => b.Key));

            Logger.Log("AGOS currently has {0} active lock(s)", locks.Count);
            int x = 0;
            foreach (string s in locks)
            {
                Logger.Log("\t{0}: {1} ({2})", x++, s, InputLockManager.lockStack[s]);
            }

        }

    }
}
