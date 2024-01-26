using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Core;
using System;
using Chiyoda.UI;
using System.Collections.Generic;
using System.Linq;

namespace Chiyoda.CAD.Plotplan
{
    [System.Serializable]
    [Entity(EntityType.Type.LeafUnit)]//暫定
    public class LeafUnit : Unit
    {
        private readonly Memento<Vector3d> _areaSize;

        public LeafUnit(Document document) : base(document)
        {
            _unitType = Plotplan.UnitType.Type.Unknown;
            _areaSize = CreateMementoAndSetupValueEvents( Vector3d.one ) ;
        }

        private UnitType.Type _unitType;
        public UnitType.Type UnitType
        {
            get { return _unitType; }
            set { _unitType = value; Name = value.ToString(); }
        }

        public Vector3d AreaSize { get => _areaSize.Value; set => _areaSize.Value = value; }

        private IEnumerable<BoundaryVertex> _innerVertices { get; set; }
        public IEnumerable<BoundaryVertex> InnerVertices
        {
            get => _innerVertices;
            set
            {
                SetVerticesLoop(value);
                _innerVertices = value;
            }
        }
        public  List<BoundaryLine> InnerBoundary { get; private set;}
        private double _setBack=0;
        public double SetBack { get => _setBack; set => _setBack = Math.Max(0, value);}
        public  IEnumerable<BoundaryVertex> OuterVertices { get; private set; }
        public  List<BoundaryLine> OuterBoundary { get; private set; }

        public void SetVerticesLoop(IEnumerable<BoundaryVertex> vertices)
        {
            vertices.ElementAt(0).PreviousVertex = vertices.ElementAt(vertices.Count()-1);
            for (int i=1; i< vertices.Count(); i++)
            {
                vertices.ElementAt(i).PreviousVertex = vertices.ElementAt(i - 1);
            }
        }

        public void SetRectangelBoundary() {
            var vertices = new List<BoundaryVertex>();
            var lines = new List<BoundaryLine>();
            for (int i = 0; i < 4; i++)
            {
                var sign_x = Math.Pow(-1,i / 2 +1);
                var sign_y = Math.Pow(-1,i % 2 +1);
                var v = new BoundaryVertex(this, new Vector3d(sign_x*AreaSize.x / 2, sign_y * AreaSize.y / 2, 0));
                var line = new StraightLine(v);
                vertices.Add(v);
                lines.Add(line);
            }
            InnerVertices = vertices;
            InnerBoundary = lines;
        }

        private static Vector3d CreatNormalVector(double direction)
        {
            double normal = direction - Math.PI / 2;
            return new Vector3d(Math.Cos(normal), Math.Sin(normal), 0);
        }
        private BoundaryVertex CreateOuterVertex(Vector3d point, double direction)
        {
            var nv = CreatNormalVector(direction);
            var op=point + nv * SetBack;
            return new BoundaryVertex(this, op);
        }

        private void AddVertexAndLineAlongInnerLine(BoundaryLine innerLine,
            List<BoundaryVertex>vertices, List<BoundaryLine> lines)
        {
            var init = innerLine.InitialDirection;
            var ov = CreateOuterVertex(innerLine.SourcePoint, init);
            if (innerLine is StraightLine)
            {
                var bl = new StraightLine(ov);
                vertices.Add(ov);
                lines.Add(bl);
            }
            else
            {
                var cl = innerLine as CircularLine;
                var bl = new CircularLine(ov, cl.Center);
                vertices.Add(ov);
                lines.Add(bl);
            }
        }

        public void SetOuterBoundary()
        {
            if (SetBack == 0)
            {
                OuterVertices = InnerVertices;
                OuterBoundary = InnerBoundary;
                return;
            }

            var vertices = new List<BoundaryVertex>();
            var lines = new List<BoundaryLine>();
            foreach(BoundaryLine innerLine in InnerBoundary)
            {
                var prev = innerLine.PreviousLine.FinalDirection ;
                var init = innerLine.InitialDirection ;
                while (init < prev)
                {
                    init += 2 * Math.PI;
                }
                if (init == prev)
                {
                    AddVertexAndLineAlongInnerLine(innerLine, vertices, lines);
                }
                else if(init - prev< Math.PI)
                {
                    var pov = CreateOuterVertex(innerLine.SourcePoint, prev);
                    var cl = new CircularLine(pov, innerLine.SourcePoint);
                    vertices.Add(pov);
                    lines.Add(cl);
                    AddVertexAndLineAlongInnerLine(innerLine, vertices, lines);
                }
                else
                {
                    var ov = CreateOuterVertex(innerLine.SourcePoint, (prev+init)/2);
                    if (innerLine is StraightLine)
                    {
                        var bl = new StraightLine(ov);
                        vertices.Add(ov);
                        lines.Add(bl);
                    }
                    else
                    {
                        var cl = innerLine as CircularLine;
                        var bl = new CircularLine(ov, cl.Center);
                        vertices.Add(ov);
                        lines.Add(bl);
                    }
                }
            }
        }

        //未実装
        // public override IEnumerable<IElement> Children;

    }

}
