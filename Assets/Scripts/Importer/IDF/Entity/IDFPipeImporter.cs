using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD;
using Chiyoda.CAD.Topology;

namespace IDF
{
  public class IDFPipeImporter : IDFEntityImporter
  {

    public IDFPipeImporter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements, Vector3d _standard, UnitOption option)
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

    public override Vector3d GetStartPoint()
    {
      return GetStartPoint( elementsDictionary[IDFRecordType.LegType.InLeg] ) ;
    }
    
    public override ErrorPosition Build( LeafEdge prev, IDFEntityImporter next )
    {
      var pipe = Entity as Pipe;
      var elements = elementsDictionary[IDFRecordType.LegType.InLeg];
      var term1 = GetStartPoint();
      var term2 = GetEndPoint(elements);
      if (IsSamePoint(term1, term2))
      {
        return ErrorPosition.Other;
      }
      var direction = term2 - term1;
      var origin = (term1 + term2) * 0.5d;
      LeafEdgeCodSysUtils.LocalizeStraightComponent(LeafEdge, origin, direction);
      pipe.Diameter = GetDiameter( elements ).OutsideMeter ;
      pipe.Length = (term2 - term1).magnitude;
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
