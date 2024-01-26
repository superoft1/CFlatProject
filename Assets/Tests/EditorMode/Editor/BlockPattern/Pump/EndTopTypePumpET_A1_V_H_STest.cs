using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.EndTopTypePump ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_A1_V_H_S ;
using NUnit.Framework ;
using UnityEngine ;

namespace Tests.EditorMode.Editor.BlockPattern.Pump
{
  public class EndTopTypePumpET_A1_V_H_STest
  {
    private Document _curDoc = null ;
    
    // 各テスト実行前にそれぞれ実行される
    [SetUp]
    public void SetUp()
    {
      Debug.Log("SetUp");
      _curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew() ;
    }
    
    // 各テスト実行後にそれぞれ実行される
    [TearDown]
    public void TearDown()
    {
      Debug.Log("TearDown");
      DocumentCollection.Instance.Close(_curDoc  ) ;
    }
    
    // A Test behaves as an ordinary method
    [Test]
    public void BlockCountTest()
    {
      var blockPattern = new EndTopTypePumpET_A1_V_H_S( _curDoc ).Create( null ) ;
      
      Assert.AreEqual( blockPattern.Parent is BlockPatternArray, true ) ;
      
      var blockArray = (BlockPatternArray) blockPattern.Parent ;
      Assert.AreEqual( blockArray.ArrayCount, 1 ) ;// 作成直後はArrayCount = 1

      _curDoc.MaintainEdgePlacement() ;
      
      Assert.AreEqual( blockArray.ArrayCount, 2 ) ;

      blockArray.GetProperty( "BlockCount" ).Value = 3 ;
      Assert.AreEqual( blockArray.ArrayCount, 2 ) ;// 変更が反映されない
      
      _curDoc.MaintainEdgePlacement() ;
      
      Assert.AreEqual( blockArray.ArrayCount, 3 ) ;
    }

    [Test]
    public void SuctionNozzlePipeLengthTest()
    {
      var blockPattern = new EndTopTypePumpET_A1_V_H_S( _curDoc ).Create( null ) ;
      var blockArray = (BlockPatternArray) blockPattern.Parent ;
      _curDoc.MaintainEdgePlacement() ;
      
      Assert.AreEqual( blockArray.ArrayCount, 2 ) ;
      for ( var i = 0 ; i < 2 ; ++i ) {
        Assert.AreEqual( blockArray.GetBlockPattern( i ).GetProperty( "NozzlePipeLength" ).Value, 4 ) ;
      }

      for ( var i = 0 ; i < 2 ; ++i ) {
        blockArray.GetBlockPattern( i ).GetProperty( "NozzlePipeLength" ).Value = 10 ;
      }
      _curDoc.MaintainEdgePlacement() ;
      for ( var i = 0 ; i < 2 ; ++i ) {
        Assert.AreEqual( blockArray.GetBlockPattern( i ).GetProperty( "NozzlePipeLength" ).Value, 10 ) ;
      }
    }

    [Test]
    public void DischargeDiameterTest()
    {
      var blockPattern = new EndTopTypePumpET_A1_V_H_S( _curDoc ).Create( null ) ;
      _curDoc.MaintainEdgePlacement() ;
      
      var dischargeEnd = blockPattern.GetElementByName( "DischargeEnd" ) as LeafEdge ;
      var dischargeNozzle = blockPattern.GetElementByName( "DischargeNozzle" ) as LeafEdge ;
      Assert.NotNull(dischargeEnd);
      Assert.NotNull(dischargeNozzle);
      Assert.AreEqual( ( (Pipe) dischargeEnd.PipingPiece ).DiameterObj.NpsInch, 3 ) ;
      Assert.AreEqual( ( (Pipe) dischargeNozzle.PipingPiece ).DiameterObj.NpsInch, 1.5 ) ;

      blockPattern.GetProperty( "DischargeDiameter" ).Value = DiameterFactory.FromNpsInch( 10 ).NpsMm ;
      _curDoc.MaintainEdgePlacement() ;
      Assert.AreEqual( ( (Pipe) dischargeEnd.PipingPiece ).DiameterObj.NpsInch, 10 ) ;
      Assert.AreEqual( ( (Pipe) dischargeNozzle.PipingPiece ).DiameterObj.NpsInch, 5 ) ;      
    }

    /// <summary>
    /// はじめの２つのBlockPattern で最小溶接間距離の相違がないかテストする
    /// </summary>
    /// <remarks>すみません一つ追加します・20inch でチェック</remarks>
    [Test]
    public void CheckMinLengthDifference()
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var importer = new EndTopTypePumpET_A1_V_H_S(curDoc);
      var blockPattern = importer.Create(null);
      int groupIndex = importer.Info.DischargeIndex;
      UnityEngine.Debug.Log($"{blockPattern.Type.ToString()}");

