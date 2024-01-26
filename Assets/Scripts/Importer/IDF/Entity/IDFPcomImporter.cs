using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;

namespace IDF
{

  public class IDFPcomImporter : IDFEntityImporter
  {

    public IDFPcomImporter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements, Vector3d _standard, UnitOption option)
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
      var blank = Entity as OpenSpectacleBlank;

      var firstElements = elementsDictionary[IDFRecordType.LegType.InLeg];
      var lastElements = elementsDictionary[IDFRecordType.LegType.InLeg];
      // 検証-1\IDF\AC21\AC21-P020026010_1.idfに96が載っていないので暫定対応
      if (elementsDictionary.ContainsKey(IDFRecordType.LegType.OutLeg))
      {
        lastElements = elementsDictionary[IDFRecordType.LegType.OutLeg];
      }
      var term1 = GetStartPoint(firstElements);
      var term2 = GetEndPoint(lastElements);
      var origin = (term1 + term2) * 0.5d;
      var direction = term2 - term1;
      if (IsSamePoint(term1, term2))
      {
        return ErrorPosition.Other;
      }

      LeafEdgeCodSysUtils.LocalizeStraightComponent(LeafEdge, origin, direction);
      blank.Diameter = GetDiameter( firstElements ).OutsideMeter ;
      blank.Length = (term2 - term1).magnitude;
      return ErrorPosition.None;
    }

    protected override IDFEntityImporter UpdateImpl(IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType, string[] columns)
    {
      if (fittingType != FittingType)
      {
        return this;
      }
      if (entityState == EntityState.Out)
      {
        return this;
      }
      if (legType == IDFRecordType.LegType.OutLeg)
      {
        elementsDictionary.Add(legType, columns);
        entityState = EntityState.Out;
        return this;
      }
      return this;
    }
  }

}
