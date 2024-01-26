using Chiyoda.CAD.Core;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  [Entity( EntityType.Type.PipeRack )]
  internal class PipeRackSingle : AutoRoutingRackBase<FrameConnectionsSingle>
  {
    public PipeRackSingle( Document document ) : base( document )
    {
    }
  }
}