      curDoc.MaintainEdgePlacement();

      var blockArray = (BlockPatternArray) blockPattern.Parent ;
      blockArray.GetProperty( "HeaderInterval" ).Value = 0.0 ;
      //blockArray.GetProperty( "DischargeDiameter" ).Value = DiameterFactory.FromNpsInch( 20 ).NpsMm ;
      //blockArray.GetProperty( "SuctionDiameter" ).Value = DiameterFactory.FromNpsInch( 20 ).NpsMm ;
      for ( var i = 0 ; i < 2 ; ++i ) {
        blockArray.GetBlockPattern( i ).GetProperty( "DischargeDiameter" ).Value = DiameterFactory.FromNpsInch( 20 ).NpsMm ;
      }
      for ( var i = 0 ; i < 2 ; ++i ) {
        blockArray.GetBlockPattern( i ).GetProperty( "SuctionDiameter" ).Value = DiameterFactory.FromNpsInch( 20 ).NpsMm ;
      }

      
      _curDoc.MaintainEdgePlacement() ;

      Assert.AreEqual(blockArray.ArrayCount, 2);

      if (blockPattern.Parent != null)
        UnityEngine.Debug.Log($"{blockPattern.Parent.GetType().ToString()}");
      Assert.AreEqual(blockPattern is Chiyoda.CAD.Topology.BlockPattern, true);

      int index = 0;
      var dischargeGroups = new System.Collections.Generic.List<IGroup> { };
      var suctionGroups = new System.Collections.Generic.List<IGroup> { };
      foreach (var bpe in blockArray.EdgeList) {
        if (! ( bpe is Chiyoda.CAD.Topology.BlockPattern bp )) continue;
        UnityEngine.Debug.Log($"BlockPattern{index++}");
        var array = bp.NonEquipmentEdges.ToArray();
        if ( array[ importer.Info.DischargeIndex ] is IGroup group ) {
          dischargeGroups.Add( group ) ;
        }
        if ( array[ importer.Info.SuctionIndex ] is IGroup group2 ) {
          suctionGroups.Add( group2 ) ;
        }
      }
      Assert.AreEqual(dischargeGroups.Count, 2);
      Assert.AreEqual(suctionGroups.Count, 2);

      var dischargeLeafEdges = new LeafEdge[2][];
      var suctionLeafEdges = new LeafEdge[2][];

      dischargeLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(0));
      dischargeLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(dischargeGroups.ElementAt(1));
      suctionLeafEdges[0] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(0));
      suctionLeafEdges[1] = EndTopTypePumpLengthUpdater.ExtractLeafEdgeArrayFromGroupHierarachy(suctionGroups.ElementAt(1));

      UnityEngine.Debug.Log("Discharge pipes");
      for (int i = 0; i < dischargeLeafEdges[0].Length; ++i) {
        var le1 = dischargeLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = dischargeLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.MinimumLengthWithoutOletRadius != pipe2.MinimumLengthWithoutOletRadius){
            UnityEngine.Debug.Log($"pipe{i} has different min-length-without-olet-radius one has {pipe1.MinimumLengthWithoutOletRadius} another has {pipe2.MinimumLengthWithoutOletRadius}");
          }else
            UnityEngine.Debug.Log($"pipe{i} has length {pipe1.MinimumLengthWithoutOletRadius}");
        } else {
          UnityEngine.Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      UnityEngine.Debug.Log("Discharge pipe ok!");
      UnityEngine.Debug.Log("Suction pipes");
      for (int i = 0; i < suctionLeafEdges[0].Length; ++i) {
        var le1 = suctionLeafEdges[0][i];
        if (!(le1.PipingPiece is Pipe pipe1))
          continue;
        var le2 = suctionLeafEdges[1][i];
        if (le2.PipingPiece is Pipe pipe2) {
          if (pipe1.MinimumLengthWithoutOletRadius != pipe2.MinimumLengthWithoutOletRadius){
            UnityEngine.Debug.Log($"pipe{i} has different min-length-without-olet-radius one has {pipe1.MinimumLengthWithoutOletRadius} another has {pipe2.MinimumLengthWithoutOletRadius}");
          }else
            UnityEngine.Debug.Log($"pipe{i} has length {pipe1.MinimumLengthWithoutOletRadius}");
        } else {
          UnityEngine.Debug.Log("Different type");
          Assert.NotNull(null); //  Type が違う
        }
      }
      UnityEngine.Debug.Log("Suction pipe ok!");
    }

  }
}
