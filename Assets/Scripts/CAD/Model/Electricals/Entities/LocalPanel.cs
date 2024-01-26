using Chiyoda.CAD.Core;

namespace Chiyoda.CAD.Model.Electricals
{
  [Entity( EntityType.Type.LocalPanel )]
  public class LocalPanel : ElectricalDevices
  {
    public LocalPanel( Document document ) : base( document )
    {
    }
  }
}