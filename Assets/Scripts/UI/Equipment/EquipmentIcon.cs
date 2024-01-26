using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;

namespace Chiyoda.UI
{
  public abstract class EquipmentIcon : DragPlacementIcon<Edge>
  {
    protected override void RemoveFromParent( Edge placement )
    {
      ( placement.Group ?? placement.Document ).RemoveEdge( placement ) ;
    }
  }
}