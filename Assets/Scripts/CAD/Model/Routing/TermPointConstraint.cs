using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Routing
{
  public class TermPointConstraints : IRoutingConstraint
  {
    private readonly Vector3d _dir ;
    private readonly Vector3d _pos ;

    public Vector3d? DirectionConstraint => _dir ;
    public Vector3d? PositionConstraint => _pos ;
    public Diameter DiameterConstraint { get ; }

    public IEnumerable<IRoutingEdge> Links => Enumerable.Empty<IRoutingEdge>() ;

    public TermPointConstraints( Vector3d pos, Vector3d dir )
    {
      _dir = dir ;
      _pos = pos ;
    }
    
    public TermPointConstraints( HalfVertex vertex )
    {
      _pos = vertex.GlobalPoint ;
      var pp = vertex.LeafEdge.PipingPiece ;
      var connPointIdx = vertex.ConnectPointIndex ;
      var connPoint = pp.GetConnectPoint( connPointIdx ) ;
      DiameterConstraint = connPoint.Diameter;
      _dir = GetDirection( vertex, pp, _pos ) ;
    }

    private static Vector3d GetDirection( HalfVertex vertex, PipingPiece pp, Vector3d origin )
    {
      // 方向の取得(PipingPieceに持たせるべき)
      switch ( pp ) {
        case Equipment instrument :
          return GetEquipmentDirection( vertex, instrument ) ;
        case Pipe _ :
        case PipingTee _ :
        case PipingElbow90 _ :
        case ControlValve _:
        case PressureReliefValve _:
          return GetRouteDirection( vertex, origin, pp ) ;
        default :
          Debug.LogError( "Cannot get direction of end points!" ) ;
          return DirectionalPoint.DefaultDirection ;
      }
    }

    private static Vector3d GetRouteDirection( HalfVertex vertex, Vector3d origin, PipingPiece pp )
    {
      var dirOriginToCp = origin - pp.GetWorldPosition() ;
      var sign = ( vertex.Flow == HalfVertex.FlowType.FromAnotherToThis ) ? -1.0f : 1.0f ;
      return sign * (Vector3) dirOriginToCp.normalized ;
    }

    private static Vector3d GetEquipmentDirection( HalfVertex vertex, Equipment instrument )
    {
      var nozzle = instrument.GetNozzle( vertex.ConnectPointIndex ) ;
      var local = GetNozzleDirection( vertex.Flow, instrument, nozzle ) ;
      return ( vertex.LeafEdge.Parent != null && vertex.LeafEdge.Parent is BlockPattern bp )
        ? ConvertToGlobal( local, vertex, bp )
        : local ;
    }

    private static Vector3d ConvertToGlobal( Vector3d src, HalfVertex vertex, BlockPattern bp )
    {
      var onVertex = vertex.LeafEdge.LocalCod.GlobalizeVector( src ) ;
      return bp.LocalCod.GlobalizeVector( onVertex ) ;
    }

    private static Vector3d GetNozzleDirection( HalfVertex.FlowType type, Equipment instrument, Nozzle nozzle )
    {
      var dir = (Vector3) instrument.GetNozzleDirection( nozzle ) ;
      switch ( type ) {
        case HalfVertex.FlowType.Undefined :
          return ( nozzle.NozzleType == Nozzle.Type.Suction ) ? -dir : dir ;
        case HalfVertex.FlowType.FromAnotherToThis :
          return -dir ;
        case HalfVertex.FlowType.FromThisToAnother :
          return dir ;
        default :
          return dir ;
      }
    }
  }
}