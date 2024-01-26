using System ;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_A1_H_H ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_A2_S_S_O ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.BlockPattern.Pump
{
  public class PumpFlowTypeTest
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
      DocumentCollection.Instance.Close(_curDoc  ) ;
    }
    
    [Test]
    public void FlowTest1()
    {
      var blockPattern = new SideSideTypePumpSS_A1_H_H( _curDoc ).Create( null ) ;
      var blockArray = (BlockPatternArray) blockPattern.Parent ;
      _curDoc.MaintainEdgePlacement() ;

      blockArray.SetFlowType() ;

      Assert.AreEqual( blockArray.GetEveryVertex().Any( v => v.Flow == HalfVertex.FlowType.Undefined ), false ) ;
      
      blockArray.GetBlockPattern( 0 ).GetProperty( "SuctionJoinType" ).Value = Convert.ToDouble(CompositeBlockPattern.JoinType.Independent) ;
      blockArray.GetBlockPattern( 0 ).GetProperty( "DischargeJoinType" ).Value = Convert.ToDouble(CompositeBlockPattern.JoinType.Independent) ;
      _curDoc.MaintainEdgePlacement() ;
      blockArray.SetFlowType() ;
      Assert.AreEqual( blockArray.GetEveryVertex().Any( v => v.Flow == HalfVertex.FlowType.Undefined ), false ) ;
    }

    
    [Test]
    public void FlowTest2()
    {
      var blockPattern = new TopTopTypePumpTT_A2_S_S_O( _curDoc ).Create( null ) ;
      var blockArray = (BlockPatternArray) blockPattern.Parent ;
      _curDoc.MaintainEdgePlacement() ;

      blockArray.SetFlowType() ;

      Assert.AreEqual( blockArray.GetEveryVertex().Any( v => v.Flow == HalfVertex.FlowType.Undefined ), false ) ;
      
      blockArray.GetBlockPattern( 0 ).GetProperty( "SuctionJoinType" ).Value = Convert.ToDouble(CompositeBlockPattern.JoinType.Independent) ;
      blockArray.GetBlockPattern( 0 ).GetProperty( "DischargeJoinType" ).Value = Convert.ToDouble(CompositeBlockPattern.JoinType.Independent) ;
      blockArray.GetBlockPattern( 0 ).GetProperty( "MinimumFlowJoinType" ).Value = Convert.ToDouble(CompositeBlockPattern.JoinType.Independent) ;
      _curDoc.MaintainEdgePlacement() ;
      blockArray.SetFlowType() ;
      Assert.AreEqual( blockArray.GetEveryVertex().Any( v => v.Flow == HalfVertex.FlowType.Undefined ), false ) ;
    }
  }
}
