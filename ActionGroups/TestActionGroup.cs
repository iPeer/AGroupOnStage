using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.ActionGroups
{
    public class TestActionGroup : AGOSActionGroup
    {

        public override bool IsTester
        {
            get
            {
                return true;
            }
        }

    }
}
