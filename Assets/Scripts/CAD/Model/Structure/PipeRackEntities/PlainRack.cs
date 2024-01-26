using System.Collections ;
using Chiyoda ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  [Entity( EntityType.Type.PlainRack )]
  internal class PlainRack : RackBase<FrameConnectionsSingle>
  {
    public PlainRack( Document document ) : base( document )
    {}

    public void SetLayerHeight( int layer, double h )
    {
      Connections.ForEach( u => u.SetHeight( layer, h ) );
    }
    
  } ;
}
