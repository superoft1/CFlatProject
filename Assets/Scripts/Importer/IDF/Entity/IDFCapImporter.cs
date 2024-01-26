using System.Collections;
using System.Collections.Generic;
using Chiyoda ;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD;
using Chiyoda.CAD.Topology;
using Chiyoda.DB ;

namespace IDF
{

  public class IDFCapImporter : IDFEntityImporter
  {
    public IDFCapImporter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements, Vector3d _standard, UnitOption option) 
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
      var cap = Entity as PipingCap;
      var elements = elementsDictionary[IDFRecordType.LegType.InLeg];
      var diameter = GetDiameter(elements);

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
        if ( prev == null ) {
          return ErrorPosition.Other;
        }
        var table = DB.Get<DimensionOfCapsTable>() ;
        var length = ( (double) table.GetOne( diameter.NpsMm ).Length ).Millimeters() ;
        outsideTerm = weldTerm + (weldTerm - prev.GlobalCod.Origin).normalized * length ;
      }
      var origin = (weldTerm + outsideTerm) * 0.5d;
      cap.Diameter = diameter.OutsideMeter;
      var direction = outsideTerm - weldTerm;
      LeafEdgeCodSysUtils.LocalizeStraightComponent(LeafEdge, origin, direction);
      cap.Length = (outsideTerm - weldTerm).magnitude;
      return ErrorPosition.None;
    }

    protected override IDFEntityImporter UpdateImpl(IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType, string[] columns)
    {
      if(legType != IDFRecordType.LegType.NoLeg && entityState == EntityState.In)
      {
        entityState = EntityState.Out;
      }

      return this;
    }
  }

}
