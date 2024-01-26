using Chiyoda.CAD.Model ;
using Routing ;

namespace VtpAutoRouting.BridgeEntities
{
  public class PipePropertyProvider : IDiameterProvider
  {
    public IPipeProperty Get( float r )
    {
      return new PipeProperty( DiameterFactory.FromOutsideMeter( r ) ) ;
    }
  }
}