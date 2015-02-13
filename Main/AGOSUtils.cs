using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    }
}
