using System.Collections.Generic ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model
{
  public abstract class WorkPlane : Entity, IWorkPlane
  {
    public abstract IEnumerable<IRegion> Regions { get ; }

    protected WorkPlane( Document document ) : base( document )
    {
    }
  }
}