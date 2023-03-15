﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Model.Entity
{
    public class EntBeam : StructuralElement
    {
        public override Level? Level => level ??= this.GetLevel();
    }
}
