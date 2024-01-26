using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.EndTopTypePump.Data8 ;
using NUnit.Framework ;
using UnityEngine ;

namespace Tests.PlayMode.BlockPattern.Pump
{
  /// <summary>
  /// 隣接するコンポーネントで接続位置が同一か、折れ曲がって接続していないかどうかのテスト
  /// </summary>
  public class PumpStraightConnectivityTest
  {
    private Document _curDoc = null ;
    private GameObject _gameObject = null ;


    // クラス内の最初のテストが実行される前に一度だけ実行される
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _gameObject = new GameObject() ;
      _gameObject.AddComponent<ImportManager>() ;
      Assert.IsNotNull( ImportManager.Instance() ) ;
    }

    // クラス内の最後のテストが実行された後に一度だけ実行される
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
      foreach ( Transform child in _gameObject.transform ) {
        Object.DestroyImmediate( child.gameObject ) ;
        child.parent = null ;
      }

      Object.DestroyImmediate( _gameObject ) ;
      Assert.IsNull( ImportManager.Instance() ) ;
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
      DocumentCollection.Instance.Close( _curDoc ) ;
    }

    [Test]
    [Category( "EndTop" )]
    public void Data8Test()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockPatternArray) CreateEndTopTypePumpData8.Create( null ).Parent ) ;
    }
  }
}