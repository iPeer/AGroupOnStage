using AGroupOnStage.Main;
using PreFlightTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.EngineersReport
{
    public class OutOfStageRangeConcern : DesignConcernBase
    {

        public override string GetConcernDescription()
        {
            return "Your vessel has one or more stage triggers configured that are out of range.";
        }

        public override string GetConcernTitle()
        {
            return "[AGOS] Out of Range Stage Configs!";
        }

        public override DesignConcernSeverity GetSeverity()
        {
            return DesignConcernSeverity.WARNING;
        }

        public override bool TestCondition()
        {
            return !AGOSUtils.hasOutOfRangeStageConfig(); // Inverted because true is satified, false is not and this method returns TRUE if the vessel has out of range configs
        }

    }
}
