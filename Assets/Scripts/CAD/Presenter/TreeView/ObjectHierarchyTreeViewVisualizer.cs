//#define SHOW_VERTEX
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;

namespace Chiyoda.CAD.Presenter.TreeView
{
  class ObjectHierarchyTreeViewVisualizer : ITreeViewVisualizer
  {
    public bool WillVisualize( IElement element )
    {
      switch ( element ) {
#if SHOW_VERTEX
        case HalfVertex v when ( v.Partner != null ) :
#else
        case HalfVertex _ :
          return false ;
#endif
        case LeafEdge le :
        case PipingPiece pp when ! ( pp is Equipment ) :
        case Line line :
        case HydraulicStream stream :
          return false ;
        default :
          return true ;
      }
    }

    public string GetNodeName( IElement element )
    {
      return element.Name ?? element.GetType().Name;
    }

    public IElement GetParentForTreeView( IElement element )
    {
      if ( element is HalfVertex he ) {
        return he.Document ;
      }

      var parent = element.Parent;
      while ( parent is LeafEdge ) {
        parent = parent.Parent;
      }
      return parent;
    }

    public IEnumerable<IElement> GetChildrenForTreeView( IElement element )
    {
      var cache = new Queue<IElement>();
      cache.Enqueue( element );

      while ( cache.Any() ) {
        element = cache.Dequeue();

        foreach ( var child in element.Children ) {
          if ( child is LeafEdge ) {
            cache.Enqueue( child );
            continue;
          }

          if ( WillVisualize( child ) ) {
            yield return child;
          }
        }
      }
    }
  }
}
