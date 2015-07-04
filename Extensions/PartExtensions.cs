﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGroupOnStage.Extensions
{
    public static class PartExtensions
    {

        public static string savedPartName(this Part part) 
        {
            return String.Format("{0}_{1}", part.name, part.craftID);
        }

    }
}