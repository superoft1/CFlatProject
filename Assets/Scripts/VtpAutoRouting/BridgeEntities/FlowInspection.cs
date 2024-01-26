using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Model.Routing ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;
using VtpAutoRouting.BrdigeEntities ;

namespace VtpAutoRouting.BridgeEntities
{
  internal static class FlowInspection
  {
    public static bool Run( Route route, IEnumerable<LeafEdge> routingResult )
    {
      var (freeVertices, error) = InspectFlowDirection( routingResult ) ;
      if ( error.Any() ) {
        // デバッグ用に エラーの位置は free にする
        error.ForEach( e => e.Partner = null );
        return false ;
      }

       
      if ( route.FromPoint.LinkPoint == null ) { //LineList からのケース：  端点に nozzle(halfVertex) がない
        if( HasInconsistentTermDirection( route, freeVertices ) ) {
          return false ;
        }
        return IsAllTermPositionReachable( freeVertices ) ;
      }
      else {
        if ( HasInconsistentTermDirection( route ) ) {
          return false ;
        }

        var allTerminal = route.GetAllTerminal().ToArray() ;
        var allTerminalVertices = allTerminal.Select( p => p.LinkPoint ).Where( v => v != null ).ToArray() ;
        return ( allTerminal.Length == allTerminalVertices.Length )
               && IsAllTermPositionReachable( allTerminalVertices ) ;
      }
    }

    private static bool HasInconsistentTermDirection( Route route, IList<HalfVertex> freeVertices )
    {
      var idealFlow = new Dictionary<IEndPoint, HalfVertex.FlowType>()
      {
        { route.FromPoint, Mate(HalfVertex.FlowType.FromThisToAnother ) },
        { route.ToPoint, Mate( HalfVertex.FlowType.FromAnotherToThis) }
      } ;
      foreach ( var b in route.GetAllBranch() ) {
        idealFlow.Add( b.TermPoint, Mate( IdealFlow( b ) ) );
      }
      
      return Matching( freeVertices, idealFlow.Keys.ToArray() )
        .Any( item => item.v.Flow != idealFlow[ item.p ] ) ;
    }
    
    private static bool HasInconsistentTermDirection( Route route )
    {
      if ( route.FromPoint.LinkPoint?.Flow != HalfVertex.FlowType.FromThisToAnother ) {
        return true ;
      }
      if ( route.ToPoint.LinkPoint?.Flow != HalfVertex.FlowType.FromAnotherToThis ) {
        return true ;
      }
      
      return route.GetAllBranch().Any( b => b.TermPoint.LinkPoint.Flow != IdealFlow( b ) ) ;
    }
    
    private static HalfVertex.FlowType Mate( HalfVertex.FlowType t )
    {
      switch ( t ) {
        case HalfVertex.FlowType.FromAnotherToThis: 
          return HalfVertex.FlowType.FromThisToAnother ;
        case HalfVertex.FlowType.FromThisToAnother:
          return HalfVertex.FlowType.FromAnotherToThis ;
        case HalfVertex.FlowType.Undefined :
          return HalfVertex.FlowType.Undefined ;
        default:
          return HalfVertex.FlowType.Undefined ;
      }      
    }

    private static HalfVertex.FlowType IdealFlow( IBranch b ) => b.IsStart
      ? HalfVertex.FlowType.FromThisToAnother
      : HalfVertex.FlowType.FromAnotherToThis ;
    
    private static bool IsAllTermPositionReachable( IList<HalfVertex> freeVertices )
    {
      var routeTerms = new HashSet<HalfVertex>(
        FollowRoute( freeVertices[0], hv => false ));
      return routeTerms.Count == (freeVertices.Count-1) 
             && freeVertices.Skip( 1 ).All( v => routeTerms.Contains( v ) ) ;
    }

