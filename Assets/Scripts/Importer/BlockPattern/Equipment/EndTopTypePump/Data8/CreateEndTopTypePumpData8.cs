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
using Importer.Equipment ;
using UnityEngine ;
using Group = Chiyoda.CAD.Topology.Group ;

namespace Importer.BlockPattern.Equipment.EndTopTypePump.Data8
{
  public class CreateEndTopTypePumpData8
  {
    public static Chiyoda.CAD.Topology.BlockPattern Create( Action<Edge> onFinish )
    {
      return EndTopTypePumpImport( "24-P0704A", ( bp ) =>
      {
        for ( int i = 0 ; i < bp.NonEquipmentEdges.Count() ; ++i ) {
          var group = bp.NonEquipmentEdges.ToList()[ i ] as Group ;
          using ( Group.ContinuityIgnorer( group ) ) {
            var removeCount = i == 0 ? 24 : 18 ;
            var removeEdgeList = group?.EdgeList.Take( removeCount ).ToList() ;
            removeEdgeList?.ForEach(e=>e.Unlink());
          }
        }

        var info = new SingleBlockPatternIndexInfo
        {
          DischargeIndex = 0,
          SuctionIndex = 1,
          BasePumpIndex = 0,
          DischargeAngleGroupIndexList = new List<int> { 13, 14, 15 },
          DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
          {
            { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, 5 },
            { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 0 },
            { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeMinLength1, 6 },
            { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeMinLength2, 10 },
          },
          SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
          {
            { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 10 },
            { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, 0 },
            { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionMinLength1, 6 },
            { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionMinLength2, 8 },
          },
          NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
          {
            { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 1 },
            { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 1 },
          },
          SuctionFlexIndexList = new List<int> { 6, 8 },
          DischargeFlexIndexList = new List<int> { 12 },
          SuctionDiameterNPSInch = 4,
          DischargeDiameterNPSInch = 4,
          SupportTypes = new Dictionary<int, SupportType>
          {
            { 4, SupportType.Trunnion },
            { 5, SupportType.Trunnion },
            { 14, SupportType.Trunnion },
            { 15, SupportType.Trunnion },
          },
        } ;

        
        SetBlockPatternInfo( bp, info ) ;

        bp.SetMinimumLengthRatioByDiameterForAllPipes(1); //  MinimumLength 更新再開・コピー前に設定すること

        //  MinLength1・2 をMinLength自動更新の対象からはずす
        var dischargeGroup = GetGroup(bp, info.DischargeIndex);
        foreach (var edge in dischargeGroup.EdgeList) {
          if (!(edge is LeafEdge le)) continue;
          if (le.ObjectName!= null && le.ObjectName.Contains("MinLength") && le.PipingPiece is Pipe pipe){
            pipe.MinimumLengthRatioByDiameter = 0.0;
            pipe.MinimumLengthWithoutOletRadius = 0.5;
            pipe.Length = 0.5;
            pipe.PreferredLength = 0.5;
          }
        }

        BlockPatternUtil.UpdateFlexRatiosByLength( bp );

        bp.Document.MaintainEdgePlacement() ;
        var bpa = bp.Parent as BlockPatternArray ;
        bpa.Name = "EndTopPumpBlocks" ;
        bpa.GetProperty( "BlockCount" ).Value = 2 ;
        bpa.GetProperty( "BOP" ).Value = 0.5 ;
        //bpa.GetProperty("PipeMinLength").Value = 0.5;
        bpa.GetProperty( "SuctionJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
        bpa.GetProperty( "DischargeJoinType" ).Value = (int) CompositeBlockPattern.JoinType.MiddleTeeInnerDir ;
      }, onFinish ) ;
    }

    static Chiyoda.CAD.Topology.BlockPattern EndTopTypePumpImport(
      string id,
      Action<Chiyoda.CAD.Topology.BlockPattern> onPostProcess = null,
      Action<Edge> onFinish = null )
    {
      var folderPath = Path.Combine( ImportManager.XMLDirectoryPath(), "End-Top-Pump/" + id ) ;
      var fileList = new List<string>() ;
      ImportManager.GetFiles( folderPath, ".xml", fileList ) ;

      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew() ;
      var bp = BlockPatternFactory.CreateBlockPattern( BlockPatternType.Type.EndTopTypePump, isBlockPatternArrayChild: true ) ;
      
      var dataSet = PipingPieceDataSet.GetPipingPieceDataSet() ;
      var equipmentTable = PipingPieceTableFactory.Create( bp.Type, dataSet ) ;
      var (equipment, origin, rot) = equipmentTable.Generate( bp.Document, id, createNozzle: true ) ;
      BlockPatternFactory.CreateInstrumentEdgeVertex( bp, equipment as Chiyoda.CAD.Model.Equipment, origin, rot ) ;
      
      ImportManager.Instance().BlockPattermXMLImport( bp, curDoc, fileList.ToArray() ) ;

      bp.SetMinimumLengthRatioByDiameterForAllPipes(0); //  MinimumLength 更新停止

      bp.LocalCod = new LocalCodSys3d( Vector3d.zero, bp.LocalCod ) ;
      foreach ( var edge in bp.NonEquipmentEdges ) {
        edge.LocalCod = LocalCodSys3d.Identity ;
      }

      var bpa = curDoc.CreateEntity<BlockPatternArray>() ;
      bpa.BaseBlockPattern = bp ;
      curDoc.AddEdge( bpa ) ;

      onPostProcess?.Invoke( bp ) ;

      // vertexにflowを設定
      // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
      ( (BlockPatternArray) bp.Parent ).SetVertexName( "DischargeEnd", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
      ( (BlockPatternArray) bp.Parent ).SetVertexName( "SuctionEnd", "N-1" , HalfVertex.FlowType.FromAnotherToThis) ;
      
      onFinish?.Invoke( bpa ) ;

      return bp ;
    }

    static void SetBlockPatternInfo( Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info )
    {
      var groupList = bp.NonEquipmentEdges.ToList() ;
      var dischargeGroup = groupList[ info.DischargeIndex ] as Group ;
      dischargeGroup.Name = "DischargePipes" ;
      var suctionGroup = groupList[ info.SuctionIndex ] as Group ;
      suctionGroup.Name = "SuctionPipes" ;

      // サポート
      new EndTopTypePumpBPSupportBuilder( bp, info ).Build() ;

      SetPropertyAndRule( bp, info ) ;

      foreach ( var flex in info.DischargeFlexIndexList ) {
        SetFlexRatio( dischargeGroup, flex, 1 ) ;
      }

      foreach ( var flex in info.SuctionFlexIndexList ) {
        SetFlexRatio( suctionGroup, flex, 1 ) ;
      }

      var basePump = GetEquipmentEdge( bp, info.BasePumpIndex ) ;
      foreach ( var value in info.DischargeIndexTypeValue.Values ) {
        var edge = GetEdge( dischargeGroup, value ) ;
        edge.ConnectionMaintenanceOrigin = basePump ;
      }
      foreach ( var value in info.SuctionIndexTypeValue.Values ) {
        var edge = GetEdge( suctionGroup, value ) ;
        edge.ConnectionMaintenanceOrigin = basePump ;
      }
      foreach (SingleBlockPatternIndexInfo.NextOfIndexType key in Enum.GetValues(typeof(SingleBlockPatternIndexInfo.NextOfIndexType))) {
        int value;
        LeafEdge edge;
        info.NextOfIndexTypeValue.TryGetValue(key, out value);
        switch(key){
          case SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd:
            edge = GetEdge(dischargeGroup, value);
            edge.ConnectionMaintenanceOrigin = basePump ;
            break;
          case SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd:
            edge = GetEdge(suctionGroup, value);
            edge.ConnectionMaintenanceOrigin = basePump ;
            break;
        }
      }

      var edges1 = info.DischargeAngleGroupIndexList.Select( v => GetEdge( dischargeGroup, v ) ) ;
      var group1 = Group.CreateContinuousGroup( edges1.WithOlets().ToArray() ) ;
      basePump.ObjectName = "BasePump" ;
      basePump.ConnectionMaintenanceOrigin = basePump ;
      group1.Name = "DischargeGroup" ;
      group1.ObjectName = "DischargeGroup" ;
      group1.ConnectionMaintenanceOrigin = basePump ;

      SetEdgeNames( bp, info ) ;

      var offset = -(Vector3) basePump.LocalCod.Origin ;
      foreach ( var leafEdge in bp.GetAllLeafEdges() ) {
        leafEdge.MoveLocalPos( offset ) ;
      }

      bp.RuleList.BindChangeEvents( true ) ;
      ( (BlockPatternArray) bp.Parent ).RuleList.BindChangeEvents( true ) ;
    }


    static void SetPropertyAndRule( Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info )
    {
      bp.RegisterUserDefinedProperty( "AccessSpace", PropertyType.Length, 1.2 ) ;

      bp.RegisterUserDefinedProperty( "PipeMinLength", PropertyType.Length, 0.5 ) ;
      bp.RuleList.AddRule( "#SuctionMinLengthPipe1.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#SuctionMinLengthPipe2.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#DischargeMinLengthPipe1.Length", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#DischargeMinLengthPipe2.Length", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#DischargeMinLengthPipe1.MinLength", "0.0" ) ;
      bp.RuleList.AddRule( "#DischargeMinLengthPipe2.MinLength", "0.0" ) ;

      {
        var bpa = bp.Parent as BlockPatternArray ;
        bpa.RegisterUserDefinedProperty( "BlockCount", PropertyType.GeneralInteger, 1 ) ;
        bpa.RuleList.AddRule( ".ArrayCount", ".BlockCount" ) ;
      }
      bp.RuleList.AddRule( ":Parent.ArrayOffsetX", "Max( #BasePump.MaxX - #DischargeBOP.MinX, #DischargeBOP.MaxX - #BasePump.MinX ) + .AccessSpace" ) ;
      
      bp.RegisterUserDefinedProperty( "SuctionJoinType", CompositeBlockPattern.JoinType.Independent )
        .AddUserDefinedRule( CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule( "SuctionEnd" ) ) ;
      bp.RegisterUserDefinedProperty( "DischargeJoinType", CompositeBlockPattern.JoinType.Independent )
        .AddUserDefinedRule( CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule( "DischargeEnd" ) ) ;
      
      bp.RegisterUserDefinedProperty( "HeaderBOP", PropertyType.Length, 2.3 ) ;

      string LengthFunc( string diameter )
      {
        return $"( Max( DiameterToElbow90Length( {diameter} ), DiameterToTeeMainLength( {diameter},{diameter} ) ) - {diameter} * 0.5 )" ;
      }
      
      bp.RuleList.AddRule( "#DischargeEndPipe.Length",
          $"#BasePump.MinZ + .HeaderBOP - {LengthFunc( "#DischargeEndPipe.Diameter" )} - #NextOfDischargeEnd.MaxZ" )
        .AddTriggerSourcePropertyName( "DischargeDiameter" ) ;
      bp.RuleList.AddRule( "#SuctionEndPipe.Length",
          $"#BasePump.MinZ + .HeaderBOP - {LengthFunc( "#SuctionEndPipe.Diameter" )} - #NextOfSuctionEnd.MaxZ" )
        .AddTriggerSourcePropertyName( "SuctionDiameter" ) ;

      bp.RegisterUserDefinedProperty( "NozzlePipeLength", PropertyType.Length, 4, "0", "10" ) ;
      bp.RuleList.AddRule( "#SuctionNozzlePipe.Length", "#SuctionNozzlePipe.Diameter * .NozzlePipeLength" ) ;

      bp.RegisterUserDefinedProperty( "BOP", PropertyType.Length, 0.0 ) ;
      bp.RuleList.AddRule( "#DischargeBOP.MinZ", "#BasePump.MinZ + .BOP" ) ;

      
      bp.RegisterUserDefinedProperty( "HeaderInterval", PropertyType.Length, 0.5 ) ;
      IRule rule = bp.RuleList.AddRule("#SuctionEnd.MinY", "#DischargeBOP.MaxY + .HeaderInterval");
      rule.AddTriggerSourcePropertyName( "SuctionDiameter" ) ;
      rule.AddTriggerSourcePropertyName("PipeMinLength");
      
      bp.RegisterUserDefinedProperty( "DischargeDirection", 0, new Dictionary<string, double> { { "Right", 0 }, { "Left", 1 } } ) ;
      bp.RegisterUserDefinedProperty( "DischargeAngle", PropertyType.Angle, 0, -60, 45, stepValue: 15 ) ;
      bp.RuleList.AddRule( "#DischargeGroup.HorizontalRotationDegree", "If( .DischargeDirection, 180 - .DischargeAngle, .DischargeAngle )" ) ;

      var range = DiameterRange.GetBlockPatternNpsMmRange();

      var suctionProp = bp.RegisterUserDefinedProperty( "SuctionDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(info.SuctionDiameterNPSInch).NpsMm, range.min, range.max) ;
      suctionProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ("SuctionEndPipe", 0) ) ) ;

      var dischargeProp = bp.RegisterUserDefinedProperty( "DischargeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch(info.DischargeDiameterNPSInch).NpsMm, range.min, range.max);
      dischargeProp.AddUserDefinedRule( new AllComponentDiameterRangeRule( ("DischargeEndPipe", 0) ) ) ;
    }

    static void SetEdgeNames( Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info )
    {
      var dischargeGroup = GetGroup( bp, info.DischargeIndex ) ;
      var suctionGroup = GetGroup( bp, info.SuctionIndex ) ;

      foreach ( SingleBlockPatternIndexInfo.DischargeIndexType value in Enum.GetValues( typeof( SingleBlockPatternIndexInfo.DischargeIndexType ) ) ) {
        var edge = GetEdge( dischargeGroup, info.DischargeIndexTypeValue[ value ] ) ;
        SetEdgeName( edge, Enum.GetName( typeof( SingleBlockPatternIndexInfo.DischargeIndexType ), value ) ) ;
        if ( value == SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP ) {
          edge.PositionMode = PositionMode.FixedZ ;
        }
      }

      foreach ( SingleBlockPatternIndexInfo.SuctionIndexType value in Enum.GetValues( typeof( SingleBlockPatternIndexInfo.SuctionIndexType ) ) ) {
        var edge = GetEdge( suctionGroup, info.SuctionIndexTypeValue[ value ] ) ;
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
        var edge = GetEdge( group, info.NextOfIndexTypeValue[ value ] ) ;
        SetEdgeName( edge, Enum.GetName( typeof( SingleBlockPatternIndexInfo.NextOfIndexType ), value ) ) ;
      }
    }


    static void SetFlexRatio( IGroup group, int edgeIndex, double flexRatio )
    {
      var edge = GetEdge( group, edgeIndex ) ;
      var flex = edge.PipingPiece as Pipe ;
      flex.FlexRatio = flexRatio ;
    }

    static IGroup GetGroup( Chiyoda.CAD.Topology.BlockPattern bp, int groupIndex )
    {
      return bp.NonEquipmentEdges.ElementAtOrDefault( groupIndex ) as IGroup ;
    }

    static LeafEdge GetEdge( IGroup group, int edgeIndex )
    {
      return group?.EdgeList.ElementAtOrDefault( edgeIndex ) as LeafEdge ;
    }

    static void SetEdgeName( LeafEdge edge, string objectName )
    {
      var pattern = @"([^0-9]+)([0-9]?)$" ;
      edge.ObjectName = objectName ;
      edge.PipingPiece.ObjectName = Regex.Replace( objectName, pattern, "$1Pipe$2" ) ;
    }

    static LeafEdge GetEquipmentEdge( Chiyoda.CAD.Topology.BlockPattern bp, int equipmentIndex )
    {
      return bp.EquipmentEdges.ElementAtOrDefault( equipmentIndex ) ;
    }
  }
}
