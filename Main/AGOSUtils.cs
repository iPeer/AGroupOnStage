using AGroupOnStage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AGroupOnStage.Main
{
    public static class AGOSUtils
    {

        public static bool isLoadedSceneOneOf(params GameScenes[] scenes)
        {
            foreach (GameScenes g in scenes)
            {
                if (HighLogic.LoadedScene == g)
                    return true;
            }
            return false;
        }

        public static Dictionary<int, GUISkin> getSkinList()
        {

            GUISkin[] skins = Resources.FindObjectsOfTypeAll(typeof(GUISkin)) as GUISkin[];
            Dictionary<int, GUISkin> guiSkins = new Dictionary<int, GUISkin>();
            int _skinID = 0;
            foreach (GUISkin _skin in skins)
                guiSkins.Add(_skinID++, _skin);
            return guiSkins;
        }

        public static Dictionary<int, string> preferredSkins = new Dictionary<int, string>() {

            { 0, "Unity" },
			{ 1, "GameSkin(Clone)" },
			{ 2, "GameSkin" }

		};

        public static GUISkin getBestAvailableSkin()
        {
            Dictionary<int, GUISkin> skinList = getSkinList();
            Logger.LogDebug("Skin list:");
            for (int x = 0; x < skinList.Count; x++)
            {
                Logger.LogDebug("\t{0}", skinList[x].name);
            }
            for (int __skinId = 0; __skinId < preferredSkins.Count; __skinId++)
            {
                string skinName = preferredSkins[__skinId];
                for (int id = 0; id < skinList.Count; id++)
                {
                    if (skinList[id].name == skinName)
                    {
                        return skinList[id];
                    }
                }
            }
            Logger.LogWarning("No preferred skin found, defaulting to HighLogic.Skin");
            return HighLogic.Skin;
        }

    }
}
