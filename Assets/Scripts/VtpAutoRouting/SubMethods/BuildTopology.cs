using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Topology ;

namespace VtpAutoRouting.SubMethods
{
  public static class BuildTopology
  {
    public static void AttachFlow( IList<(HalfVertex, int)> seedVertices )
    {
      var priorityGroups = seedVertices
        .GroupBy( item => item.Item2 )
        .OrderByDescending( item => item.Key )
        .ToArray() ;

      if ( priorityGroups.Length < 1 ) return ;
      
      foreach ( var group in priorityGroups.Take( priorityGroups.Length-1 ) ) {
        foreach ( var (seed, _) in group ) {
          SetFlowDirectionUntilBifurcation( seed );  
        }
      }
      foreach ( var (seed, _) in priorityGroups.Last() ) {
        SetFlowToAll( seed );
      }
    }

    private static void SetFlowToAll( HalfVertex seed )
    {
      var branched = SetFlowDirectionUntilBifurcation( seed ) ;
      if ( branched == null ) {
        return ;
      }
      foreach ( var next in GetFlowNext( branched ) ) {
        next.Flow = Next( branched.Flow ) ;
        if ( next.Partner != null ) {
          SetFlowToAll( next.Partner ) ;
        }
      }
    }

    private static HalfVertex SetFlowDirectionUntilBifurcation( HalfVertex seed )
    {
      var current = seed ;
      for ( var i = 0 ; i < int.MaxValue ; ++i ) {
        if ( current.LeafEdge == null ) {
          return null ;
        }

        var nextVertices = GetFlowNext( current ).ToArray() ;
        if ( ! nextVertices.Any() ) {
          return null ;
        }

        var next = GetSingleFlowNext( nextVertices ) ;
        if ( next == null ) {
          return current ; // 分岐があって次の方向がわからない
        }
        if ( next.Flow != HalfVertex.FlowType.Undefined ) {
          return null ;
        }
        next.Flow = Next( current.Flow );
        // 端部にTができて freevertex になっているケースがあるための対応
        foreach ( var n in nextVertices.Where( p => p.Flow == HalfVertex.FlowType.Undefined ) ) {
          n.Flow = Next( current.Flow ) ;
        }
        if ( next.Partner == null || next.Partner.Flow != HalfVertex.FlowType.Undefined ) {
          break ;
        }
        next.Partner.Flow = Next( next.Flow ) ;
        current = next.Partner ; 
      }
      return null ;
    }

    private static IEnumerable<HalfVertex> GetFlowNext( HalfVertex current )
    {
      return current.LeafEdge.Vertices
        .Where( v => ( v != current ) && v.Flow == HalfVertex.FlowType.Undefined ) ;
    }
    
    private static HalfVertex.FlowType Next( HalfVertex.FlowType t )
    {
      return ( t == HalfVertex.FlowType.FromAnotherToThis )
        ? HalfVertex.FlowType.FromThisToAnother 
        : HalfVertex.FlowType.FromAnotherToThis ;
    }

    private static HalfVertex GetSingleFlowNext( HalfVertex[] nextVertexCandidate )
    {
      if ( nextVertexCandidate.Length == 1 ) {
        return ( nextVertexCandidate[ 0 ].Flow == HalfVertex.FlowType.Undefined ) ? nextVertexCandidate[ 0 ] : null ;
      }

      var connectedVertices = nextVertexCandidate
        .Where( p => (p.Partner != null) && (p.Partner.Flow == HalfVertex.FlowType.Undefined) )
        .ToArray() ;
      return ( connectedVertices.Length == 1 ) ? connectedVertices[ 0 ] : null ;
    }
  }
}