using System ;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD;
using Chiyoda.CAD.Topology;

namespace IDF
{

  public class IDFElbowImporter : IDFEntityImporter
  {
    EntityType.Type entityType ;

    public IDFElbowImporter( IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements,
      Vector3d _standard, UnitOption option )
      : base( _type, _legType, _elements, _standard, option )
    {
    }

    public override LeafEdge Import( Chiyoda.CAD.Core.Document doc )
    {
      var firstElements = elementsDictionary[ IDFRecordType.LegType.InLeg ] ;
      entityType = ( int.Parse( firstElements[ 12 ] ) == 4500 )
        ? EntityType.Type.PipingElbow45
        : EntityType.Type.PipingElbow90 ;
      var entity = doc.CreateEntity( entityType ) ;
      LeafEdge = doc.CreateEntity<LeafEdge>() ;
      LeafEdge.PipingPiece = entity as PipingPiece ;
      return LeafEdge ;
    }

    private ErrorPosition BuildElbow90()
    {
      var elbow = Entity as PipingElbow90 ;
      var firstElements = elementsDictionary[ IDFRecordType.LegType.InLeg ] ;
      if ( ! elementsDictionary.ContainsKey( IDFRecordType.LegType.OutLeg ) ) {
        return ErrorPosition.Other ;
      }

      var lastElements = elementsDictionary[ IDFRecordType.LegType.OutLeg ] ;

      var origin = GetEndPoint( firstElements ) ;
      var axis = ( GetStartPoint( firstElements ) - GetEndPoint( firstElements ) ).normalized ;
      var reference = ( GetStartPoint( lastElements ) - GetEndPoint( lastElements ) ).normalized ;

      var term1 = GetStartPoint( firstElements ) ;
      LeafEdgeCodSysUtils.LocalizeElbow90Component( LeafEdge, origin, axis, reference ) ;
      var diameter = GetDiameter( firstElements ) ;
      elbow.ChangeSizeNpsMm( 0, diameter.NpsMm ) ;
      elbow.SetElbowTypeFromBendLength( diameter, LeafEdge.GlobalCod.LocalizePoint( term1 ).magnitude ) ;

      return ErrorPosition.None ;
    }

    private ErrorPosition BuildElbow45()
    {
      var elbow = Entity as PipingElbow45 ;
      var firstElements = elementsDictionary[ IDFRecordType.LegType.InLeg ] ;
      if ( ! elementsDictionary.ContainsKey( IDFRecordType.LegType.OutLeg ) ) {
        return ErrorPosition.Other ;
      }

      var lastElements = elementsDictionary[ IDFRecordType.LegType.OutLeg ] ;

      var origin = GetEndPoint( firstElements ) ;
      var reference = ( GetStartPoint( lastElements ) - GetEndPoint( lastElements ) ).normalized ;

      var axis = ( GetStartPoint( firstElements ) - GetEndPoint( firstElements ) ).normalized ;

      var term1 = GetStartPoint( firstElements ) ;
      var term2 = GetEndPoint( lastElements ) ;

      if ( IsSamePoint( term1, term2 ) ) {
        return ErrorPosition.Other ;
      }

      LeafEdgeCodSysUtils.LocalizeElbow45Component( LeafEdge, origin, axis, reference ) ;
      elbow.ChangeSizeNpsMm( 0, GetDiameter( firstElements ).NpsMm ) ;
      elbow.BendLength = LeafEdge.GlobalCod.LocalizePoint( term1 ).magnitude ;

      return ErrorPosition.None ;
    }

    public override ErrorPosition Build( LeafEdge prev, IDFEntityImporter next )
    {
      if ( entityType == EntityType.Type.PipingElbow90 ) {
        return BuildElbow90() ;
      }

      return BuildElbow45() ;
    }

    protected override IDFEntityImporter UpdateImpl( IDFRecordType.FittingType fittingType,
      IDFRecordType.LegType legType, string[] columns )
    {
      if ( fittingType != FittingType ) {
        return this ;
      }

      if ( entityState == EntityState.Out ) {
        return this ;
      }

      if ( legType == IDFRecordType.LegType.OutLeg ) {
        elementsDictionary.Add( legType, columns ) ;
        entityState = EntityState.Out ;
        return this ;
      }

      return this ;
    }
  }
}