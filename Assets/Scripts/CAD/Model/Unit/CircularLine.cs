using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chiyoda.CAD.Plotplan
{
    public class CircularLine : BoundaryLine
    {
        private Vector3d _center;
        public Vector3d Center { get => _center;set { _center = ProjectionToVerticesCenterLine2D(value); } }
        public CircularLine(BoundaryVertex bv,Vector3d center) : base(bv)
        {
            Center = center;
        }

        public double Radius { get => Vector3d.Distance(Center,SourcePoint);}
        public override double InitialDirection {
            get {
                var v = SourcePoint - Center;
                return Math.Atan2(v.y, v.x)+Math.PI/2;
                }
        }

        public override double FinalDirection
        {
            get
            {
                var v = TargetPoint - Center;
                return Math.Atan2(v.y, v.x)+Math.PI/2;
            }
        }

        private Vector3d ProjectionToVerticesCenterLine2D(Vector3d point)
        {
            var s = SourcePoint;
            var t = TargetPoint;
            var m = (t + s) / 2;
            var v = (t - s).normalized;
            m.z = 0;
            v.z = 0;
            var p = point - m;
            var pv = Vector3d.Project(point,v);
            return p-pv;
        }
    }
}

