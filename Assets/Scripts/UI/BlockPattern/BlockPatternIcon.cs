using Chiyoda.CAD.Topology ;

namespace Chiyoda.UI
{
  public abstract class BlockPatternIcon : DragPlacementIcon<Edge>
  {
    protected override void RemoveFromParent( Edge placement )
    {
      ( placement.Group ?? placement.Document ).RemoveEdge( placement ) ;
    }
  }
}