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

namespace Importer.BlockPattern.Equipment.TopTopTypePump
{
  public abstract class TopTopTypePumpBase
  {
    public const bool INDEXING_NOW = false;

    internal Document Doc { get ; }
    internal Chiyoda.CAD.Topology.BlockPattern BaseBp { get ; }
    internal CompositeBlockPattern BpOwner { get ; }

    public SingleBlockPatternIndexInfo Info { get ; protected set ; }

    protected string IdfFolderPath { get ; }

    protected string PumpShapeName { get ; }

    protected TopTopTypePumpBase( Document doc, string pumpShapeName, CompositeBlockPattern bpOwner )
    {
      Doc = doc ;
      PumpShapeName = pumpShapeName ;
      BaseBp = BlockPatternFactory.CreateBlockPattern( BlockPatternType.Type.TopTopTypePump, isBlockPatternArrayChild: true ) ;
      BpOwner = bpOwner ;
      IdfFolderPath = Path.Combine( ImportManager.IDFBlockPatternDirectoryPath(), "HorizontalPump/PumpTop-TopType" ) ;
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
    protected virtual Chiyoda.CAD.Model.Equipment ImportIdfAndPump()
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

    internal virtual void PostProcess()
    {
      SetBlockPatternInfo( Info ) ;
      Doc.AddEdge( (BlockEdge) BpOwner ?? BaseBp ) ;

      BaseBp.Document.MaintainEdgePlacement() ;
      var bpa = BpOwner ;
      if (!INDEXING_NOW){ 
        bpa.Name = "TopTopPumpBlocks" + $"({PumpShapeName})";
        bpa.GetProperty( "BlockCount" ).Value = 2 ;
        bpa.GetProperty( "SuctionJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
        bpa.GetProperty( "DischargeJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
        //bpa.GetProperty( "DischargeDiameter" ).Value = Doc.DiameterLevel.GetLevelFromNPSInch(Info.DischargeDiameterNPSInch) ;
        //bpa.GetProperty( "SuctionDiameter" ).Value = Doc.DiameterLevel.GetLevelFromNPSInch(Info.SuctionDiameterNPSInch) ;

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

    protected virtual void ImportIdf()
    {
      foreach ( var file in IdfFiles() ) {
        var grpInfo = new GroupInfo( Doc, BaseBp, file ) ;
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

    internal void SetFlexRatio( IGroup group, int edgeIndex, double flexRatio )
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
    public static void SetEdgeNameStatic( LeafEdge edge, string objectName ){
      var pattern = @"([^0-9]+)([0-9]?)$" ;
      edge.ObjectName = objectName ;
      edge.PipingPiece.ObjectName = Regex.Replace( objectName, pattern, "$1Pipe$2" ) ;
    }
    protected void SetEdgeName( LeafEdge edge, string objectName )
    { 
      var pattern = @"([^0-9]+)([0-9]?)$" ;
      edge.ObjectName = objectName ;
      edge.PipingPiece.ObjectName = Regex.Replace( objectName, pattern, "$1Pipe$2" ) ;
    }

    protected LeafEdge GetEquipmentEdge( Chiyoda.CAD.Topology.BlockPattern bp, int equipmentIndex )
    {
      return bp.EquipmentEdges.ElementAtOrDefault( equipmentIndex ) ;
    }

    internal virtual void SetEdgeNames( SingleBlockPatternIndexInfo info )
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


    protected virtual void SetBlockPatternInfo( SingleBlockPatternIndexInfo info )
    {
      var groupList = BaseBp.NonEquipmentEdges.ToList() ;
      var dischargeGroup = groupList[ info.DischargeIndex ] as Group ;
      dischargeGroup.Name = "DischargePipes" ;
      var suctionGroup = groupList[ info.SuctionIndex ] as Group ;
      suctionGroup.Name = "SuctionPipes" ;

      if (!INDEXING_NOW)
        SetPropertyAndRule( info ) ;

      if ( null != info.DischargeFlexIndexList )
        foreach ( var flex in info.DischargeFlexIndexList ) {
          SetFlexRatio( dischargeGroup, flex, 1 ) ;
        }

      if ( null != info.SuctionFlexIndexList )
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
      
      if (!INDEXING_NOW){ 
        if (info.DischargeAngleGroupIndexList != null){
          var edges1 = info.DischargeAngleGroupIndexList.Select( v => GetEdge( dischargeGroup, v ) ) ;
          var group1 = Group.CreateContinuousGroup( edges1.WithOlets().ToArray() ) ;
          group1.Name = "DischargeGroup" ;
          group1.ObjectName = "DischargeGroup" ;
          group1.ConnectionMaintenanceOrigin = basePump ;
        }
        OnGroupChanged();
      }
      
      HorizontalPumpImporter.AlignAllLeafEdges( BaseBp, basePump ) ;

      BaseBp.RuleList.BindChangeEvents( true ) ;
      if ( null != BpOwner ) {
        BpOwner.BaseBlockPattern = BaseBp ;
        BpOwner.RuleList.BindChangeEvents(true);
      }
    }

    internal virtual void OnGroupChanged(  )
    {
      //throw new NotImplementedException() ;
    }

    internal abstract void SetPropertyAndRule( SingleBlockPatternIndexInfo info ) ;

  }

  public abstract class TopTopTypePumpBase<T> : TopTopTypePumpBase where T : CompositeBlockPattern
  {
    protected TopTopTypePumpBase( Document doc, string patternName ) : base( doc, patternName, doc.CreateEntity<T>() )
    {
    }

    protected new T BpOwner => (T) base.BpOwner ;
  }
  
}
