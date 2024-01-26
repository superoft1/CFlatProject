using System.Collections.Generic ;
using Routing ;

namespace VtpAutoRouting.BridgeEntities
{
  public interface IRegistrationError 
  {
    string Message { get ; }
    IEnumerable< IRouteVertex > ErrorPositions { get ; }
  }
}