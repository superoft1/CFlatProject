using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
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

namespace Tests.EditorMode.Editor.BlockPattern.Pump
{
  /// <summary>
  /// 隣接するコンポーネントで接続位置が同一か、折れ曲がって接続していないかどうかのテスト
  /// </summary>
  public class PumpStraightConnectivityTest
  {
    private Document _curDoc = null ;

    // 各テスト実行前にそれぞれ実行される
    [SetUp]
    public void SetUp()
    {
      _curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew() ;
    }

    // 各テスト実行後にそれぞれ実行される
    [TearDown]
    public void TearDown()
    {
      DocumentCollection.Instance.Close( _curDoc ) ;
    }


    [Test]
    [Category( "EndTop" )]
    public void ET_A1_H_H_LTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new EndTopTypePumpET_A1_H_H_L( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "EndTop" )]
    public void ET_A1_V_H_STest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new EndTopTypePumpET_A1_V_H_S( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "EndTop" )]
    public void ET_A1_V_V_LTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new EndTopTypePumpET_A1_V_V_L( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "EndTop" )]
    public void ET_A2_H_H_LTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new EndTopTypePumpET_A2_H_H_L( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "EndTop" )]
    public void ET_B1_H_H_LTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new EndTopTypePumpET_B1_H_H_L( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "EndTop" )]
    public void ET_B1_V_H_LTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new EndTopTypePumpET_B1_V_H_L( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "EndTop" )]
    public void ET_B1_V_H_STest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new EndTopTypePumpET_B1_V_H_S( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "EndTop" )]
    public void ET_B2_H_H_LTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new EndTopTypePumpET_B2_H_H_L( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "EndTop" )]
    public void ET_C1_H_H_LTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new EndTopTypePumpET_C1_H_H_L( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "EndTop" )]
    public void ET_C1_V_V_LTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new EndTopTypePumpET_C1_V_V_L( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "EndTop" )]
    public void ET_C2_H_H_LTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new EndTopTypePumpET_C2_H_H_L( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "SideSide" )]
    public void SS_A1_H_HTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new SideSideTypePumpSS_A1_H_H( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "SideSide" )]
    public void SS_A1_V_VTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new SideSideTypePumpSS_A1_V_V( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "SideSide" )]
    public void SS_B1_H_HTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new SideSideTypePumpSS_B1_H_H( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "SideSide" )]
    public void SS_B1_V_VTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new SideSideTypePumpSS_B1_V_V( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "SideSide" )]
    public void SS_C1_H_HTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new SideSideTypePumpSS_C1_H_H( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "SideSide" )]
    public void SS_C1_V_VTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new SideSideTypePumpSS_C1_V_V( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "TopTop" )]
    public void TT_A1_S_G_NTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new TopTopTypePumpTT_A1_S_G_N( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "TopTop" )]
    public void TT_A1_S_S_NTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new TopTopTypePumpTT_A1_S_S_N( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "TopTop" )]
    public void TT_A2_S_S_CTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new TopTopTypePumpTT_A2_S_S_C( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "TopTop" )]
    public void TT_A2_S_S_OTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new TopTopTypePumpTT_A2_S_S_O( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "TopTop" )]
    public void TT_B1_S_G_NTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new TopTopTypePumpTT_B1_S_G_N( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "TopTop" )]
    public void TT_B1_S_S_NTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new TopTopTypePumpTT_B1_S_S_N( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "TopTop" )]
    public void TT_B2_S_S_CTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new TopTopTypePumpTT_B2_S_S_C( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "TopTop" )]
    public void TT_B2_S_S_OTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new TopTopTypePumpTT_B2_S_S_O( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "TopTop" )]
    public void TT_C1_S_G_NTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new TopTopTypePumpTT_C1_S_G_N( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "TopTop" )]
    public void TT_C1_S_S_NTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new TopTopTypePumpTT_C1_S_S_N( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "TopTop" )]
    public void TT_C2_S_S_CTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new TopTopTypePumpTT_C2_S_S_C( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Category( "TopTop" )]
    public void TT_C2_S_S_OTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) new TopTopTypePumpTT_C2_S_S_O( _curDoc ).Create( null ).Parent ) ;
    }
  }
}