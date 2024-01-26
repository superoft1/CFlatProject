using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Chiyoda.CAD.Plotplan
{
    public abstract class BoundaryLine
    {
        public BoundaryVertex SourceVertex { get; set; }//このへんをしっかり管理したいEvent?
        public BoundaryLine(BoundaryVertex bv)
        {
            SourceVertex = bv;
            bv.BoundaryLine = this;
        }

        public Vector3d SourcePoint { get=> SourceVertex.Point; }
        public Vector3d TargetPoint { get=> SourceVertex.NextVertex.Point; }

        public BoundaryLine NextLine
        {
            get=> SourceVertex.NextVertex?.BoundaryLine;
        }

        public BoundaryLine PreviousLine
        {
            get=> SourceVertex.PreviousVertex?.BoundaryLine;
        }

        public abstract double InitialDirection { get; }
        public abstract double FinalDirection { get; }
    }
}