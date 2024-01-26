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
using Group = Chiyoda.CAD.Topology.Group ;

namespace Importer.BlockPattern.Equipment.Filter
{
  public abstract class FilterBase
  {
    protected Document Doc { get ; }
    protected Chiyoda.CAD.Topology.BlockPattern BaseBp { get ; }
    protected CompositeBlockPattern BpOwner { get ; }

    protected SingleBlockPatternIndexInfo Info { get ; set ; }

    private string IdfFolderPath { get ; }

    protected string PatternName { get ; }

    protected FilterBase( Document doc, string patternName, CompositeBlockPattern bpOwner )
    {
      Doc = doc ;
      PatternName = patternName ;
      BaseBp = BlockPatternFactory.CreateBlockPattern( BlockPatternType.Type.Filter, isBlockPatternArrayChild: true ) ;
      BpOwner = bpOwner ;
      IdfFolderPath = Path.Combine(Path.Combine( ImportManager.IDFBlockPatternDirectoryPath(), "Filter/IDF" ), PatternName);
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
    protected void ImportIdfAndEquipment()
    {
      ImportIdf() ;

      // フランジを付ける場合にバーテックスを考慮する関係で、ポンプよりも先にIDFを読み込む必要がある
      ImportEquipment() ;
    }


    /// <summary>
    /// ポンプの読み込み
    /// </summary>
    /// <returns></returns>
    private void ImportEquipment()
    {
      FilterBlockPatternImporter.FilterImport( PatternName, BaseBp ) ;
    }

    protected virtual void PostProcess()
    {
      SetBlockPatternInfo( Info ) ;

      Doc.AddEdge( (BlockEdge) BpOwner ?? BaseBp ) ;
      
      var bpa = BpOwner ;
      bpa.Name = bpa.GetType().Name ;
      //bpa.GetProperty( "SuctionJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
      //bpa.GetProperty( "DischargeJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
    }


    protected virtual bool SelectIdf( string idf )
    {
      if ( ! idf.Contains( PatternName ) ) {
        return false ;
      }
      // DRAIN, VENTを除外
      if (Regex.IsMatch(idf, @".*-(DRAIN|VENT)(\(\w\))?_.*")) {
        return false;
      }
      return true ;
    }

    protected virtual void ImportIdf()
    {
      foreach ( var file in IdfFiles() ) {
        var grpInfo = new GroupInfo( Doc, BaseBp, file, appendDirectlyToGroup: false ) ;
        new IDFDeserializer().ImportData( grpInfo, file ) ;

        var group = grpInfo.Line2Group.Values.ElementAt(0);
        RemoveExtraEdges(group, file);
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
      var flex = edge?.PipingPiece as Pipe ;
      if (flex == null){
        UnityEngine.Debug.Log($"{group.Name}:{edgeIndex} is not a pipe");
      }else
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

    protected void SetEdgeName( LeafEdge edge, string objectName )
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
          edge.PositionMode = PositionMode.FixedX ;
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
      }
      //  TODO : スーパークラス側で、サブクラスを見わける仕組みは避けたいが、現時点では最もシンプル
      if (info is SingleBlockPatternIndexInfoWithMirror infoDerived)
      {

        foreach (SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType value in Enum.GetValues(typeof(SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType)))
        {
         if ( ! infoDerived.DischargeAdditionalIndexTypeValue.TryGetValue( value, out var index ) ) {
            continue ;
          }          
          var edge = GetEdge(dischargeGroup, index);
          if (edge == null)
          {
            continue;
          }
          SetEdgeName(edge, Enum.GetName(typeof(SingleBlockPatternIndexInfoWithMirror.DischargeAdditionalIndexType), value));
        }

        foreach (SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType value in Enum.GetValues(typeof(SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType)))
        {
         if ( ! infoDerived.SuctionAdditionalIndexTypeValue.TryGetValue( value, out var index ) ) {
            continue ;
          }          
          var edge = GetEdge(suctionGroup, index);
          if (edge == null)
          {
            continue;
          }
          SetEdgeName(edge, Enum.GetName(typeof(SingleBlockPatternIndexInfoWithMirror.SuctionAdditionalIndexType), value));
        }
      }
      if (info is SingleFilterPatternInfo infoDerived2)
      {

        foreach (SingleFilterPatternInfo.DischargeAdditionalIndexType value in Enum.GetValues(typeof(SingleFilterPatternInfo.DischargeAdditionalIndexType)))
        {
          if ( ! infoDerived2.DischargeAdditionalIndexTypeValue.TryGetValue( value, out var index ) ) {
            continue ;
          }
          var edge = GetEdge(dischargeGroup, index);
          if (edge == null)
          {
            continue;
          }
          SetEdgeName(edge, Enum.GetName(typeof(SingleFilterPatternInfo.DischargeAdditionalIndexType), value));
        }

        foreach (SingleFilterPatternInfo.SuctionAdditionalIndexType value in Enum.GetValues(typeof(SingleFilterPatternInfo.SuctionAdditionalIndexType)))
        {
          if ( ! infoDerived2.SuctionAdditionalIndexTypeValue.TryGetValue( value, out var index ) ) {
            continue ;
          }
          var edge = GetEdge(suctionGroup, index);
          if (edge == null)
          {
            continue;
          }
          SetEdgeName(edge, Enum.GetName(typeof(SingleFilterPatternInfo.SuctionAdditionalIndexType), value));
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
        var edge = GetEdge( group, index) ;
        if ( edge == null ) {
          continue ;
        }
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

      SetPropertyAndRule( info ) ;

      if (info.DischargeFlexIndexList != null){
        foreach ( var flex in info.DischargeFlexIndexList ) {
          SetFlexRatio( dischargeGroup, flex, 1 ) ;
        }
      }

      if (info.SuctionFlexIndexList != null){
        foreach ( var flex in info.SuctionFlexIndexList ) {
          SetFlexRatio( suctionGroup, flex, 1 ) ;
        }
      }

      var basePump = GetEquipmentEdge(BaseBp, info.BasePumpIndex);
      basePump.ObjectName = "BasePump";
      basePump.ConnectionMaintenanceOrigin = basePump;
      foreach (var value in info.DischargeIndexTypeValue.Values.Where(v => v >= 0))
      {
        var edge = GetEdge(dischargeGroup, value);
        if (edge == null){
          UnityEngine.Debug.Log($"Index { value } not found!");
        }else
          edge.ConnectionMaintenanceOrigin = basePump;
      }
      foreach (var value in info.SuctionIndexTypeValue.Values.Where(v => v >= 0))
      {
        var edge = GetEdge(suctionGroup, value);
        if (edge == null){
          UnityEngine.Debug.Log($"Index { value } not found!");
        }else
          edge.ConnectionMaintenanceOrigin = basePump;
      }
      //  TODO : スーパークラス側で、サブクラスを見わける仕組みは避けたいが、現時点では最もシンプル
      if (info is SingleBlockPatternIndexInfoWithMirror infoDerived)
      {
        foreach (var value in infoDerived.DischargeAdditionalIndexTypeValue.Values.Where(v => v >= 0))
        {
          var edge = GetEdge(dischargeGroup, value);
          edge.ConnectionMaintenanceOrigin = basePump;
        }
        foreach (var value in infoDerived.SuctionAdditionalIndexTypeValue.Values.Where(v => v >= 0))
        {
          var edge = GetEdge(suctionGroup, value);
          edge.ConnectionMaintenanceOrigin = basePump;
        }
      }
      if (info is SingleFilterPatternInfo infoDerived2)
      {
        foreach (var value in infoDerived2.DischargeAdditionalIndexTypeValue.Values.Where(v => v >= 0))
        {
          var edge = GetEdge(dischargeGroup, value);
          edge.ConnectionMaintenanceOrigin = basePump;
        }
        foreach (var value in infoDerived2.SuctionAdditionalIndexTypeValue.Values.Where(v => v >= 0))
        {
          var edge = GetEdge(suctionGroup, value);
          edge.ConnectionMaintenanceOrigin = basePump;
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
        if ( edge == null ) {
          continue ;
        }
        edge.ConnectionMaintenanceOrigin = basePump;
      }


      SetEdgeNames( info ) ;
      

      //var edges1 = info.DischargeAngleGroupIndexList.Select(v => GetEdge(dischargeGroup, v));
      //var group1 = Group.CreateContinuousGroup(edges1.WithOlets().ToArray());
      //group1.Name = "DischargeGroup";
      //group1.ObjectName = "DischargeGroup";
      //group1.ConnectionMaintenanceOrigin = basePump;

      HorizontalPumpImporter.AlignAllLeafEdges(BaseBp, basePump);

      BaseBp.RuleList.BindChangeEvents(true);
      if ( null != BpOwner ) {
        BpOwner.BaseBlockPattern = BaseBp ;
        BpOwner.RuleList.BindChangeEvents(true);
      }
    }

    protected abstract void SetPropertyAndRule( SingleBlockPatternIndexInfo info ) ;



  }

  public abstract class FilterBase<T> : FilterBase where T : CompositeBlockPattern
  {
    protected FilterBase( Document doc, string patternName ) : base( doc, patternName, doc.CreateEntity<T>() )
    {
    }
    protected new T BpOwner => (T) base.BpOwner ;
  }
}
