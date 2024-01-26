using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace IDF
{
  public class IDFBlindFlangeImporter : IDFEntityImporter
  {
    public IDFBlindFlangeImporter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType,
      string[] _elements, Vector3d _standard, UnitOption option) : base(_type, _legType, _elements, _standard, option)
    {
    }

    public override LeafEdge Import(Chiyoda.CAD.Core.Document doc)
    {
      var entity = doc.CreateEntity(IDFEntityType.GetType(FittingType));
      LeafEdge = doc.CreateEntity<LeafEdge>();
      LeafEdge.PipingPiece = entity as PipingPiece;
      return LeafEdge;
    }

    public override ErrorPosition Build( LeafEdge prev, IDFEntityImporter next )
    {
      var elements = elementsDictionary[IDFRecordType.LegType.InLeg];
      Vector3d weldTerm;
      Vector3d outsideTerm;
      if (next == null)
      {
        weldTerm = GetStartPoint(elements);
        outsideTerm = GetEndPoint(elements);
      }
      else
      {
        weldTerm = GetEndPoint(elements);
        outsideTerm = GetStartPoint(elements);
      }
      if (IsSamePoint(weldTerm, outsideTerm))
      {
        return ErrorPosition.Other;
      }
      var diameter = GetDiameter(elements);
      var origin = (weldTerm + outsideTerm) * 0.5d;

      var direction = weldTerm - outsideTerm;
      LeafEdgeCodSysUtils.LocalizeStraightComponent(LeafEdge, origin, direction);

      var flange = Entity as BlindFlange;
      Debug.Assert(flange != null, "flange != null");
      flange.Diameter = diameter.OutsideMeter ;
      flange.Length = (outsideTerm - weldTerm).magnitude;
      return ErrorPosition.None;
    }

    protected override IDFEntityImporter UpdateImpl(IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType,
      string[] columns)
    {
      if (legType != IDFRecordType.LegType.NoLeg && entityState == EntityState.In)
      {
        entityState = EntityState.Out;
      }

      return this;
    }
  }
}