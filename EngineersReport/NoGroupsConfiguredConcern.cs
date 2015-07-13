using AGroupOnStage.Main;
using PreFlightTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.EngineersReport
{
    public class NoGroupsConfiguredConcern : DesignConcernBase
    {

        public override string GetConcernDescription()
        {
            return "There are no action group configurations assigned to this vessel.";
        }

        public override string GetConcernTitle()
        {
            return "[AGOS] Nothing Configured!";
        }

        public override DesignConcernSeverity GetSeverity()
        {
            return DesignConcernSeverity.NOTICE;
        }

        public override bool TestCondition()
        {
            return AGOSMain.Instance.actionGroups.Count() > 0;
        }

    }
}
