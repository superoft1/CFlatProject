using Chiyoda.CAD.Model ;

namespace Chiyoda.UI
{
  public abstract class ElectoricalIcon : DragPlacementIcon<ElectricalDevices>
  {
    protected override void RemoveFromParent( ElectricalDevices placement )
    {
      placement.Document.Structures.Remove( placement ) ;
    }
  }
}