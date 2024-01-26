///
///<summary>
/// FS_A1_1_S を all Fixed（ただし、X方向だけ）で動かすバージョン
///</summary>
///
using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Chiyoda.DB ;
using IDF ;
using UnityEngine ;
using Group = Chiyoda.CAD.Topology.Group ;

namespace Importer.BlockPattern.Equipment.Filter
{
  public class FilterFS_A1_1_S_Temp : FilterBase<BlockPatternArray>
  {

    public FilterFS_A1_1_S_Temp( Document doc ) : base( doc, "FS-A1-1-S" )
    {
      Info = new SingleFilterPatternInfo
      {
        DischargeIndex = 0,
        SuctionIndex = 1,
        BasePumpIndex = 0,
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int> { },
        DischargeAdditionalIndexTypeValue = new Dictionary<SingleFilterPatternInfo.DischargeAdditionalIndexType, int>
        {
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPostNozzlePipe, 0 },
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisTee, 8 }, //  discharge 側 Tee(FixedX)
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveA1Valve, 5 }, //  discharge 側 MainValve (FixedX)
          { SingleFilterPatternInfo.DischargeAdditionalIndexType.DisPreTeePipe, 7 },
        },
        SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
        {
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 18 },
        },
        SuctionAdditionalIndexTypeValue = new Dictionary<SingleFilterPatternInfo.SuctionAdditionalIndexType, int>
        {
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPostNozzlePipe, 18 },
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA1Valve, 13 }, //  suction 側 MainValve (FixedX)
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucTee, 0 }, //  suction 側 Tee (FixedX)
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchPipeA, 1 }, //  suction 側 pipe 
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchElbowA0Elbow, 2 }, //  suction 側 elb
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassPipeA, 3 }, //  suction 側 の次パイプ (FixedY)
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassPipeB, 7 }, //  suction 側 の次パイプ
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassPipeC, 8 }, //  suction 側 の次パイプ
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchPipeB, 10 }, //  discharge 側 pipe
          { SingleFilterPatternInfo.SuctionAdditionalIndexType.SucPreTeePipe, 11 }, //  

        },
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
          /*
                    { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, -1 },
                    { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, -1 },
                  */
        },

        //
        //  Pipe index helper
        //                               sm - branch     dm - branch   pump-suctee    pump-distee
        SuctionFlexHelper = new int[ , ] { { 1, 1 }, { 10, 10 }, { 11, 16 }, { 3, 8 } },
        DischargeFlexHelper = new int[ , ] { { 2, 7 } }, //  all discharge pipes
        SuctionPipeIndexRange = new int[ , ] { { 10, 16 }, }, //  all pipe (  Suction nozzle post は含まない）
        DischargePipeIndexRange = new int[ , ] { { 2, 7 }, }, //  all pipe

        SuctionDiameterNPSInch = 6,
        DischargeDiameterNPSInch = -1,
      } ;
    }

    public Chiyoda.CAD.Topology.BlockPattern Create( Action<Edge> onFinish )
    {
      ImportIdfAndEquipment() ;
      foreach ( var edge in BaseBp.NonEquipmentEdges ) {
        edge.LocalCod = LocalCodSys3d.Identity ;
      }

      FilterIndexingHelper.BuildIndexList( BpOwner, BaseBp, Info ) ;
      //FilterPipeLengthUpdater.AssortMinimumLengths( BaseBp, Info ) ;

      PostProcess() ;
      
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(1);

      //  初期値調整
      BpOwner.GetProperty("PipeDiameter").Value = DiameterFactory.FromNpsInch(Info.SuctionDiameterNPSInch).NpsMm;
      BpOwner.GetProperty( "DischargeFilterToValve" ).Value = 0.5;
      BpOwner.GetProperty( "SuctionFilterToValve" ).Value = 0.5;

      onFinish?.Invoke( (BlockEdge) BpOwner ?? BaseBp ) ;

      return BaseBp ;
    }
    protected override void ImportIdf(){
      base.ImportIdf();
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(0);
    }
    /// <summary>
    /// IDFにノズル側にフランジ形状が潰れてしまっているので特別に追加する
    /// </summary>
    /// <param name="group"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    protected override LeafEdge GetNozzleSideFlange( Group group, string file )
    {
      return null ;
    }

    protected override void SetEdgeNames( SingleBlockPatternIndexInfo info )
    {
      base.SetEdgeNames( info ) ;
      /*
      if (info is SingleFilterPatternInfo infoDerived){
        var dischargeGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault( info.DischargeIndex ) as IGroup ;
        var suctionGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault( info.SuctionIndex ) as IGroup ;
        var le = suctionGroup?.EdgeList.ElementAtOrDefault( infoDerived.SuctionAdditionalIndexTypeValue[ SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchPipeA]  ) as LeafEdge ;
        le.PositionMode = PositionMode.FixedX ;
        le = suctionGroup?.EdgeList.ElementAtOrDefault( infoDerived.SuctionAdditionalIndexTypeValue[ SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBranchPipeB]  ) as LeafEdge ;
        le.PositionMode = PositionMode.FixedX ;
      }
      */
    }

    protected override void RemoveExtraEdges( Group group, string file )
    {
    }

    protected override void SetBlockPatternInfo( SingleBlockPatternIndexInfo info )
    {
      base.SetBlockPatternInfo( info ) ;
      if ( info is SingleFilterPatternInfo infoDerived ) {
        var dischargeGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault( info.DischargeIndex ) as IGroup ;
        var suctionGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault( info.SuctionIndex ) as IGroup ;
        var basePump = BaseBp.EquipmentEdges.ElementAtOrDefault( info.BasePumpIndex ) ;
        foreach ( SingleFilterPatternInfo.DischargeAdditionalIndexType value in infoDerived.DischargeAdditionalIndexTypeValue.Values.Where( v => v >= 0 ) ) {
          if ( ! infoDerived.DischargeAdditionalIndexTypeValue.TryGetValue( value, out var index ) ) {
            continue ;
          }

          var edge = GetEdge( dischargeGroup, index ) ;
          if ( edge == null )
            continue ;
          edge.ConnectionMaintenanceOrigin = basePump ;
          UnityEngine.Debug.Log( $"{dischargeGroup.Name}.{index} maintained!" ) ;
        }

        foreach ( SingleFilterPatternInfo.SuctionAdditionalIndexType value in infoDerived.SuctionAdditionalIndexTypeValue.Values.Where( v => v >= 0 ) ) {
          if ( ! infoDerived.SuctionAdditionalIndexTypeValue.TryGetValue( value, out var index ) ) {
            continue ;
          }

          var edge = GetEdge( suctionGroup, index ) ;
          if ( edge == null )
            continue ;
          edge.ConnectionMaintenanceOrigin = basePump ;
          UnityEngine.Debug.Log( $"{suctionGroup.Name}.{index} maintained!" ) ;
        }
      }
    }

    protected override void SetPropertyAndRule( SingleBlockPatternIndexInfo info )
    {
      // SuctionDiameter == DischargeDiameter
      var range = DiameterRange.GetBlockPatternNpsMmRange();
      double dlevel = (double)info.SuctionDiameterNPSInch;
      var diameterProp = BaseBp.RegisterUserDefinedProperty("PipeDiameter", PropertyType.DiameterRange, DiameterFactory.FromNpsInch((dlevel >= 8) ? 6.0 : 8.0).NpsMm,
        range.min, range.max);
      diameterProp.AddUserDefinedRule(new AllComponentDiameterRangeRule(("DisTeePipe", 0), ("DisTeePipe", 2), ("SucBranchPipeBPipe", 1), ("SucTeePipe", 1)));

      const string mainToBypassMinRule = "MinHorzDistanceOf(#SucBranchElbowA0Elbow, #SucTee) - #DisPreTeePipePipe.Diameter" ;
      
      const string dischargeFilterToBranchMinRule = "MinHorzDistanceOf(#DisPostNozzlePipe,#DisTee) + #DisPostNozzlePipePipe.Length*0.5 - #DisPreTeePipePipe.Diameter*0.5" ;
      const string dischargeFilterToValveMinRule = "MinHorzDistanceOf(#DisPostNozzlePipe,#DisValveA1Valve) + #DisPostNozzlePipePipe.Length*0.5" ;
      const string dischargeFilterToValveMaxRule = ".DischargeFilterToBranch + #DisPreTeePipePipe.Diameter*0.5 - MinHorzDistanceOf(#DisValveA1Valve,#DisTee)" ;

      const string suctionFilterToBranchMinRule = "MinHorzDistanceOf(#SucPostNozzlePipe,#SucTee) + #SucPostNozzlePipePipe.Length*0.5 - #SucPreTeePipePipe.Diameter*0.5" ;
      const string suctionFilterToValveMinRule = "MinHorzDistanceOf(#SucPostNozzlePipe,#SucMainValveA1Valve) + #SucPostNozzlePipePipe.Length*0.5" ;
      const string suctionFilterToValveMaxRule = ".SuctionFilterToBranch + #SucPreTeePipePipe.Diameter*0.5 - MinHorzDistanceOf(#SucMainValveA1Valve,#SucTee)" ;

      //
      // UI Properties
      //
      BaseBp.RegisterUserDefinedProperty( "MainToBypass", PropertyType.Length, 0.962, mainToBypassMinRule, null ) ;
      BaseBp.RegisterUserDefinedProperty( "DischargeFilterToBranch", PropertyType.Length, 1.075, dischargeFilterToBranchMinRule, null ) ;
      BaseBp.RegisterUserDefinedProperty( "DischargeFilterToValve", PropertyType.Length, 0.387, dischargeFilterToValveMinRule, dischargeFilterToValveMaxRule ) ;
      BaseBp.RegisterUserDefinedProperty( "SuctionFilterToBranch", PropertyType.Length, 1.075, suctionFilterToBranchMinRule, null ) ;
      BaseBp.RegisterUserDefinedProperty( "SuctionFilterToValve", PropertyType.Length, 0.488, suctionFilterToValveMinRule, suctionFilterToValveMaxRule ) ;

      // Bypass Pipe の距離を設定する（心間ではなく外間距離なので注意）
      BaseBp.RuleList.AddRule( "#SucBypassPipeA.MinY", "#DisPreTeePipe.MaxY + .MainToBypass" ).AddTriggerSourcePropertyName( "PipeDiameter" ) ;

      // 左右のBranchTee の(中心)座標を設定する
      BaseBp.RuleList.AddRule( "#DisTee.PosX", "#BasePump.MinX - .DischargeFilterToBranch - #DisPostNozzlePipePipe.Diameter*0.5" ).AddTriggerSourcePropertyName( "PipeDiameter" ) ;
      BaseBp.RuleList.AddRule( "#SucTee.PosX", "#BasePump.MaxX + .SuctionFilterToBranch + #SucPostNozzlePipePipe.Diameter*0.5" ).AddTriggerSourcePropertyName( "PipeDiameter" ) ;

      // 左右のバルブの中心座標を設定する
      BaseBp.RuleList.AddRule( "#DisValveA1Valve.PosX", "#BasePump.MinX - .DischargeFilterToValve" ).AddTriggerSourcePropertyName( "PipeDiameter" ) ;
      BaseBp.RuleList.AddRule( "#SucMainValveA1Valve.PosX", "#BasePump.MaxX + .SuctionFilterToValve" ).AddTriggerSourcePropertyName( "PipeDiameter" ) ;

      if ( info is SingleFilterPatternInfo infoDerived ) {
        var suctionGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault( info.SuctionIndex ) as IGroup ;
        var dischargeGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault( info.DischargeIndex ) as IGroup ;

        if ( suctionGroup?.EdgeList.ElementAtOrDefault( infoDerived.SuctionAdditionalIndexTypeValue[ SingleFilterPatternInfo.SuctionAdditionalIndexType.SucTee ] ) is LeafEdge sucTee ) {
          sucTee.PositionMode = PositionMode.FixedX ;
        }

        if ( suctionGroup?.EdgeList.ElementAtOrDefault( infoDerived.SuctionAdditionalIndexTypeValue[ SingleFilterPatternInfo.SuctionAdditionalIndexType.SucMainValveA1Valve ] ) is LeafEdge sucMainValveA1 ) {
          sucMainValveA1.PositionMode = PositionMode.FixedX ;
        }

        if ( dischargeGroup?.EdgeList.ElementAtOrDefault( infoDerived.DischargeAdditionalIndexTypeValue[ SingleFilterPatternInfo.DischargeAdditionalIndexType.DisTee ] ) is LeafEdge disTee ) {
          disTee.PositionMode = PositionMode.FixedX ;
        }

        if ( dischargeGroup?.EdgeList.ElementAtOrDefault( infoDerived.DischargeAdditionalIndexTypeValue[ SingleFilterPatternInfo.DischargeAdditionalIndexType.DisValveA1Valve ] ) is LeafEdge disMainValveA1 ) {
          disMainValveA1.PositionMode = PositionMode.FixedX ;
        }

        if ( suctionGroup?.EdgeList.ElementAtOrDefault( infoDerived.SuctionAdditionalIndexTypeValue[ SingleFilterPatternInfo.SuctionAdditionalIndexType.SucBypassPipeA ] ) is LeafEdge bypass ) {
          bypass.PositionMode = PositionMode.FixedY ;
        }
      }
    }
  }
}
