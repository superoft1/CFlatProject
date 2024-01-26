using System ;
using System.Collections.Generic ;
using UnityEngine;

namespace Chiyoda.CAD.Model
{

  public interface INozzlePlacement
  {
    event EventHandler NozzlePositionChanged ;
    IEnumerable<Nozzle> Nozzles { get ; }
    Vector3d GetNozzleOriginPosition( Nozzle nozzle ) ; 
    Vector3d GetNozzleDirection( Nozzle nozzle ) ;
  }

}
