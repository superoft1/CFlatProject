using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD;
using Chiyoda.CAD.Topology;

namespace IDF
{
  public class IDFOutletImporter : IDFEntityImporter
  {
    public IDFOutletImporter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements,
      Vector3d _standard, UnitOption option) : base(_type, _legType, _elements, _standard, option)
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
      var olet = Entity as WeldOlet;
      var elements = elementsDictionary[IDFRecordType.LegType.InLeg];
      var start = GetStartPoint(elements); 
      var end = GetEndPoint(elements); 
      if (start == end)
      {
        elements = elementsDictionary[IDFRecordType.LegType.FirstBranchLeg];
        start = GetStartPoint(elements); 
        end = GetEndPoint(elements); 
      }

      var mainTerm = start;
      var branchTerm = end;
      if (IsSamePoint(mainTerm, branchTerm))
      {
        return ErrorPosition.Other;
      }
      var direction = branchTerm - mainTerm;

      LeafEdgeCodSysUtils.LocalizeStraightComponent(LeafEdge, mainTerm, direction);

      olet.Diameter = GetDiameter( elements ).OutsideMeter ;
      olet.LengthFromPipeCenter = (branchTerm - mainTerm).magnitude;
      return ErrorPosition.None;
    }

    protected override IDFEntityImporter UpdateImpl(IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType,
      string[] columns)
    {
      if (fittingType != FittingType)
      {
        return this;
      }

      if (entityState == EntityState.Out)
      {
        return this;
      }

      elementsDictionary.Add(legType, columns);
      switch (legType)
      {
        case IDFRecordType.LegType.FirstBranchLeg:
          entityState = EntityState.FirstLeg;
          break;
        case IDFRecordType.LegType.OutLeg:
          entityState = EntityState.Out;
          break;
      }

      return this;
    }
  }
}