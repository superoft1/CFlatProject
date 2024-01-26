using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chiyoda.CAD.Plotplan
{
    public class StraightLine : BoundaryLine
    {
        public StraightLine(BoundaryVertex bv) : base(bv) { }

        public override double InitialDirection { get => GetDirection(); }
        public override double FinalDirection { get => GetDirection(); }
        private double GetDirection()
        {
            var v = TargetPoint - SourcePoint;
            return Math.Atan2(v.y, v.x);
        }
    }
}