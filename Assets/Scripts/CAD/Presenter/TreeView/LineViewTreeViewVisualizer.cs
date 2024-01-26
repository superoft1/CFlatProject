using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;

namespace Chiyoda.CAD.Presenter.TreeView
{
  class LineViewTreeViewVisualizer : ITreeViewVisualizer
  {
    public bool WillVisualize( IElement element )
    {
      if ( element is Document || element is Line ) return true;

      return false;
    }

    public string GetNodeName( IElement element )
    {
      return element.Name ?? element.GetType().Name;
    }

    public IElement GetParentForTreeView( IElement element )
    {
      // TODO
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
