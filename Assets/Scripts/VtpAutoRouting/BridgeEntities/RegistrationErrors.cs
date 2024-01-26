using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using Chiyoda ;
using Routing ;
using Routing.Process ;

namespace VtpAutoRouting.BridgeEntities
{
  internal class RegistrationErrors
  {
    private readonly Dictionary<IRouteVertex, List<IRegistrationError>> _errors
      = new Dictionary<IRouteVertex, List<IRegistrationError>>() ;

    public void Register( IRouteVertex v, IRegistrationError e )
    {
      if ( _errors.ContainsKey( v ) ) {
        _errors[ v ].Add( e ) ;
      }
      else {
        _errors.Add( v, new List<IRegistrationError>() { e } ) ;
      }
    }

    public bool HasError => _errors.Any() ;
    
    public string Message 
      => _errors.Any()
      ? _errors.Values.SelectMany( es => es ).Select( e => e.Message ).Aggregate( ( s, m ) => $"{s}, {m}" )
    : "No error" ;

    public int ErrorRouteCount => _errors.Count ;

    public void ErrorExport( string filepath )
    {
      using ( var s = new StreamWriter( filepath ) ) {
        if ( ! _errors.Any() ) {
          s.WriteLine( "No error." ) ;
          return ;
        }

        s.WriteLine( $"{ErrorRouteCount} errors found." ) ;
        foreach ( var (checkPoint, routingErrors) in _errors ) {
          s.WriteLine( $"EndPoint Position : {checkPoint.Position.ToString()}" ) ;
          routingErrors.ForEach( e => ExportError( s, e ) );
        }
      }
    }

    private static void ExportError( TextWriter s, IRegistrationError e )
    {
      s.WriteLine( $" |- Msg: {e.Message}" ) ;
      foreach ( var errorPos in e.ErrorPositions ) {
        s.WriteLine( $"     |- Error Position : {errorPos.Position}" ) ;
      }
    }
  }
}