using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Model.Routing ;
using Chiyoda.CAD.Topology ;

namespace VtpAutoRouting.BridgeEntities
{
  internal static class RouteServiceFunctions
  {
    public static IEnumerable<IEndPoint> GetAllTerminal( this Route r )
    {
      yield return r.FromPoint ;
      yield return r.ToPoint ;
      foreach ( var p in r.Branches.SelectMany( FlattenBranch ) ) {
        yield return p.TermPoint ;
      }
    }

    public static IEnumerable<HalfVertex> GetAllTerminalVertices( this Route r )
      => r.GetAllTerminal().Select( p => p.LinkPoint ).Where( v => v != null ) ;

    public static IEnumerable<IBranch> GetAllBranch( this Route r )
      => r.Branches.SelectMany( FlattenBranch ) ;
    
    private static IEnumerable<IBranch> FlattenBranch( IBranch b )
      => (new[] { b }).Concat( b.Branches.SelectMany( FlattenBranch ) ) ;
  }
}