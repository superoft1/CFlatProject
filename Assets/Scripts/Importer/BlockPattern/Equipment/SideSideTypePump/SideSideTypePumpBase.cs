using System ;
using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using System.Text.RegularExpressions ;
using Chiyoda ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Chiyoda.Importer ;
using IDF ;
using UnityEngine ;
using Group = Chiyoda.CAD.Topology.Group ;

namespace Importer.BlockPattern.Equipment.SideSideTypePump
{
  public abstract class SideSideTypePumpBase
  {
    public const bool INDEXING_NOW = false;
    protected Document Doc { get ; }
    protected Chiyoda.CAD.Topology.BlockPattern BaseBp { get ; }
    protected CompositeBlockPattern BpOwner { get ; }

    protected SingleBlockPatternIndexInfo Info { get ; set ; }

    private string IdfFolderPath { get ; }

    private string PumpShapeName { get ; }

    protected SideSideTypePumpBase( Document doc, string pumpShapeName, CompositeBlockPattern bpOwner )
    {
      Doc = doc ;
      PumpShapeName = pumpShapeName ;
      BaseBp = BlockPatternFactory.CreateBlockPattern( BlockPatternType.Type.SideSideTypePump, isBlockPatternArrayChild: true ) ;
      BpOwner = bpOwner ;
      IdfFolderPath = Path.Combine( ImportManager.IDFBlockPatternDirectoryPath(), "HorizontalPump/PumpSide-Side_SundyneType" ) ;
    }

    private IEnumerable<string> IdfFiles()
    {
      var fileList = new List<string>() ;
      ImportManager.GetFiles( IdfFolderPath, new List<string> { ".idf", ".id0", ".id1", ".id2", ".id3", ".id4" }, fileList ) ;
      foreach ( var file in fileList.Where( SelectIdf ) ) {
        yield return file ;
      }
    }

    /// <summary>
    /// IDFとポンプの読み込み
    /// </summary>
    /// <returns>ポンプ</returns>
    protected Chiyoda.CAD.Model.Equipment ImportIdfAndPump()
    {
      ImportIdf() ;
      // フランジを付ける場合にバーテックスを考慮する関係で、ポンプよりも先にIDFを読み込む必要がある
      return ImportPump() ;
    }


    /// <summary>
    /// ポンプの読み込み
    /// </summary>
    /// <returns></returns>
    private Chiyoda.CAD.Model.Equipment ImportPump()
    {
      return HorizontalPumpImporter.PumpImport( PumpShapeName, BaseBp ) ;
    }

