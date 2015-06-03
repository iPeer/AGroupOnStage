using AGroupOnStage._000Toolbar;
using AGroupOnStage.Logging;
using AGroupOnStage.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{
    public class AGOSToolbarManager
    {

        public static bool launcherButtonAdded = false;
        public static bool using000Toolbar = false;
        public static IButton _000agosButton;
        public static ApplicationLauncherButton agosButton;

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
            if ((r.NextBoolOneIn(5) || AGOSMain.Settings.get<bool>("TacosAllDayErrDay")) && AGOSMain.Settings.get<bool>("AllowEE")) // 2.0.7-dev1: This would never be true at its previous value (5) (C# Random is *weird*)
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
                    AGOSMain.Instance.toggleGUI,
                    AGOSMain.Instance.toggleGUI,
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

        public static Texture2D createButtonTexture(bool blizzy = false)
        {
            int x = 0;
            int y = 0;
            int w = 128;
            int h = 128;
            if (blizzy) // After writing this, I discovered you *must* give Toolbar a path, not a texture ಠ_ಠ
            {
                w = h = 24;
                x = 128;
            }
            if ((AGOSMain.Settings.get<bool>("TacosAllDayErrDay") || (new System.Random()).NextBoolOneIn(5)) && AGOSMain.Settings.get<bool>("AllowEE"))
            {
                Logger.Log("Are you hungry?");
                y = (blizzy ? 24 : 128);
            }
            Texture2D mainTex = AGOSUtils.loadTextureFromDDS(System.IO.Path.Combine(AGOSUtils.getDLLPath(), "Textures/Buttons.dds"));
            Color[] pixels = mainTex.GetPixels(x, y, w, h); // Get the pixels we want from the main texture
            Texture2D buttonTex = new Texture2D(w, h, TextureFormat.ARGB32, false); // Create the image that will be used for the button
            buttonTex.SetPixels(pixels); // Fill the image with the pixels we want
            if (AGOSMain.Instance.SpecialOccasion) // Draw confetting for special occasions! \o/
            {
                for (int a = 0; a < 128; a++) // height
                {
                    for (int b = 0; b < 128; b++) // width
                    {
                        Color pixel = mainTex.GetPixel(a + 128, b + 128);
                        Color pixelBack = buttonTex.GetPixel(a, b);
                        Color _pixel = pixel * pixel.a;
                        buttonTex.SetPixel(a, b, pixelBack + _pixel);
                    }
                }
            }

            buttonTex.Apply(); // Apply changes
            return buttonTex;

        }

    }
}
