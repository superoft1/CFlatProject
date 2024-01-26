using System ;
using System.Collections;
using System.Collections.Generic;
using System.Configuration ;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD;
using Chiyoda.CAD.Topology;

namespace IDF
{

  public class IDFFlangeImporter : IDFEntityImporter
  {
    public IDFFlangeImporter( IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements,
      Vector3d _standard, UnitOption option )
      : base( _type, _legType, _elements, _standard, option )
    {
    }

    public override LeafEdge Import( Chiyoda.CAD.Core.Document doc )
    {
      var entity = doc.CreateEntity( IDFEntityType.GetType( FittingType ) ) ;
      LeafEdge = doc.CreateEntity<LeafEdge>() ;
      LeafEdge.PipingPiece = entity as PipingPiece ;
      return LeafEdge ;
    }

    public static void CalcShape( WeldNeckFlange flange, Vector3d weld, Vector3d outside, double diameter )
    {
      var origin = ( weld + outside ) * 0.5d ;
      var direction = weld - outside ;

      LeafEdgeCodSysUtils.LocalizeStraightComponent( (LeafEdge) flange.Parent, origin, direction ) ;

      flange.WeldDiameter = diameter ;
      flange.OutsideDiameter = flange.WeldDiameter ;
      flange.Length = ( outside - weld ).magnitude ;
    }

    public override ErrorPosition Build( LeafEdge prev, IDFEntityImporter next )
    {
      var flange = Entity as WeldNeckFlange ;
      var elements = elementsDictionary[ IDFRecordType.LegType.InLeg ] ;
      var startPoint = GetStartPoint( elements ) ;
      var endPoint = GetEndPoint( elements ) ;
      
      if ( IsSamePoint( startPoint, endPoint ) ) {
        endPoint = next.GetStartPoint() ;
      }
      if ( IsSamePoint( startPoint, endPoint ) ) {
        return ErrorPosition.Other ;
      }

      Vector3d weld = endPoint ;
      Vector3d outside = startPoint ;
      switch ( JudgeDirection( prev, next, ( startPoint - endPoint ).magnitude ) ) {
        case FlangeDirection.From2To :
          outside = startPoint ;
          weld = endPoint ;
          break ;
        case FlangeDirection.To2From :
          weld = startPoint ;
          outside = endPoint ;
          break ;
        case FlangeDirection.UnKnown :
          break ;
        default :
          throw new ArgumentOutOfRangeException() ;
      }

      CalcShape( flange, weld, outside, GetDiameter( elements ).OutsideMeter ) ;
      return ErrorPosition.None ;
    }

    enum FlangeDirection
    {
      From2To, // To側が大きい
      To2From, // From側が大きい
      UnKnown,
    }

    private FlangeDirection JudgeDirection( LeafEdge prev, IDFEntityImporter next, double flangeLength )
    {
      if ( prev == null ) {
        if ( next?.LeafEdge.PipingPiece is Pipe ||
             next?.LeafEdge.PipingPiece is PipingElbow90 ||
             next?.LeafEdge.PipingPiece is PipingElbow45 ||
             next?.LeafEdge.PipingPiece is PipingTee ) {
          return FlangeDirection.To2From ;
        }
      }
      else if ( next?.LeafEdge.PipingPiece is WeldNeckFlange ||
                next?.LeafEdge.PipingPiece is ControlValve || 
                next?.LeafEdge.PipingPiece is CheckValve ) {
        return FlangeDirection.From2To ;
      }
      else
        switch ( prev.PipingPiece ) {
          case WeldNeckFlange _ :
            return FlangeDirection.To2From ;
          case Pipe pipe :
            if ( next != null ) {
              // Flangeの前後にパイプがついているデータがあったが、とりあえずFlange間にはFlangeよりも長い直管はつかないという想定
              return pipe.Length > flangeLength ? FlangeDirection.From2To : FlangeDirection.To2From ;
            }
            else {
              // Pipe --> Flange --> CntrolValveで、Pipeの長さがFlangeよりも僅かに短いケースがあったのでとりあえずpipe*2にしている
              return pipe.Length * 2 > flangeLength ? FlangeDirection.From2To : FlangeDirection.To2From ;
            }

          case PipingElbow45 _ :
          case PipingElbow90 _ :
          case PipingTee _ :
            // Flange間にはElbowはつかないという想定
            return FlangeDirection.From2To ;
          default :
            return FlangeDirection.To2From ;
        }

      if ( next?.LeafEdge.PipingPiece is WeldNeckFlange ) {
        return FlangeDirection.From2To ;
      }

      // このケースの対応が必要な場合には、IDFの全形状確定後の判断が必要になる
      Debug.LogWarning( "FlangeDirection.UnKnown" ) ;
      return FlangeDirection.UnKnown ;
    }

    protected override IDFEntityImporter UpdateImpl( IDFRecordType.FittingType fittingType,
      IDFRecordType.LegType legType, string[] columns )
    {
      if ( entityState == EntityState.Out ) {
        return this ;
      }

      if ( entityState == EntityState.In ) {
        entityState = EntityState.Out ;
      }

      return this ;
    }
  }
}