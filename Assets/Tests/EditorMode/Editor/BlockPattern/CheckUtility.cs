using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using NUnit.Framework ;
using UnityEngine ;

namespace Tests.EditorMode.Editor.BlockPattern
{
  public static class CheckUtility
  {
    public static void DistanceParallelCheck(Document doc, BlockEdge blockEdge)
    {
      doc.MaintainEdgePlacement() ;
      var vs = blockEdge.GetEveryVertex() ;
      foreach ( var v in vs.Where(v=>v.Partner != null) ) {
        DistanceParallelCheckImpl( blockEdge, v, v.Partner ) ;
      }
    }

    private static  void DistanceParallelCheckImpl(BlockEdge blockEdge, HalfVertex v, HalfVertex p)
    {
      var vp = v.LeafEdge.PipingPiece ;
      var vl = v.LeafEdge ;
      var pp = p.LeafEdge.PipingPiece ;
      var pl = p.LeafEdge ;
      var vgVec = vl.GlobalCod.GlobalizeVector( ( v.GetConnectVector() ) ) ;
      var pgVec = pl.GlobalCod.GlobalizeVector( ( p.GetConnectVector() ) ) ;
        
      Assert.AreEqual( 0, Vector3d.Distance( v.GlobalPoint, p.GlobalPoint ), Tolerance.DistanceTolerance,
        $"Not Connected: {blockEdge.ObjectName}, [{vp.Ancestor<CompositeEdge>().Name}, {vp.Name}, {vp.ObjectName}] <--> [{pp.Ancestor<CompositeEdge>().Name}, {pp.Name}, {pp.ObjectName}]" ) ;

      Assert.IsTrue( vgVec.IsParallelTo( pgVec ),
        $"Not Parallel: {blockEdge.ObjectName}, [{vp.Ancestor<CompositeEdge>().Name}, {vp.Name}, {vp.ObjectName}] <--> [{pp.Ancestor<CompositeEdge>().Name}, {pp.Name}, {pp.ObjectName}]" ) ;
    }
  }
}