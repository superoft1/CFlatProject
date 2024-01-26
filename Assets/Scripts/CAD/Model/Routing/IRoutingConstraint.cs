using System.Collections.Generic ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Routing
{
  public interface IRoutingConstraint
  {
    Vector3d? PositionConstraint { get ; }
    Vector3d? DirectionConstraint { get ;  }
    
    Diameter DiameterConstraint { get ;  }
    
    IEnumerable<IRoutingEdge> Links { get ; }
  }
}