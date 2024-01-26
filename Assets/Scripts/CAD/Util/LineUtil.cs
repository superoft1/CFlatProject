using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

namespace Chiyoda.CAD.Util
{
  public static class LineUtil
  {
    public static void EraseByPipingPiece( this Line line, PipingPiece pp )
    {
      var edge = line.LeafEdges.FirstOrDefault( le => le.PipingPiece == pp );
      if (null == edge) return;
      (edge.Parent as IGroup)?.RemoveEdge( edge );
      edge.Line = null;
      if ( ! line.LeafEdges.Any() ) {
        ( line.Parent as Document )?.RemoveLine( line.LineId, false ) ;
      }
    }
  }
}
