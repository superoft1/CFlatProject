using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Topology
{
  public interface IGroup : IElement
  {
    IEnumerable<Edge> EdgeList { get; }
    int EdgeCount { get; }

    bool AddEdge( Edge edge );
    bool RemoveEdge( Edge edge );
    bool ReplaceEdge( int index, Edge newEdge );
    bool ReplaceEdge( Edge oldEdge, Edge newEdge );

    LocalCodSys3d GlobalCod { get; }
  }
}
