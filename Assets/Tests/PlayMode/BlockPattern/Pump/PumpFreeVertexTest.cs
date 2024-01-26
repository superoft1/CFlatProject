using System.Linq ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.EndTopTypePump.Data8 ;
using NUnit.Framework ;
using UnityEngine ;

namespace Tests.PlayMode.BlockPattern.Pump
{
  public class PumpFreeVertexTest
  {
    private Document _curDoc = null ;
    private GameObject _gameObject = null ;
    
    // クラス内の最初のテストが実行される前に一度だけ実行される
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _gameObject = new GameObject();
      _gameObject.AddComponent<ImportManager>();
      Assert.IsNotNull( ImportManager.Instance() );
    }
    
    // クラス内の最後のテストが実行された後に一度だけ実行される
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
      foreach ( Transform child in _gameObject.transform ) {
        Object.DestroyImmediate(child.gameObject);
        child.parent = null ;
      }
      Object.DestroyImmediate(_gameObject);
      Assert.IsNull( ImportManager.Instance() );
    }
    
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
    public void FreeVertexName2Test()
    {
      CreateEndTopTypePumpData8.Create(null);
      _curDoc.MaintainEdgePlacement() ;

      var blockPatternArray = _curDoc.EdgeList.OfType<CompositeBlockPattern>().FirstOrDefault(b=>b.BaseBlockPattern.Type == BlockPatternType.Type.EndTopTypePump);//EndTopTypeのポンプを使用する
      Assert.IsNotNull(blockPatternArray);

      // Instrumentsの名前を書き換える
      Equipment first = blockPatternArray.GetBlockPattern( 0 ).Equipments.FirstOrDefault() ;
      if (first != null) first.EquipNo = "171-P003A";
      Equipment second = blockPatternArray.GetBlockPattern( 1 ).Equipments.FirstOrDefault();
      if (second != null) second.EquipNo = "171-P003B";

      // 端点にバーテックス名を設定
      blockPatternArray.SetVertexName( "DischargeEnd", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
      blockPatternArray.SetVertexName( "SuctionEnd", "N-1" , HalfVertex.FlowType.FromAnotherToThis) ;
      
      var vs = blockPatternArray.Vertices.ToList() ;
      Assert.IsNotNull( vs.FirstOrDefault(v=>v.Name == "N-1") ) ;
      Assert.IsNotNull( vs.FirstOrDefault(v=>v.Name == "N-2") ) ;
    }
  }
}
