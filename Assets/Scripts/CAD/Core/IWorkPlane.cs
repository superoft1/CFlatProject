using System.Collections.Generic ;
using UnityEngine ;

namespace Chiyoda.CAD.Core
{
  public interface IWorkPlane : IElement
  {
    IEnumerable<IRegion> Regions { get ; }
  }
}