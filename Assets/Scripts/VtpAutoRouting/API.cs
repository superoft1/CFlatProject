using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Topology ;
using Routing ;
using Routing.Model ;

namespace VtpAutoRouting
{
  public static class API
  {
    public static (int ok, int ng, TimeSpan calcTime) CreateRoute( 
     IList<APipeRack> racks, IEnumerable<Route> targetRoutes, IDebugFilePath workingDir = null )
    {
      var start = DateTime.Now ;
      
      var srcLookup = targetRoutes.ToDictionary( ModelConverter.ToAutoRoutingSource ) ;

      var ng = 0 ;
      var total = 0 ;
      foreach ( var (src, result) in Routing.API.Execute( racks, srcLookup.Keys, workingDir ) ) {
        if ( result == null ) {
          continue ;
        }

        total += src.RouteCount ;
        var route = srcLookup[ src ] ;
        
        var error = SubMethods.RegisterPipingPieces.Execute( route, result );

        if ( ! error.HasError ) {
          continue ;
        }

        if ( workingDir != null ) {
          var filepathError = workingDir.PathToRoutingResult( $"error_{route.LineId}.txt" ) ;
          error.ErrorExport( filepathError );
        }

        route.ErrorMessage = error.Message ;
        ng += error.ErrorRouteCount ;
      }
      return ( total - ng, ng, ( DateTime.Now - start ) ) ;
    }

    
  }
}