    protected virtual void PostProcess()
    {
      SideSideTypePumpPipeIndexHelper.BuildFlexList(BpOwner, BaseBp, Info);
      SideSideTypePumpMinimumLengthUpdater.AssortMinimumLengths(BaseBp, this.Info);
      SetBlockPatternInfo( Info ) ;
      Doc.AddEdge( (BlockEdge) BpOwner ?? BaseBp ) ;

      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(1); //  SideSide Pump はグループ組み替えがないためここで大丈夫

      BaseBp.Document.MaintainEdgePlacement() ;
      var bpa = BpOwner ;
      bpa.Name = "SideSidePumpBlocks" + $"({PumpShapeName})";
      if (!INDEXING_NOW){
        bpa.GetProperty( "BlockCount" ).Value = 2 ;
        bpa.GetProperty( "SuctionJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
        bpa.GetProperty( "DischargeJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;

      }
      
    }

    protected virtual bool SelectIdf( string idf )
    {
      if ( ! idf.Contains( PumpShapeName ) ) {
        return false ;
      }
      if ( idf.Contains( "-DIS-A" ) || idf.Contains( "-SUC-A" ) ) {
        return true ;
      }
      return false ;
    }

    private void ImportIdf()
    {
      foreach ( var file in IdfFiles() ) {
        var grpInfo = new GroupInfo( Doc, BaseBp, file, appendDirectlyToGroup: false ) ;
        new IDFDeserializer().ImportData( grpInfo, file ) ;

        var group = grpInfo.Line2Group.Values.ElementAt( 0 ) ;
        RemoveExtraEdges( group, file ) ;

        // IDFにノズル側にフランジ形状が潰れてしまっているので特別に追加する
        var flange = GetNozzleSideFlange( group, file ) ;
        if ( flange != null ) {
          group.AddEdge( flange ) ;
        }
      }
    }

    protected virtual LeafEdge GetNozzleSideFlange( Group group, string file )
    {
      throw new NotImplementedException() ;
    }

    protected virtual void RemoveExtraEdges( Group group, string file )
    {
      throw new NotImplementedException() ;
    }

    private void SetFlexRatio( IGroup group, int edgeIndex, double flexRatio )
    {
      if ( edgeIndex < 0 ) {
        return ;
      }
      var edge = GetEdge( group, edgeIndex ) ;
      var flex = edge.PipingPiece as Pipe ;
      flex.FlexRatio = flexRatio ;
    }

    protected IGroup GetGroup( int groupIndex )
    {
      return BaseBp.NonEquipmentEdges.ElementAtOrDefault( groupIndex ) as IGroup ;
    }

    protected LeafEdge GetEdge( IGroup group, int edgeIndex )
    {
      if ( edgeIndex < 0 ) {
        return null ;
      }
      return group?.EdgeList.ElementAtOrDefault( edgeIndex ) as LeafEdge ;
    }

    protected virtual void SetEdgeName( LeafEdge edge, string objectName )
    {
      var pattern = @"([^0-9]+)([0-9]?)$" ;
      edge.ObjectName = objectName ;
      edge.PipingPiece.ObjectName = Regex.Replace( objectName, pattern, "$1Pipe$2" ) ;
    }

    private LeafEdge GetEquipmentEdge( Chiyoda.CAD.Topology.BlockPattern bp, int equipmentIndex )
    {
      return bp.EquipmentEdges.ElementAtOrDefault( equipmentIndex ) ;
    }

    protected virtual void SetEdgeNames( SingleBlockPatternIndexInfo info )
    {
      var dischargeGroup = GetGroup( info.DischargeIndex ) ;
      var suctionGroup = GetGroup( info.SuctionIndex ) ;

      foreach ( SingleBlockPatternIndexInfo.DischargeIndexType value in Enum.GetValues( typeof( SingleBlockPatternIndexInfo.DischargeIndexType ) ) ) {
        if ( ! info.DischargeIndexTypeValue.TryGetValue( value, out var index ) ) {
          continue ;
        }
        var edge = GetEdge( dischargeGroup, index ) ;
        if ( edge == null ) {
          continue ;
        }
        SetEdgeName( edge, Enum.GetName( typeof( SingleBlockPatternIndexInfo.DischargeIndexType ), value ) ) ;
        if ( value == SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP ) {
          edge.PositionMode = PositionMode.FixedZ ;
        }
        else if ( value == SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd ) {
          edge.PositionMode = PositionMode.FixedY ;
        }
      }

      foreach ( SingleBlockPatternIndexInfo.SuctionIndexType value in Enum.GetValues( typeof( SingleBlockPatternIndexInfo.SuctionIndexType ) ) ) {
        if ( ! info.SuctionIndexTypeValue.TryGetValue( value, out var index ) ) {
          continue ;
        }
        var edge = GetEdge( suctionGroup, index ) ;
        if ( edge == null ) {
          continue ;
        }
        SetEdgeName( edge, Enum.GetName( typeof( SingleBlockPatternIndexInfo.SuctionIndexType ), value ) ) ;
        if ( value == SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd ) {
          edge.PositionMode = PositionMode.FixedY ;
        }
      }

      foreach ( SingleBlockPatternIndexInfo.NextOfIndexType value in Enum.GetValues( typeof( SingleBlockPatternIndexInfo.NextOfIndexType ) ) ) {
        IGroup group = null ;
        switch ( value ) {
          case SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd :
            group = dischargeGroup ;
            break ;
          case SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd :
            group = suctionGroup ;
            break ;
          default :
            throw new ArgumentOutOfRangeException() ;
        }
        if ( ! info.NextOfIndexTypeValue.TryGetValue( value, out var index ) ) {
          continue ;
        }
        var edge = GetEdge( group, index ) ;
        SetEdgeName( edge, Enum.GetName( typeof( SingleBlockPatternIndexInfo.NextOfIndexType ), value ) ) ;
      }
    }


    private void SetBlockPatternInfo( SingleBlockPatternIndexInfo info )
    {
      var groupList = BaseBp.NonEquipmentEdges.ToList() ;
      var dischargeGroup = groupList[ info.DischargeIndex ] as Group ;
      dischargeGroup.Name = "DischargePipes" ;
      var suctionGroup = groupList[ info.SuctionIndex ] as Group ;
      suctionGroup.Name = "SuctionPipes" ;

      if (!INDEXING_NOW)
        SetPropertyAndRule( info ) ;

      foreach ( var flex in info.DischargeFlexIndexList ) {
        SetFlexRatio( dischargeGroup, flex, 1 ) ;
      }

      foreach ( var flex in info.SuctionFlexIndexList ) {
        SetFlexRatio( suctionGroup, flex, 1 ) ;
      }

      var basePump = GetEquipmentEdge( BaseBp, info.BasePumpIndex ) ;
      basePump.ObjectName = "BasePump" ;
      basePump.ConnectionMaintenanceOrigin = basePump ;
      foreach ( var value in info.DischargeIndexTypeValue.Values.Where( v => v >= 0 ) ) {
        var edge = GetEdge( dischargeGroup, value ) ;
        edge.ConnectionMaintenanceOrigin = basePump ;
      }
      foreach ( var value in info.SuctionIndexTypeValue.Values.Where( v => v >= 0 ) ) {
        var edge = GetEdge( suctionGroup, value ) ;
        edge.ConnectionMaintenanceOrigin = basePump ;
      }

      SetEdgeNames( info ) ;

      HorizontalPumpImporter.AlignAllLeafEdges( BaseBp, basePump ) ;

      BaseBp.RuleList.BindChangeEvents( true ) ;
      if ( null != BpOwner ) {
        BpOwner.BaseBlockPattern = BaseBp ;
        BpOwner.RuleList.BindChangeEvents(true);
      }
    }

    protected abstract void SetPropertyAndRule( SingleBlockPatternIndexInfo info ) ;
    /**
    * @brief stub routine to index LeafEdges
    * @note only for windows editor
    */
    static public void IndexLeafEdges() {
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            GameObject []thePumps = FindPumps();
            if (thePumps != null){
                foreach (GameObject aPump in thePumps){
                    for (int k = 0; k < aPump.transform.childCount; ++k){
                        GameObject decendant = aPump.transform.GetChild(k).gameObject;
                        if (decendant.name.Contains("Dis")||decendant.name.Contains("Suc")||decendant.name.Contains("Min")){
                            int index = 0;
                            for (int l = 0; l < decendant.transform.childCount ; ++l){
                                GameObject leafedges = decendant.transform.GetChild(l).gameObject;
                                if (Regex.IsMatch(leafedges.name,"^LeafEdge$",RegexOptions.Singleline)){
                                    leafedges.name = String.Format("LeafEdge.{0:d}",index++); 
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /**
    * @brief find the pump in root
    * @return the pumps found
    */
    static private GameObject []FindPumps(){
        var result = new List<GameObject>();
            GameObject obj = GameObject.Find("Roots");
            if (obj != null) {
                for (int i = 0; i < obj.transform.childCount ; ++i) {
                    GameObject child = obj.transform.GetChild(i).gameObject;
                    if (Regex.IsMatch(child.name,"^SideSidePumpBlocks.*", RegexOptions.Singleline)
                        || Regex.IsMatch(child.name,"^BlockPattern.*", RegexOptions.Singleline)){
                        for (int j = 0; j < child.transform.childCount; ++j) {
                            GameObject decendant = child.transform.GetChild(j).gameObject;
                            if (decendant.name == "SideSideTypePumpBlock") {
                                result.Add(decendant);
                            } 
                        }                        
                    }
                }
            }
        if (result.Count== 0)
            return null;
        else{
            GameObject [] array = new GameObject[result.Count];
            int ix = 0;
            foreach(var item in result){
                array[ix++] = item;
            }
            return array;
        }
    }
  }

  public abstract class SideSideTypePumpBase<T> : SideSideTypePumpBase where T : CompositeBlockPattern
  {
    protected SideSideTypePumpBase( Document doc, string pumpShapeName ) : base( doc, pumpShapeName, doc.CreateEntity<T>() )
    {
    }

    protected new T BpOwner => (T) base.BpOwner ;
  }
}
