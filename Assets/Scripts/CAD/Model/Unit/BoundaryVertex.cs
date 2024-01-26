using UnityEngine;

namespace Chiyoda.CAD.Plotplan
{
    public class BoundaryVertex//暫定not memorable
    {
        private Vector3d _point; 
        public Vector3d Point { private set { _point = value; _point.z = 0; } get => _point; }//平面想定
        public Unit Parent { get; }
        public BoundaryVertex(Unit unit ,in Vector3d point)
        {
            Parent = unit;
            Point = point;
        }

        private BoundaryLine _boundaryLine = null;
        public BoundaryLine BoundaryLine {
            get => _boundaryLine;
            set {
                _boundaryLine = value;
                value.SourceVertex = this;
            }
        }
        private BoundaryVertex _nextVertex = null;
        private BoundaryVertex _previousVertex = null;
        private void ClearNextVertex(){_nextVertex = null; BoundaryLine = null; }
        private void ClearPreviousVertex(){
            if (_previousVertex != null) {
                _previousVertex.BoundaryLine = null;
            }
            _previousVertex = null;}
        
        public BoundaryVertex NextVertex
        {
            get => _nextVertex;
            set
            {
                if (value == null)
                {
                    ClearNextVertex();
                }
                else if (value.Parent == Parent)
                {
                    _nextVertex = value;
                    if (value.PreviousVertex != this) 
                    {
                        value.PreviousVertex?.ClearNextVertex();
                        value.PreviousVertex = this;
                    }
                }
            }
        }

        public BoundaryVertex PreviousVertex
        {
            get => _previousVertex;
            set
            {
                if (value == null)
                {
                    ClearPreviousVertex();
                }
                else if (value.Parent == Parent)
                {
                    _previousVertex = value;
                    if (value.NextVertex != this)
                    {
                        value.NextVertex?.ClearPreviousVertex();
                        value.NextVertex = this;
                    }
                }

            }
        }

        public Vector3d GlobalPoint
        {
            get {
                if (Parent.Parent is Unit unit){
                    return unit.GlobalCod.GlobalizePoint(Point);
                }
                else{
                    return Point;
                }
            }
        }
        public string Tag {get;set;}
 
    }
}