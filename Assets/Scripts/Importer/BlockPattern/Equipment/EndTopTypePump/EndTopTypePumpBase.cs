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

namespace Importer.BlockPattern.Equipment.EndTopTypePump
{
  public abstract class EndTopTypePumpBase
  {
    public const bool INDEXING_NOW = false;
    internal Document Doc { get ; }
    public Chiyoda.CAD.Topology.BlockPattern BaseBp { get ; }
    public CompositeBlockPattern BpOwner { get ; }

    public SingleBlockPatternIndexInfo Info { get ; set ; }

    protected string IdfFolderPath { get ; }

    protected string PumpShapeName { get ; }

    protected EndTopTypePumpBase( Document doc, string pumpShapeName, CompositeBlockPattern bpOwner )
    {
      Doc = doc ;
      PumpShapeName = pumpShapeName ;
      BaseBp = BlockPatternFactory.CreateBlockPattern( BlockPatternType.Type.EndTopTypePump, isBlockPatternArrayChild: true ) ;
      BpOwner = bpOwner ;
      IdfFolderPath = Path.Combine( ImportManager.IDFBlockPatternDirectoryPath(), "HorizontalPump/PumpEnd-TopType" ) ;
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
      SetBlockPatternInfo( Info ) ;

      Doc.AddEdge( (BlockEdge) BpOwner ?? BaseBp ) ;

      BaseBp.Document.MaintainEdgePlacement() ;
      var bpa = BpOwner ;
      bpa.Name = "EndTopPumpBlocks" + $"({PumpShapeName})";

      if (INDEXING_NOW == false){ 
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

    protected virtual void ImportIdf()
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

    internal void SetFlexRatio( IGroup group, int edgeIndex, double flexRatio )
    {
      if ( edgeIndex < 0 ) {
        return ;
      }
      var edge = GetEdge( group, edgeIndex ) ;
      var flex = edge.PipingPiece as Pipe ;
      /*
      if (flex == null){
        Debug.Log("the flex is null");
      }
      */
      flex.FlexRatio = flexRatio ;
    }

    internal IGroup GetGroup( int groupIndex )
    {
      return BaseBp.NonEquipmentEdges.ElementAtOrDefault( groupIndex ) as IGroup ;
    }

    internal LeafEdge GetEdge( IGroup group, int edgeIndex )
    {
      if ( edgeIndex < 0 ) {
        return null ;
      }
      return group?.EdgeList.ElementAtOrDefault( edgeIndex ) as LeafEdge ;
    }

    internal static void SetEdgeNameStatic(LeafEdge edge, string objectName ){
      var pattern = @"([^0-9]+)([0-9]?)$" ;
      edge.ObjectName = objectName ;
      edge.PipingPiece.ObjectName = Regex.Replace( objectName, pattern, "$1Pipe$2" ) ;
    }

    internal void SetEdgeName( LeafEdge edge, string objectName )
    {
      EndTopTypePumpBase.SetEdgeNameStatic(edge, objectName);
    }

    internal LeafEdge GetEquipmentEdge( Chiyoda.CAD.Topology.BlockPattern bp, int equipmentIndex )
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

      if (info.DischargeFlexIndexList != null)
        foreach ( var flex in info.DischargeFlexIndexList ) {
          SetFlexRatio( dischargeGroup, flex, 1 ) ;
        }

      if (info.SuctionFlexIndexList != null)
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

      if (INDEXING_NOW == false){ 
        var edges1 = info.DischargeAngleGroupIndexList.Select( v => GetEdge( dischargeGroup, v ) ) ;
        var group1 = Group.CreateContinuousGroup( edges1.WithOlets().ToArray() ) ;
        group1.Name = "DischargeGroup" ;
        group1.ObjectName = "DischargeGroup" ;
        group1.ConnectionMaintenanceOrigin = basePump ;
        NormalizeDischargeGroupAngle( group1 ) ;
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
    protected virtual void NormalizeDischargeGroupAngle( Group dischargeGroup )
    {
    }

    /// <summary>
    /// グループを指定した角度に整列する
    /// </summary>
    /// <param name="dischargeGroup">グループ</param>
    /// <param name="newAngleDegree">新しい角度</param>
    /// <returns>旧い角度</returns>
    protected static double? ModifyGroupAngle( Group dischargeGroup, double newAngleDegree )
    {
      var angle = GuessGroupAngle( LocalCodSys3d.Identity, dischargeGroup ) ;
      if ( null == angle ) return null ;

      var oldAngle = angle.Value ;
      while ( oldAngle < newAngleDegree - 90 ) oldAngle += 180 ;
      while ( oldAngle > newAngleDegree + 90 ) oldAngle -= 180 ;
      
      var angleDiff = (newAngleDegree - oldAngle).Deg2Rad() ;
      double sin = Math.Sin( angleDiff ), cos = Math.Cos( angleDiff ) ;
      var rotation = new LocalCodSys3d( Vector3d.zero, new Vector3d( cos, sin, 0 ), new Vector3d( -sin, cos, 0 ), Vector3d.zero ) ;
      foreach ( var edge in dischargeGroup.EdgeList ) {
        edge.LocalCod = rotation.GlobalizeCodSys( edge.LocalCod ) ;
      }

      return oldAngle ;
    }

    private static double? GuessGroupAngle( LocalCodSys3d codsys, CompositeEdge dischargeGroup )
    {
      codsys = codsys.GlobalizeCodSys( dischargeGroup.LocalCod ) ;
      var subGroups = new List<CompositeEdge>() ;
      foreach ( var edge in dischargeGroup.EdgeList ) {
        if ( edge is LeafEdge le ) {
          var angle = GuessLeafEdgeAngle( codsys, le ) ;
          if ( null != angle ) return angle ;
        }
        else if ( edge is CompositeEdge ce ) {
          subGroups.Add( ce ) ;
        }
      }

      foreach ( var subGroup in subGroups ) {
        var angle = GuessGroupAngle( codsys, subGroup ) ;
        if ( null != angle ) return angle ;
      }

      return null ;
    }

    private static double? GuessLeafEdgeAngle( LocalCodSys3d codsys, LeafEdge edge )
    {
      codsys = codsys.GlobalizeCodSys( edge.LocalCod ) ;
      var pp = edge.PipingPiece ;
      if ( pp is Chiyoda.CAD.Model.Equipment ) return null ;
      int n = Math.Min( 2, pp.ConnectPointCount ) ;
      for ( int i = 0 ; i < n ; ++i ) {
        var vec = codsys.GlobalizeVector( edge.PipingPiece.GetConnectPoint( i ).Vector ) ;
        var xy2 = vec.x * vec.x + vec.y * vec.y ;
        var z2 = vec.z * vec.z ;
        if ( xy2 + z2 < Vector3.kEpsilon * Vector3.kEpsilon ) continue ;
        if ( Math.Abs( xy2 ) <= Math.Abs( z2 ) ) continue ;

        return Math.Atan2( vec.y, vec.x ).Rad2Deg() ;
      }

      return null ;
    }

    internal abstract void SetPropertyAndRule( SingleBlockPatternIndexInfo info ) ;


  }
  
  public abstract class EndTopTypePumpBase<T> : EndTopTypePumpBase where T : CompositeBlockPattern
  {
    protected EndTopTypePumpBase( Document doc, string pumpShapeName ) : base( doc, pumpShapeName, doc.CreateEntity<T>() )
    {
    }

    protected new T BpOwner => (T) base.BpOwner ;
  }
}
