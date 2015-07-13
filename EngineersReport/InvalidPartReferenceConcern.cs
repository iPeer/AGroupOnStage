using AGroupOnStage.Main;
using PreFlightTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.EngineersReport
{
    public class InvalidPartReferenceConcern : DesignConcernBase
    {
        // This is never actually displayed; that's the plan, anyway
        public override string GetConcernDescription()
        {
            return "Your vessel has one or more part-linked triggers configured that are linked to parts that are not on this vessel";
        }

        public override string GetConcernTitle()
        {
            return "[AGOS] Groups With Invalid Part References!";
        }

        public override DesignConcernSeverity GetSeverity()
        {
            return DesignConcernSeverity.WARNING;
        }

        public override bool TestCondition()
        {
            return !AGOSUtils.hasInvalidPartLinkedConfig(); // Inverted because true is satified, false is not and this method returns TRUE if the vessel has invalid links
        }

    }
}
