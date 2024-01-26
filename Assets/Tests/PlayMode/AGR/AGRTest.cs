using System ;
using System.Data ;
using System.IO ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using NUnit.Framework ;
using UnityEngine ;
using Object = UnityEngine.Object ;

namespace Tests.PlayMode.AGR
{
  /// <summary>
  /// AGRを読み込んでProdrawまで実行した時に例外が発生しない、座標が飛んでいない事をチェック
  /// 自動ルーティングのチェックまではしていない
  /// </summary>
  public class AGRTest
  {
    private Document _curDoc = null ;
    private DataSet _dataSet ;
    private GameObject _gameObject = null ;


    // クラス内の最初のテストが実行される前に一度だけ実行される
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      _curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew() ;
      _dataSet = AGRUnitView.InstrumentsDataSet() ;
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
      DocumentCollection.Instance.Close( _curDoc ) ;
    }


    [Test]
    [Order( 0 )]
    public void InstrumentsDataSetTest()
    {
      try {
        _dataSet = AGRUnitView.InstrumentsDataSet() ;
      }
      catch ( Exception e ) {
        Assert.Fail( e.Message ) ;
      }
    }

    [Test]
    [Order( 1 )]
    public void ImportStructureListTest()
    {
      try {
        var rackPath = Path.Combine( ImportManager.StructuresPath(), "AllPipeRack.csv" ) ;
        StructureImporter.ImportStructureList( rackPath, null ) ;
      }
      catch ( Exception e ) {
        Assert.Fail( e.Message ) ;
      }
    }

    [Test]
    [Order( 2 )]
    public void FineTuneSubPipeRackTest()
    {
      try {
        AGRUnitView.FineTuneSubPipeRack( DocumentCollection.Instance.Current.Structures ) ;
      }
      catch ( Exception e ) {
        Assert.Fail( e.Message ) ;
      }
    }

    [Test]
    [Order( 3 )]
    public void ImportNoPipesBlockPatternsTest()
    {
      try {
        AGRUnitView.ImportNoPipesBlockPatterns( _dataSet ) ;
      }
      catch ( Exception e ) {
        Assert.Fail( e.Message ) ;
      }
    }

    [Test]
    [Order( 4 )]
    public void ImportFixedBlockPatternsTest()
    {
      try {
        AGRUnitView.ImportFixedBlockPatterns( _dataSet ) ;
      }
      catch ( Exception e ) {
        Assert.Fail( e.Message ) ;
      }
    }

    [Test]
    [Order( 5 )]
    public void ImportAdjustableBlockPatternsTest()
    {
      try {
        AGRUnitView.ImportAdjustableBlockPatterns( _dataSet ) ;
      }
      catch ( Exception e ) {
        Assert.Fail( e.Message ) ;
      }
    }

    [Test]
    [Order( 6 )]
    public void SetupDummyRacksTest()
    {
      try {
        AGRUnitView.SetupDummyRacks() ;
      }
      catch ( Exception e ) {
        Assert.Fail( e.Message ) ;
      }
    }

    [Test]
    [Order( 7 )]
    public void ImportProDrawTest()
    {
      try {
        AGRUnitView.ImportProdrawStatic() ;
      }
      catch ( Exception e ) {
        Assert.Fail( e.Message ) ;
      }
    }

    [Test]
    [Order( 8 )]
    public void RegionTest()
    {
      _curDoc.MaintainEdgePlacement() ;

      //      doc.Area.EWOrigin = 400;
      //      doc.Area.NSOrigin = -20;
      //      doc.Area.EWWidth = 300;
      //      doc.Area.NSWidth = 180;

      var expectedRegion = new Bounds( new Vector3( -250, 110, 0 ), new Vector3( 300, 180, 500 ) ) ;//Z値は適当
      foreach ( var element in _curDoc.Children ) {
        if ( element is Region ) {
          continue ;
        }

        var gb = element.GetGlobalBounds() ;
        if ( ! gb.HasValue ) {
          continue ;
        }

        var objName = string.Empty ;
        if ( element is PipingPiece piece ) {
          objName = piece.ObjectName ;
        }
        
        // Bounds.Intersectsでは内側に含まれているかどうかの判断をしている(Intersectsだと交差判定な感じがするけど違った)
        Assert.IsTrue( expectedRegion.Intersects( gb.Value ), $"Protrude!: {element.Name}, {objName}, expected: {expectedRegion}, target: {gb.Value}" ) ;
        Assert.IsTrue( expectedRegion.Contains( gb.Value.center ), $"Not Contain!: {element.Name}, {objName}, expected: {expectedRegion}, target: {gb.Value}" ) ;
      }
    }

    [Test]
    [Order( 9 )]
    public void HistoryCommitTest()
    {
      Assert.IsTrue( _curDoc.History.Commit() );
    }

    [Test]
    [Order( 10 )]
    public void HistoryUndoTest()
    {
      try {
        var doneCount = _curDoc.History.Back( 1 );
        Assert.AreEqual( 1, doneCount );
      }
      catch ( Exception e ) {
        Assert.Fail( e.Message ) ;
      }
    }

    [Test]
    [Order( 11 )]
    public void HistoryRedoTest()
    {
      try {
        var doneCount = _curDoc.History.Go( 1 );
        Assert.AreEqual( 1, doneCount );
      }
      catch ( Exception e ) {
        Assert.Fail( e.Message ) ;
      }
    }
  }
}