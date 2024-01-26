using System.Collections.Generic ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model
{
  public class Platform : WorkPlane
  {
    private readonly MementoList<IRegion> _regions ;

    public override IEnumerable<IRegion> Regions => _regions ;

    public Platform( Document document ) : base( document )
    {
      _regions = new MementoList<IRegion>( this, 1 ) ;
    }
  }
}