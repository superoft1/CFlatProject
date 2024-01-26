using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Core;

namespace Chiyoda.CAD.Presenter.TreeView
{
  interface ITreeViewVisualizer
  {
    bool WillVisualize( IElement element );
    string GetNodeName( IElement element );
    IElement GetParentForTreeView( IElement element );
    IEnumerable<IElement> GetChildrenForTreeView( IElement element );
  }
}
