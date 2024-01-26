//#define HEADER_INTERVAL_ON

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
using Chiyoda.DB ;
using Chiyoda.Importer ;
using IDF ;
using UnityEngine ;
using Group = Chiyoda.CAD.Topology.Group ;

namespace Importer.BlockPattern.Equipment.EndTopTypePump.ET_A2_H_H_L
{
  public class EndTopTypePumpET_A2_H_H_L : EndTopTypePumpBase<BlockPatternArray>
  {
    EndTopTypePumpPipeIndexKeeper _keeper;

    public EndTopTypePumpET_A2_H_H_L( Document doc ) : base( doc, "ET-A2-H-H-L")
    {
      Info = new SingleBlockPatternIndexInfoWithMinimumFlow
      {
        DischargeIndex = 0,
        MinimumFlowIndex = 1,
        SuctionIndex = 2,
        BasePumpIndex = 0,
        DischargeAngleGroupIndexList = Enumerable.Range( 2, 12 ).ToList(),
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
        {
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeNozzle, 0 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, 48 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 52 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeMinLength1, 16 }, // 流量計手前の直管
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeMinLength2, 20 }, // 流量計直後の直管
        },
        SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
        {
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 30 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, 0 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStart,  2 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStop,   28 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexOrigin, 29 }, //  suction origin (reducer)
        },
        MinimumFlowIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType, int>
        {
          { SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.NextOfMinimumFlowEnd, 25 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.MinimumFlowEnd, 26 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.MinimumFlowHeaderBOPSpacer, 24 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.NextOfMinimumFlowHeaderBOPSpacer, 23 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.MinimumFlowInletReducer, 0 },
        },
        DischargeAdditionalIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType, int>
        {
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.DischargeHeaderBOPSpacer, 50 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.NextOfDischargeHeaderBOPSpacer, 49 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.NextOfDischargeOriFlowPath, 21 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.DischargeOriFlowDownTube, 22 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.DischargeOriFlowHeaderBOPGoal, 15 },
        },
        //OriFlowHeaderBOPSpacerRatioForSpacer1 = (0.150/(0.150+1.925)),  //  初期比率
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 51 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 1 },
        },

        #if HEADER_INTERVAL_ON
        SuctionFlexHelper = new int[,]{ { 2, 10 },{ 14, 25 }, },
        #endif

        DischargeFlexHelper = new int[,]{ 
          { 22, 22  },        //  down tube
#if HEADER_INTERVAL_ON
          { 16,16 },{20, 20  },        //  ori-flow tube
#endif
          { 1, 1 },{14,14 },  //  up tube 
        },
        SuctionPipeIndexRange = new int[,]{ { 0, 10 },{14,30}, },
  
        DischargePipeIndexRange = new int[,] { { 1, 15 }, { 21, 52 } }, //  to avoid 16,20 pipe's min length being changed because they are changed in another rule.
        MinimumFlowPipeIndexRange = new int[,] { { 1, 26 } },
        OriFlowInOutPipeIndexList = new int[] { 16,20 },

        SuctionDiameterNPSInch = 12,      
        DischargeDiameterNPSInch = 10,    
        MinimumFlowDiameterNPSInch = 4,   
      } ;
    }

    public Chiyoda.CAD.Topology.BlockPattern Create( Action<Edge> onFinish )
    {
      return EndTopTypePumpImport( onFinish ) ;
    }

    protected override bool SelectIdf( string idf )
    {  
      if ( ! idf.Contains( base.PumpShapeName ) ) {
        return false ;
      }
      if ( idf.Contains( "-DIS-A" ) || idf.Contains( "-SUC-A" ) || idf.Contains("-MIN-A" )) {
        return true ;
      }
      return false ;
    }

    internal override void SetEdgeNames( SingleBlockPatternIndexInfo info )
    {
      base.SetEdgeNames(info);
      EndTopTypeMinimumFlowExtension.AppendEdgeNames(this,info);
    }

    protected override void SetBlockPatternInfo(SingleBlockPatternIndexInfo info)
    {
      EndTopTypeMinimumFlowExtension.SetBlockPatternInfoWithMinimumFlow(this,info);
    }

    protected override void PostProcess()
    {
      base.PostProcess();
      EndTopTypeMinimumFlowExtension.PostPostProcessWithMinimumFlow(this,Info);

    }

    Chiyoda.CAD.Topology.BlockPattern EndTopTypePumpImport(
      Action<Edge> onFinish = null )
    {
      if (!(Info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived))
        return null;

      ImportIdfAndPump() ;

      foreach ( var edge in BaseBp.NonEquipmentEdges ) {
        edge.LocalCod = LocalCodSys3d.Identity ;
      }

      EndTopTypeMinimumFlowPipeIndexHelper.BuildIndexList(BpOwner, BaseBp, Info);

      var dischargeGroup = GetGroup(Info.DischargeIndex);
      _keeper = new EndTopTypePumpPipeIndexKeeper(dischargeGroup, infoDerived.OriFlowInOutPipeIndexList);

      PostProcess() ;

      // vertexにflowを設定
      // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
      BpOwner.SetVertexName( "MinimumFlowEnd", "N-3", HalfVertex.FlowType.FromThisToAnother ) ;
      BpOwner.SetVertexName( "DischargeEnd", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
      BpOwner.SetVertexName( "SuctionEnd", "N-1" , HalfVertex.FlowType.FromAnotherToThis ) ;

      onFinish?.Invoke( (BlockEdge) BpOwner ?? BaseBp ) ;

      return BaseBp ;
    }

    protected override void ImportIdf(){
      base.ImportIdf();
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(0); //  MinLength 停止
    }
    /// <summary>
    /// 基底クラスでグループが組み替えられると呼ばれる
    /// </summary>
    /// <returns></returns>
    internal override void OnGroupChanged()
    {
      //  Ori flow 用に使用する必要あり
      if (Info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived) {
        var dischargeGroup = GetGroup(Info.DischargeIndex);

        infoDerived.OriFlowInOutPipeIndexList = _keeper.ReassignIndices(infoDerived.OriFlowInOutPipeIndexList);
        BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(1); //  MinLength 反映　ToDo: ori frow In / Out を解除

        //  Ori flow のMinimumLength 更新をしない
        foreach (var index in infoDerived.OriFlowInOutPipeIndexList){
          if ( GetEdge(dischargeGroup, index).PipingPiece is Pipe pipe){
            pipe.MinimumLengthRatioByDiameter = 0.0;
          }
        }
      }
    }

    protected override void RemoveExtraEdges( Group group, string file )
    {
      using ( Group.ContinuityIgnorer( group ) ) {
        List<Edge> removeEdgeList = null ;
        if ( file.Contains( "-DIS-A" ) ) {
          removeEdgeList = group.EdgeList.Reverse().Take( 3 ).ToList() ;
        }
        else if (file.Contains( "-MIN-A" )) {
          removeEdgeList = group.EdgeList.Reverse().Take( 3 ).ToList() ;
        }
        else if (file.Contains( "-SUC-A" )) {
          removeEdgeList = group.EdgeList.Take( 1 ).ToList() ;
        }
        removeEdgeList?.ForEach( e => e.Unlink() ) ;
      }
    }


    /// <summary>
    /// IDFにノズル側にフランジ形状が潰れてしまっているものがあれば追加する
    /// </summary>
    /// <param name="group"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    protected override LeafEdge GetNozzleSideFlange( Group group, string file )
    {
      return null ;
    }

    internal override void SetPropertyAndRule( SingleBlockPatternIndexInfo info )
    {
      if (EndTopTypePumpBase.INDEXING_NOW)
        return;
      if (!(info is SingleBlockPatternIndexInfoWithMinimumFlow)){
        return; //  error
      }
      var infoDerived = (SingleBlockPatternIndexInfoWithMinimumFlow)info;

      BaseBp.RegisterUserDefinedProperty( "AccessSpace", PropertyType.Length, 4.4 ) ;

      {
        var bpa = BpOwner ;
        bpa.RegisterUserDefinedProperty( "BlockCount", PropertyType.GeneralInteger, 1 ) ;
        bpa.RuleList.AddRule( ".ArrayCount", ".BlockCount" ) ;
      }

      BaseBp.RuleList.AddRule( ":Parent.ArrayOffsetX", "Max( #BasePump.MaxX - #DischargeMinLength1.MinX, #DischargeMinLength1.MaxX - #BasePump.MinX ) + .AccessSpace " ) ;


      EndTopTypePumpMinimumFlowJointsPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, infoDerived);  //  Joint types

      EndTopTypePumpMinimumFlowBOPandOriFlowPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, infoDerived);

      EndTopTypeMinimumFlowSuctionNozzlePropertyAndRule.SetPropertiesAndRules(BpOwner, BaseBp, infoDerived);  //  Nozzle pipe length

      //EndTopTypePumpMinimumFlowJointDistancePropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, infoDerived);
　    {
        IRule rule;
        var bp = BaseBp;
        bp.RegisterUserDefinedProperty( "MiniFlowJointDistance", PropertyType.Length, 0.7 ) ;
        bp.RegisterUserDefinedProperty( "MHJ_Offset", PropertyType.TemporaryValue, 0.0  ) ; //  offset for MiniFlowHeaderJointDistance
        bp.RuleList.AddRule(".MHJ_Offset", "2.0*(DiameterToElbow90Length( #MinimumFlowEndPipe.Diameter ) - #MinimumFlowEndPipe.Diameter*0.5)");
        rule = bp.RuleList.AddRule("#MinimumFlowEndPipe.Length", ".MiniFlowJointDistance - .MHJ_Offset");
        rule.AddTriggerSourcePropertyName( "MiniFlowDiameter" ) ;
        rule.AddTriggerSourcePropertyName( "DischargeBOP","PosY" ) ;
      
        bp.RegisterUserDefinedProperty( "DischargeHeaderJointDistance", PropertyType.Length, 0.7 ) ;
        bp.RegisterUserDefinedProperty( "DHJ_Offset", PropertyType.TemporaryValue, 0.0  ) ; //  offset for MiniFlowHeaderJointDistance
        bp.RuleList.AddRule(".DHJ_Offset", "2.0*(DiameterToElbow90Length( #DischargeEndPipe.Diameter ) - #DischargeEndPipe.Diameter*0.5)");

        rule = bp.RuleList.AddRule("#DischargeEndPipe.Length", ".DischargeHeaderJointDistance - .DHJ_Offset");
        rule.AddTriggerSourcePropertyName( "DischargeDiameter" ) ;
        rule.AddTriggerSourcePropertyName( "DischargeBOP","PosY" ) ;

#if HEADER_INTERVAL_ON

        //  HeaderIntereval
        bp.RegisterUserDefinedProperty("HeaderInterval", PropertyType.Length, 4.5);

        //  各JointEdge とBasePump との距離を設定
        bp.RegisterUserDefinedProperty("D_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  Dischare パイプの一番短くできる長さ
        bp.RegisterUserDefinedProperty("S_MinSysLength", PropertyType.TemporaryValue, 0.0);  //  Suctionパイプの一番短くできる長さ
        bp.RegisterUserDefinedProperty("D_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  DischargeEnd の一番近くできる座標
        bp.RegisterUserDefinedProperty("S_MinPos_Y", PropertyType.TemporaryValue, 0.0);  //  SuctionEnd の一番近くできる座標
        bp.RegisterUserDefinedProperty("S_Base", PropertyType.TemporaryValue, 0.0);  // Discharge基準は0 Suction基準は1
        bp.RegisterUserDefinedProperty("D_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
        bp.RegisterUserDefinedProperty("S_EndPos_Y", PropertyType.TemporaryValue, 0.0);   // 
        bp.RegisterUserDefinedProperty("D_EndPos_Y2", PropertyType.TemporaryValue, 0.0);  // 
        bp.RegisterUserDefinedProperty("S_EndPos_Y2", PropertyType.TemporaryValue, 0.0);  // 
        bp.RegisterUserDefinedProperty("D_EndPos_Y3", PropertyType.TemporaryValue, 0.0);  // 
        bp.RegisterUserDefinedProperty("S_EndPos_Y3", PropertyType.TemporaryValue, 0.0);  // 
        bp.RegisterUserDefinedProperty("D_EndPos_Offset", PropertyType.TemporaryValue, 0.0);  // 
        bp.RegisterUserDefinedProperty("HI_Offset", PropertyType.TemporaryValue, 0.0);  //  header interval offset
        rule = bp.RuleList.AddRule(".HI_Offset", "(#DischargeEndPipe.Diameter + #SuctionEndPipe.Diameter)*0.5");
        rule = bp.RuleList.AddRule(".D_EndPos_Offset", "3.0*DiameterToElbow90Length(#DischargeEndPipe.Diameter)+#DischargeEndPipe.Length");

        //  System length      
        rule = bp.RuleList.AddRule(".D_MinSysLength", "MinHorzDistanceOf(#DischargeMinLength1, #DischargeMinLength2)+(#DischargeMinLengthPipe1.MinLength + #DischargeMinLengthPipe2.MinLength)*0.5");
        rule = bp.RuleList.AddRule(".S_MinSysLength", "MinHorzDistanceOf(#SuctionSystemFlexStart, #SuctionSystemFlexStop)+(#SuctionSystemFlexStartPipe.MinLength + #SuctionSystemFlexStopPipe.MinLength)*0.5");

        //  Minimum Y coord of DischargeEnd
        rule = bp.RuleList.AddRule(".D_MinPos_Y", "#DischargeOriFlowHeaderBOPGoal.MaxY + .D_MinSysLength + .D_EndPos_Offset"); 

        //  Minimum Y coord of SuctionEnd
        rule = bp.RuleList.AddRule(".S_MinPos_Y", "#SuctionSystemFlexOrigin.MaxY + .S_MinSysLength + DiameterToElbow90Length(#SuctionEndPipe.Diameter)"); 

        //  Select origin position 
        rule = bp.RuleList.AddRule(".S_Base", "IF(.S_MinPos_Y >=  .D_MinPos_Y, 1.0, 0.0)"); 

        //  基準座標設定
        rule = bp.RuleList.AddRule(".S_EndPos_Y", "IF(.S_Base > 0, .S_MinPos_Y, IF(.HeaderInterval >= 0, .D_MinPos_Y-.HI_Offset - .HeaderInterval, .D_MinPos_Y + .HI_Offset - .HeaderInterval ))"); 
        rule = bp.RuleList.AddRule(".D_EndPos_Y", "IF(.S_Base > 0, IF(.HeaderInterval >= 0, .S_MinPos_Y - .HI_Offset - .HeaderInterval, .S_MinPos_Y + .HI_Offset - .HeaderInterval), .D_MinPos_Y)");

        rule = bp.RuleList.AddRule(".S_EndPos_Y2", "Max(.S_EndPos_Y, .S_MinPos_Y)");
        rule = bp.RuleList.AddRule(".D_EndPos_Y2", "Max(.D_EndPos_Y, .D_MinPos_Y)");

        //  移動不可能時設定
        rule = bp.RuleList.AddRule(".S_EndPos_Y3", "IF(.S_Base > 0, IF (.HeaderInterval >= 0, .D_EndPos_Y2 + .HeaderInterval + .HI_Offset, .S_EndPos_Y2), .S_EndPos_Y2)"); 
        rule = bp.RuleList.AddRule(".D_EndPos_Y3", "IF(.S_Base > 0, .D_EndPos_Y2, IF (.HeaderInterval >= 0, .S_EndPos_Y2 + .HeaderInterval +.HI_Offset, .D_EndPos_Y2))"); 

        //  LeafEdge に反映
        rule = bp.RuleList.AddRule("#SuctionEnd.PosY", ".S_EndPos_Y3");
        rule = bp.RuleList.AddRule("#NextOfDischargeOriFlowPath.MinY", ".D_EndPos_Y3 - .D_EndPos_Offset");

        { 
          var suctionGroup = bp.NonEquipmentEdges.ElementAtOrDefault(info.SuctionIndex) as IGroup;
          var edge = suctionGroup?.EdgeList.ElementAtOrDefault(info.SuctionIndexTypeValue[SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd]) as LeafEdge;
          if (edge != null){
            edge.PositionMode = PositionMode.FixedY ;
          }
        }
#endif
      }

      //  DischargeAngle
      BaseBp.RegisterUserDefinedProperty( "DischargeDirection", 0, new Dictionary<string, double> { { "Right", 0 }, { "Left", 1 } } ) ;
      BaseBp.RegisterUserDefinedProperty("DischargeAngle", PropertyType.Angle, 0, -60, 45, stepValue: 15);
      BaseBp.RuleList.AddRule( "#DischargeGroup.HorizontalRotationDegree", "If( .DischargeDirection, 180 - .DischargeAngle, .DischargeAngle )" ) ;

      var range = DiameterRange.GetBlockPatternNpsMmRange();
      EndTopTypePumpMinimumFlowDiameterPropertiesAndRules.Set(BpOwner, BaseBp, infoDerived, null , range.min, range.max, range.min, range.max, range.min, range.max);
    }
  }
}
