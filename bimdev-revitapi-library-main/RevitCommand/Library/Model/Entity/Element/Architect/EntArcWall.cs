﻿using Autodesk.Revit.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Model.Entity
{
    public class EntArcWall : ArchitectElement
    {
        public override Level? Level => level ??= this.GetLevel();

        public override Level? BaseLevel => baseLevel ??= this.GetBaseLevel();

        public override Level? TopLevel => topLevel ??= this.GetTopLevel();
    }
}
