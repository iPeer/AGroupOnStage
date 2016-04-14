using AGroupOnStage.Logging;
using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Tests
{
    [KSPAddon(KSPAddon.Startup.EditorAny, true)]
    public class KSP11GUIAppLauncherTest : MonoBehaviour
    {

        bool guiVisible = true; 

        public void Start()
        {
            Logger.Log("KSP11GUIAppLauncherTest Start");
            ApplicationLauncherButton agosButton = ApplicationLauncher.Instance.AddModApplication(
                /*() => AGOSMain.Instance.toggleGUI()*/showGUI,
                /*() => AGOSMain.Instance.toggleGUI()*/hideGUI,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.SPACECENTER,
                //ApplicationLauncher.AppScenes.ALWAYS,
                    new Texture2D(64, 64)
                );
        }

        public void showGUI()
        {
            guiVisible = true;
        }

        public void hideGUI()
        {
            guiVisible = false;
        }

        public void OnGUI()
        {
            if (!guiVisible) { return; }
            GUI.Window(9999, new Rect(10, 10, 100, 100), OnWindow, "AGOS TEST");
        }

        public void OnWindow(int id)
        {
            GUILayout.Label("TEST!!!!!!");
        }

    }
}
