using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.Filter ;
using NUnit.Framework ;

namespace Tests.EditorMode.Editor.BlockPattern.Filter
{
  /// <summary>
  /// 隣接するコンポーネントで接続位置が同一か、折れ曲がって接続していないかどうかのテスト
  /// </summary>
  public class FilterStraightConnectivityTest
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
    [Ignore("動くようになったら実行")]
    public void FilterFS_A1_1_STest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockEdge) new FilterFS_A1_1_S( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Ignore("動くようになったら実行")]
    public void FilterFS_A1_1_GTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockEdge) new FilterFS_A1_1_G( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    public void FilterFS_A2_2_STest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockEdge) new FilterFS_A2_2_S( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    public void FilterFS_A2_2_GTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockEdge) new FilterFS_A2_2_G( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Ignore("動くようになったら実行")]
    public void FilterFD_B1_1_STest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockEdge) new FilterFD_B1_1_S( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    [Ignore("動くようになったら実行")]
    public void FilterFD_B1_1_GTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockEdge) new FilterFD_B1_1_G( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    public void FilterFD_B2_2_STest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockEdge) new FilterFD_B2_2_S( _curDoc ).Create( null ).Parent ) ;
    }

    [Test]
    public void FilterFD_B2_2_GTest()
    {
      CheckUtility.DistanceParallelCheck( _curDoc,
        (BlockEdge) new FilterFD_B2_2_G( _curDoc ).Create( null ).Parent ) ;
    }
  }
}