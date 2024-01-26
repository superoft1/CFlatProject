using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Model.Routing ;
using Chiyoda.CAD.Topology ;
using Routing ;
using Routing.Model ;
using Routing.TempMath ;
using UnityEngine ;
using VtpAutoRouting.SubMethods;
using Factory = Routing.Factory ;

namespace VtpAutoRouting.BridgeEntities
{
  internal class RoutingSource : IAutoRoutingSource
  {
    private class Connection : ILinkedRoute
    {
      public Connection( IPipeProperty diameter, string insulationType, double temperature )
      {
        Diameter = diameter ;
        InsulationType = insulationType ;
        Temperature = temperature ;
      }
    
      public IPipeProperty Diameter { get ; }
      public string InsulationType { get ; }
      public double Temperature { get ; }
    }
    
    private readonly Route _route ;

    public RoutingSource( Route r )
    {
      _route = r ;
    }

    public string LineId => _route.LineId ;

    public int RouteCount { get ; private set ; } = 0 ;
    
    public IAutoRoutingCondition Condition => new AutoRoutingCondition( 
        "TBD", _route.LineType, _route.FluidPhase,
        _route.IsEndPointDirectionFix, _route.IsRoutingPipeRacks, _route.Temperature, _route.InsulationType ) ;

    public IAutoRoutingSpatialConstraints CreateConstraints()
    {
      _route.DeleteResult() ;
      _route.SyncEndPointsToLinkPoint() ;

      if ( (_route.FromPoint == null) || (_route.ToPoint == null) ) {
        return null ;
      }

      var route = new Connection( new PipeProperty(_route.FromPoint.DiameterConstraint), _route.InsulationType, _route.Temperature) ;

      var (startPos, baseProp) = GetBaseProperty( _route.FromPoint, ( _route.Branches.Any() ? null : _route.ToPoint ),
        _route.IsRoutingPipeRacks, 1.0e-2f ) ;

      var from = _route.FromPoint ;
      var start = Factory.CreateAutoRoutingEndPoint("start", LineId, startPos, To3d_( from.DirectionConstraint ), baseProp, route, true, 0 ) ;
      //ToEndPoint( "start", LineId, _route.FromPoint, route, true, 0 ) ;
      var i = 1 ;

      var to = _route.ToPoint ; 
      var end = Factory.CreateAutoRoutingEndPoint( "leaf" + i++, LineId,
        To3d_( to.PositionConstraint ), To3d_( to.DirectionConstraint ), baseProp, route, false, 0 ) ;
      var endPoints = new List<( IEndPoint, AEndPoint)> { (to, end) } ;
      
      foreach ( var (b, depth) in CollectEndPoint( _route.Branches, 1 ) ) {
        endPoints.Add( ToEndPoint( "leaf" + i++, LineId, b.TermPoint, route, b.IsStart, depth) ) ;
      }
      endPoints.Sort( ( p0, p1 ) => ( p0.Item2.IsStart == p1.Item2.IsStart ) ? 0 : ( p0.Item2.IsStart ? 1 : -1 ) ) ;
      RouteCount = endPoints.Count ;
      return new AutoRoutingSpatialConstraints( (from, start), endPoints ) ;
    }

    private static (Vector3, IPipeProperty) GetBaseProperty(
      IEndPoint start, IEndPoint to, bool isRoutingOnPipeRacks, float tolerance )
    {
      var ss = (Vector3)(start.PositionConstraint ?? Vector3d.zero) ;
      var sd = (Vector3)(start.DirectionConstraint ?? Vector3d.right) ;
      var sProp = new PipeProperty( start.DiameterConstraint ) ;

      if ( to == null ) {
        return GetBaseProperty( ( ss, sd, sProp ), ( Vector3.zero, Vector3.zero, null ), isRoutingOnPipeRacks,
          tolerance ) ;
      }
      
      var es = (Vector3)(to.PositionConstraint ?? Vector3d.zero) ;
      var ed = (Vector3)(to.DirectionConstraint ?? Vector3d.right) ;
      var eProp = new PipeProperty( to.DiameterConstraint ) ;
      return GetBaseProperty( ( ss, sd, sProp ), ( es, ed, eProp ), isRoutingOnPipeRacks, tolerance ) ;
    }

    private static (Vector3 pos, IPipeProperty prop) GetBaseProperty(
      (Vector3 p, Vector3 d, IPipeProperty prop) src, (Vector3 p, Vector3 d, IPipeProperty prop) end,
      bool isRoutingOnPipeRacks, float tolerance )
    {
      // 始/終点に不整合があれば、強制的に修正する（ラックの高さが合わなくなるため）
      IPipeProperty baseProp = src.prop ;
      if ( (end.prop != null) && (src.prop.NPSmm != end.prop.NPSmm) ) {
        baseProp = Extensions.GetMin( src.prop, end.prop ) ;
      }

      if ( isRoutingOnPipeRacks || (end.prop == null) ) {
        return (src.Item1, baseProp);
      }
      
      var pos = src.p ;
      if ( src.d.GetAxis() == AxisDir.X && src.d.GetAxis() == AxisDir.X &&
           Math.Abs( src.p.y - end.p.y ) < tolerance &&
           Math.Abs( src.p.z - end.p.z ) < tolerance ) {
        pos = new Vector3( src.p.x, end.p.y, end.p.z ) ;
      }

      if ( src.d.GetAxis() == AxisDir.Y && end.d.GetAxis() == AxisDir.Y &&
           Math.Abs( src.p.x - end.p.x ) < tolerance &&
           Math.Abs( src.p.z - end.p.z ) < tolerance ) {
        pos = new Vector3( end.p.x, src.p.y, src.p.z ) ;
      }
      return ( pos, baseProp ) ;
    }

    private (IEndPoint, AEndPoint) ToEndPoint( string name, string lineId, IEndPoint src, ILinkedRoute route, bool isStart, int priority )
     => ( src, Factory.CreateAutoRoutingEndPoint( name, lineId, To3d_( src.PositionConstraint ), To3d_( src.DirectionConstraint ),
       new PipeProperty( src.DiameterConstraint ),  route, isStart, priority ) ) ;

    IEnumerable<(IBranch, int)> CollectEndPoint( IEnumerable<IBranch> branches, int currentDepth )
    {
      if ( currentDepth >= 10 ) {
        yield break;
      }
      foreach ( var b in branches ) {
        yield return ( b, currentDepth ) ;
        foreach ( var item in CollectEndPoint( b.Branches, currentDepth+1 ) ) {
          yield return item ;
        }
      }    
    }

    private static Vector3d_? To3d_( Vector3d? v )
    {
      if ( v.HasValue ) {
        return new Vector3d_( v.Value.x, v.Value.y, v.Value.z ) ;
      }
      return null ;
    }

    public string AutoRoutingId
    {
      set => _route.AutoRoutingId = value ;
    }


  }
}