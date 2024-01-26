using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Topology ;
using Chiyoda.Routing ;
using Routing ;
using VtpAutoRouting.BridgeEntities ;

namespace VtpAutoRouting
{
  internal static class ModelConverter
  {
     public static IDebugFilePath ToDebugFilePath( DirectoryUtility dir ) => new BrdigeEntities.DebugPath( dir );
     
     public static IAutoRoutingSource ToAutoRoutingSource( Route r ) => new RoutingSource(  r  );

     public static IEnumerable<IAutoRoutingSource> ToAutoRoutingSources( IEnumerable<Route> routes ) =>
       routes.Select( r => ToAutoRoutingSource( r ) ) ;
  }
}