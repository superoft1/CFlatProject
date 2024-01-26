using Chiyoda.CAD.Core;

namespace Chiyoda.CAD.Model.Electricals
{
  [Entity( EntityType.Type.SubStation )]
  public class SubStation : ElectricalDevices
  {
    public SubStation( Document document ) : base( document )
    {
    }
  }
}