using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD;
using Chiyoda.CAD.Topology;

namespace IDF
{
  public class IDFUnionImporter : IDFEntityImporter
  {
    public IDFUnionImporter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements, Vector3d _standard, UnitOption option)
      : base(_type, _legType, _elements, _standard, option)
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
      var term1 = GetStartPoint(elements);
      var term2 = GetEndPoint(elements);
      if (IsSamePoint(term1, term2))
      {
        return ErrorPosition.Other;
      }
      var origin = (term1 + term2) * 0.5d;
      var union = Entity as Union;
      var direction = term2 - term1;
      LeafEdgeCodSysUtils.LocalizeStraightComponent(LeafEdge, origin, direction);
      union.Diameter = GetDiameter(elements).OutsideMeter;
      union.Length = (term2 - term1).magnitude;
      return ErrorPosition.None;
    }

    protected override IDFEntityImporter UpdateImpl(IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType, string[] columns)
    {
      if (entityState == EntityState.Out)
      {
        return this;
      }
      if (entityState == EntityState.In)
      {
        entityState = EntityState.Out;
      }
      return this;
    }
  }
}
