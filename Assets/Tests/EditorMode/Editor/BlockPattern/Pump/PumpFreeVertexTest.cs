using System.Linq ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.EndTopTypePump.Data8 ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_A1_V_H_S ;
using NUnit.Framework ;
using UnityEngine ;

namespace Tests.EditorMode.Editor.BlockPattern.Pump
{
  public class PumpFreeVertexTest
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
    public void FreeVertexNameTest()
    {
      var blockPattern = new EndTopTypePumpET_A1_V_H_S( _curDoc ).Create( null ) ;

      var blockArray = (BlockPatternArray) blockPattern.Parent ;

      _curDoc.MaintainEdgePlacement() ;
      
      blockArray.SetVertexName( "DischargeEnd", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
      blockArray.SetVertexName( "SuctionEnd", "N-1" , HalfVertex.FlowType.FromAnotherToThis) ;

      var vs = blockArray.Vertices.ToList() ;
      Assert.IsNotNull( vs.FirstOrDefault(v=>v.Name == "N-1") ) ;
      Assert.IsNotNull( vs.FirstOrDefault(v=>v.Name == "N-2") ) ;
    }
  }
}
