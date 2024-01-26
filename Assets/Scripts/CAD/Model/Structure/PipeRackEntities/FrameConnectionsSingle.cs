using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  [Entity( EntityType.Type.FrameConnectionSingle )]
  internal class FrameConnectionsSingle : FrameConnections<ConnectionSingleFrame>
  {
    public FrameConnectionsSingle( Document document ) : base( document )
    {}
  }
} 