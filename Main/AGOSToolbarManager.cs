﻿using AGroupOnStage._000Toolbar;
using AGroupOnStage.Logging;
using AGroupOnStage.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace AGroupOnStage.Main
{
    public class AGOSToolbarManager
    {

        public static bool launcherButtonAdded = false;
        public static bool using000Toolbar = false;
        public static IButton _000agosButton;
        public static ApplicationLauncherButton agosButton;

        public enum ButtonType
        {
            DEFAULT,
            SHIMMY_TACO,
            SETTINGS,
            BOB_ROSS
        }

        public static void addToolbarButton()
        {
            if ((ApplicationLauncher.Ready && AGOSMain.Settings.get<bool>("UseStockToolbar")) || !ToolbarManager.ToolbarAvailable)
                setupToolbarButton();
            else
            {
                setup000ToolbarButton();
            }
        }

        public static void switchToolbarsIfNeeded()
        {
            if (ToolbarManager.ToolbarAvailable && !AGOSMain.Settings.get<bool>("UseStockToolbar") && !using000Toolbar)
            {
                if (launcherButtonAdded)
                    removeToolbarButton();
                setup000ToolbarButton();
            }
            else if ((AGOSMain.Settings.get<bool>("UseStockToolbar") && !launcherButtonAdded) || !ToolbarManager.ToolbarAvailable)
            {
                if (using000Toolbar)
                    remove000ToolbarButton();
                setupToolbarButton();
            }
        }

        public static void setup000ToolbarButton()
        {
            Logger.Log("Setting up 000Toolbar");
            _000agosButton = ToolbarManager.Instance.add("AGOS", "AGroupOnStage");
            string _texture = "iPeer/AGroupOnStage/Textures/Toolbar000";
            System.Random r = new System.Random();
            if ((new System.Random().NextBoolOneIn(AGOSMain.Settings.get<int>("TacoButtonChance")) || AGOSMain.Settings.get<bool>("TacosAllDayErrDay")) && AGOSMain.Settings.get<bool>("AllowEE")) // 2.0.7-dev1: This would never be true at its previous value (5) (C# Random is *weird*)
            {
                Logger.Log("Are you hungry?");
                _texture = "iPeer/AGroupOnStage/Textures/Toolbar_alt000";
            }
            _000agosButton.TexturePath = _texture; // The fact we have to give a path and not an actual texture makes me very, very sad.
            _000agosButton.OnClick += (e) => { if (e.MouseButton == 1) { AGOSMain.Settings.toggleGUI(); } else { AGOSMain.Instance.toggleGUI(false); } };
            using000Toolbar = true;

        }

        public static void remove000ToolbarButton()
        {
            Logger.Log("Removing 000Toolbar button");
            _000agosButton.Destroy();
            _000agosButton = null;
            using000Toolbar = false;
        }

        public static void setupToolbarButton()
        {
            if (!launcherButtonAdded)
            {
                /*string _texture = "iPeer/AGroupOnStage/Textures/Toolbar";
                System.Random r = new System.Random();
                if ((r.NextBoolOneIn(5) || AGOSMain.Settings.get<bool>("TacosAllDayErrDay")) && AGOSMain.Settings.get<bool>("AllowEE")) // 2.0.7-dev1: This would never be true at its previous value (5) (C# Random is *weird*)
                {
                    Logger.Log("Are you hungry?");
                    _texture = "iPeer/AGroupOnStage/Textures/Toolbar_alt";
                }*/
                Logger.Log("Adding ApplicationLauncher button");
                agosButton = ApplicationLauncher.Instance.AddModApplication(
                    () => AGOSMain.Instance.toggleGUI(),
                    () => AGOSMain.Instance.toggleGUI(),
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.SPACECENTER,
                    //ApplicationLauncher.AppScenes.ALWAYS,
                    (Texture)createButtonTexture()
                );
                launcherButtonAdded = true;
            }
            else
                Logger.LogWarning("ApplicationLauncher button is already present (harmless)");

        }

        public static void removeToolbarButton()
        {
            Logger.Log("Removing ApplicationLauncher button");
            ApplicationLauncher.Instance.RemoveModApplication(agosButton);
            launcherButtonAdded = false;
        }

        public static Texture2D createButtonTexture()
        {
            ButtonType t = ButtonType.DEFAULT;
            if ((AGOSMain.Settings.get<bool>("TacosAllDayErrDay") || AGOSMain.Settings.get<bool>("HappyLittleTrees") || new System.Random().NextBoolOneIn(AGOSMain.Settings.get<int>("TacoButtonChance"))) && AGOSMain.Settings.get<bool>("AllowEE"))
            {
                bool taco = new System.Random().NextBool();
                if (taco || AGOSMain.Settings.get<bool>("TacosAllDayErrDay"))
                    t = ButtonType.SHIMMY_TACO;
                else if (!taco || AGOSMain.Settings.get<bool>("HappyLittleTrees"))
                    t = ButtonType.BOB_ROSS;
            }
            return createButtonTexture(t);

        }

        public static Texture2D createButtonTexture(ButtonType type)
        {
            int x = 0;
            int y = 128;
            int w = 128;
            int h = 128;
            if (type == ButtonType.SHIMMY_TACO)
            {
                Logger.Log("Are you hungry?");
                y = 256;
            }
            else if (type == ButtonType.SETTINGS)
            {
                x = y = 184;
                w = h = 72;
            }
            else if (type == ButtonType.BOB_ROSS)
            {
                Logger.Log("Happy little accidents!");
                x = 128;
                y = 0;
            }
            Texture2D mainTex = AGOSUtils.loadTextureFromDDS(System.IO.Path.Combine(AGOSUtils.getDLLPath(), "Textures/Buttons.dds"), TextureFormat.DXT5);
            Color[] pixels = mainTex.GetPixels(x, y, w, h); // Get the pixels we want from the main texture
            Texture2D buttonTex = new Texture2D(w, h, TextureFormat.ARGB32, false); // Create the image that will be used for the button
            //buttonTex.SetPixels(pixels); // Fill the image with the pixels we want
            if (type != ButtonType.SETTINGS && AGOSMain.Instance.SpecialOccasion) // Draw confetting for special occasions! \o/
            {
                // 2.0.12-dev4: Fixes button showing white textures when overlaying confetti. Also made this code slightly more efficient, because everyone likes efficiency!
                Color[] confetti = mainTex.GetPixels(128, 256, 128, 128);

                for (int i = 0; i < pixels.Length; i++)
                {
                    if (confetti[i].a < 1f)
                        pixels[i] += confetti[i];
                    else pixels[i] = confetti[i];
                }
            }

            buttonTex.SetPixels(pixels);

            buttonTex.Apply(); // Apply changes
            return buttonTex;

        }

        public static void disableToolbarButton()
        {
            if (using000Toolbar)
                _000agosButton.Enabled = false;
            else
                agosButton.Disable();
        }

        public static void enableToolbarButton()
        {
            if (using000Toolbar)
                _000agosButton.Enabled = true;
            else
                agosButton.Enable();
        }

        public static void toggleToolbarButtonEnabledState()
        {
            if (using000Toolbar)
                _000agosButton.Enabled = !_000agosButton.Enabled;
            else
                if (agosButton.enabled)
                    agosButton.Disable();
                else
                    agosButton.Enable();
        }

    }
}
