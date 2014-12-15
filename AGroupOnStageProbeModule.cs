using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iPeerLib.Logging;
using AGroupOnStage.StageConfig;

namespace AGroupOnStage
{
    [KSPModule("Action Group On Stage Controller")]
    public class AGroupOnStageProbeModule : PartModule
    {

        private int currentVesselStages = 0;
        public Dictionary<int, StageConfiguration> stageMap = new Dictionary<int, StageConfiguration>();

        #region module description

        public override string GetInfo()
        {
            return "Can trigger action groups across the entire vessel when any stage is fired.";
        }

        #endregion
        #region module hooks

        public override void OnAwake()
        {
            if (iPeerLib.Utility.Utils.isLoadedSceneOneOf(GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.SPH))
            {
                createStageMap();
                GameEvents.onStageActivate.Add(onStageActivate);
                GameEvents.onStageSeparation.Add(onStageSeparation);
            }
        }

        private void onStageSeparation(EventReport data)
        {
#if DEBUG
            Logger.Log("{0}, {1}, {2}, {3}, Stage: {4}", data.msg, data.origin, data.other, data.sender, data.stage);
#endif
        }

        private void onStageActivate(int data)
        {
#if !DEBUG
            throw new NotImplementedException();
#endif
            Logger.Log("Stage fired: {0}", data);
        }

        #endregion

        #region custom methods

        public void createStageMap()
        {
            Vessel v = FlightGlobals.ActiveVessel;
            for (int x = 0; x < v.currentStage; x++)
            {
                stageMap.Add(x, new StageConfiguration());
            }
            Logger.Log("Created StageMap for {0} stages.", v.currentStage);
        }

        #endregion


    }
}