    private static IEnumerable<(HalfVertex v, IEndPoint p)> Matching( 
      IList<HalfVertex> freeVertices, IEndPoint[] points )
    {
      (IEndPoint p, double d) FindNearestPoint( Vector3d v ) => points
        .Select( p => ( p, ( v - p.PositionConstraint.Value ).sqrMagnitude ) )
        .OrderBy( tuple => tuple.sqrMagnitude )
        .FirstOrDefault();
      var vertexToEndPoint = new Dictionary<HalfVertex, IEndPoint>() ;
      freeVertices.Select( v => ( v, FindNearestPoint( v.GlobalPoint ) ) )
        .ForEach( tuple => {
          if ( tuple.Item2.d < 0.01 ) {
            vertexToEndPoint.Add( tuple.v, tuple.Item2.p ) ;
          }
        } );

      (HalfVertex, double) FindNearestVertex( Vector3d p ) => freeVertices
        .Select( v => ( v, ( p - v.GlobalPoint ).sqrMagnitude ) )
        .OrderBy( item => item.sqrMagnitude )
        .First() ;
      foreach ( var ( (v, d), p) in points.Select( p => ( FindNearestVertex( p.PositionConstraint.Value ), p ) ) ) {
        if ( d > 0.01 || !vertexToEndPoint.TryGetValue( v, out var nearest ) ) {
          continue ;
        }        
        if ( nearest == p ) {
          yield return ( v, p ) ;
        }
      }
    }

    private static bool IsAllTermPositionReachable( HalfVertex[] terminals )
    {
      var terms = new HashSet<HalfVertex>( terminals.Skip( 1 ) );
      var reachableTerms = new HashSet<HalfVertex>(
        FollowRoute( terminals[ 0 ].Partner, v => terms.Contains( v ) ) ) ;
      
      return ( terms.Count == reachableTerms.Count )             
             && terms.SetEquals( reachableTerms ) ;
    }

    private static IEnumerable<HalfVertex> FollowRoute( HalfVertex seed, Predicate<HalfVertex> additionalTermCondition )
    {
      var current = seed ;
      for ( var i = 0 ; i < int.MaxValue ; ++i ) {
        if ( additionalTermCondition( current ) ) {
          yield return current ;
          yield break;
        }
        
        if ( current?.LeafEdges == null ) {
          yield break;
        }

        var nextVertices = current.LeafEdge.Vertices.Where( v => v != current ).ToArray() ;
        
        if( nextVertices.Length != 1 ) {
          foreach ( var v in nextVertices ) {
            if ( (v.Partner == null) || additionalTermCondition( v ) ) {
              yield return v ;
            }
            foreach ( var t in FollowRoute( v.Partner, additionalTermCondition ) ) {
              yield return t ;
            }
          }
          yield break;
        }
        
        if ( (nextVertices[0].Partner == null) || additionalTermCondition( nextVertices[ 0 ] ) ) {
          yield return nextVertices[ 0 ] ;
          yield break;
        }
        current = nextVertices[ 0 ].Partner ;
      }
    }

    private static (IList<HalfVertex> freeVertices, IList<HalfVertex> errorVertices)
      InspectFlowDirection( IEnumerable<LeafEdge> routingResult )
    {
      var freeVertices = new List<HalfVertex>() ;
      var error = new List<HalfVertex>() ;
      foreach ( var e in routingResult ) {
        error.AddRange( e.Vertices.Where( v => !IsValid( v ) ) );
        freeVertices.AddRange( e.GetFreeVertex() );
      }
      return ( freeVertices, error ) ;
    }

    private static bool IsValid( HalfVertex v )
    {
      if ( v.Flow == HalfVertex.FlowType.Undefined ) {
        return false ;
      }
      if ( v.Partner == null ) {
        return true ;
      }

      if ( v.Partner.Flow == HalfVertex.FlowType.Undefined ) {
        return false ;
      }
      return v.Partner.Flow != v.Flow ; // = [in/out の整合性が取れている]
    }
  }
}