using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD;
using Chiyoda.CAD.Topology;

namespace IDF
{

  public class IDFValveImporter : IDFEntityImporter
  {
    EntityType.Type ValveType{ get; }

    
    public IDFValveImporter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements, Vector3d _standard, UnitOption option)
      : base(_type, _legType, _elements, _standard, option)
    {
      ValveType = GetValveType( _elements[ 11 ] ) ;
    }

    public override LeafEdge Import(Chiyoda.CAD.Core.Document doc)
    {
      var entity = doc.CreateEntity(ValveType);
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
      var direction = (term2 - term1).normalized;
      LeafEdgeCodSysUtils.LocalizeStraightComponent(LeafEdge, origin, direction);

      var diameter = GetDiameter( elements ).OutsideMeter ;
      switch ( Entity ) {
        case BallValve ballValve :
          ballValve.Diameter = diameter;
          ballValve.Length = (term2 - term1).magnitude;
          break ;
        case GlobeValve globeValve :
          globeValve.Diameter = diameter;
          globeValve.Length = (term2 - term1).magnitude;
          break ;
        case CheckValve checkValve :
          checkValve.Diameter = diameter;
          checkValve.Length = (term2 - term1).magnitude;
          break ;
        case ButterflyValve butterflyValve :
          butterflyValve.Diameter = diameter;
          butterflyValve.Length = (term2 - term1).magnitude;
          break ;
        case GateValve gateValve :
          gateValve.Diameter = diameter;
          gateValve.Length = (term2 - term1).magnitude;
          break ;
        default:
          Debug.LogError("IDF Valve Missing.");
          break ;
      }

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
    
    private static EntityType.Type GetValveType(string symbolKey)
    {
      if ( symbolKey.StartsWith( "VB" ) ) return EntityType.Type.BallValve ;
      if ( symbolKey.StartsWith( "VG" ) ) return EntityType.Type.GlobeValve ;
      if ( symbolKey.StartsWith( "VC" ) ) return EntityType.Type.CheckValve ;
      if ( symbolKey.StartsWith( "CK" ) ) return EntityType.Type.CheckValve ;
      if ( symbolKey.StartsWith( "ZB" ) ) return EntityType.Type.ButterflyValve ;
      if ( symbolKey.StartsWith( "VT" ) ) return EntityType.Type.GateValve ;
      // 該当なしもGateValveに含める
      return EntityType.Type.GateValve ;
    }
  }

}
