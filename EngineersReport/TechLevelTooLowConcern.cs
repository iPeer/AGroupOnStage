using AGroupOnStage.Main;
using PreFlightTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.EngineersReport
{
    public class TechLevelTooLowConcern : DesignConcernBase
    {

        public override string GetConcernDescription()
        {
            return "Your vessel has one or more stage triggers configured that are too advanced for your current tech level.";
        }

        public override string GetConcernTitle()
        {
            return "[AGOS] Tech Level too low!";
        }

        public override DesignConcernSeverity GetSeverity()
        {
            return DesignConcernSeverity.CRITICAL;
        }

        public override bool TestCondition()
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER || AGOSMain.Instance.actionGroups.Count == 0)
                return true;
            return AGOSMain.Instance.actionGroups.Count(a => AGOSUtils.techLevelEnoughForGroup(a.Group)) > 0;
        }

    }
}
