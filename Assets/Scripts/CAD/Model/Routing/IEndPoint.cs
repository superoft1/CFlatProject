using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Routing
{
  
  public interface IEndPoint : IElement, IRoutingConstraint
  {
    new string Name { get ; set ; }
    
    string LineId { get ; }

    HalfVertex LinkPoint { get ; }

    void SyncWith( IRoutingConstraint src ) ;
  }
}