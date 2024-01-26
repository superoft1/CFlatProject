using System.Collections.Generic ;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.EndTopTypePump.Data8 ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_A1_H_H_L ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_A1_V_H_S ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_A1_V_V_L ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_A2_H_H_L ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_B1_H_H_L ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_B1_V_H_L ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_B1_V_H_S ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_B2_H_H_L ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_C1_H_H_L ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_C1_V_V_L ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_C2_H_H_L ;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_A1_H_H ;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_A1_V_V ;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_B1_H_H ;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_B1_V_V ;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_C1_H_H ;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_C1_V_V ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_A1_S_G_N ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_A1_S_S_N ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_A2_S_S_C ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_A2_S_S_O ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_B1_S_G_N ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_B1_S_S_N ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_B2_S_S_C ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_B2_S_S_O ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_C1_S_G_N ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_C1_S_S_N ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_C2_S_S_C ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_C2_S_S_O ;
using NUnit.Framework ;
using UnityEngine ;

namespace Tests.PlayMode.BlockPattern
{
  public static class CheckUtility
  {
    public static void DistanceParallelCheck(Document doc, BlockPatternArray patternArray)
    {
      doc.MaintainEdgePlacement() ;
      var vs = patternArray.GetEveryVertex() ;
      foreach ( var v in vs.Where(v=>v.Partner != null) ) {
        DistanceParallelCheckImpl( patternArray, v, v.Partner ) ;
      }
    }

    private static  void DistanceParallelCheckImpl(BlockPatternArray blockArray, HalfVertex v, HalfVertex p)
    {
      var vp = v.LeafEdge.PipingPiece ;
      var vl = v.LeafEdge ;
      var pp = p.LeafEdge.PipingPiece ;
      var pl = p.LeafEdge ;
      var vgVec = vl.GlobalCod.GlobalizeVector( ( v.GetConnectVector() ) ) ;
      var pgVec = pl.GlobalCod.GlobalizeVector( ( p.GetConnectVector() ) ) ;
        
      Assert.AreEqual( 0, Vector3d.Distance( v.GlobalPoint, p.GlobalPoint ), Tolerance.DistanceTolerance,
        $"Not Connected: {blockArray.ObjectName}, [{vp.Ancestor<CompositeEdge>().Name}, {vp.Name}, {vp.ObjectName}] <--> [{pp.Ancestor<CompositeEdge>().Name}, {pp.Name}, {pp.ObjectName}]" ) ;

      Assert.IsTrue( vgVec.IsParallelTo( pgVec ),
        $"Not Parallel: {blockArray.ObjectName}, [{vp.Ancestor<CompositeEdge>().Name}, {vp.Name}, {vp.ObjectName}] <--> [{pp.Ancestor<CompositeEdge>().Name}, {pp.Name}, {pp.ObjectName}]" ) ;
    }
  }
}