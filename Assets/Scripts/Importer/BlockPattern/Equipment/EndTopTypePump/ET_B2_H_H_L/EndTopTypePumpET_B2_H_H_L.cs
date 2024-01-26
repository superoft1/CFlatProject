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

namespace Importer.BlockPattern.Equipment.EndTopTypePump.ET_B2_H_H_L
{
  public class EndTopTypePumpET_B2_H_H_L : EndTopTypePumpBase<BlockPatternArray>
  {
    EndTopTypePumpPipeIndexKeeper _keeper;

    public EndTopTypePumpET_B2_H_H_L( Document doc ) : base( doc, "ET-B2-H-H-L")
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
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, 56 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 60 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeMinLength1, 16 }, // TODO MinLengthは一旦設定しない
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeMinLength2, 20 }, // TODO MinLengthは一旦設定しない

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
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.DischargeHeaderBOPSpacer, 58 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.NextOfDischargeHeaderBOPSpacer, 57 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.NextOfDischargeOriFlowPath, 21 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.DischargeOriFlowHeaderBOPGoal, 15 },
        },
        //OriFlowHeaderBOPSpacerRatioForSpacer1 = 0.150/(0.150+1.935),  //  Idfファイルより
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 59 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 1 },
        },
        //SuctionFlexHelper = new int[,]{ { 2, 10 },{ 14, 25 }, },  //  NO.12 is between two flanges with no length.
        DischargeFlexHelper = new int[,]{ 
          { 22, 22  },            //  down tube
          //{ 16,16 },{20, 20  }, //  ori-flow tube
          { 1, 1 },{14,14 },      //  up tube 
        },
        SuctionPipeIndexRange = new int[,]{ { 0, 10 },{14,30}, },
        DischargePipeIndexRange = new int[,] { { 1, 15 }, { 21, 60 } }, //  ommit pipes from 16 to 20 to prevent their min-length being changed because they are changed in another rule.
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

    protected override void ImportIdf(){
      base.ImportIdf();
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(0); //  MinLength 停止
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

      BaseBp.RegisterUserDefinedProperty( "AccessSpace", PropertyType.Length, 5.2 ) ;

      {
        var bpa = BpOwner ;
        bpa.RegisterUserDefinedProperty( "BlockCount", PropertyType.GeneralInteger, 1 ) ;
        bpa.RuleList.AddRule( ".ArrayCount", ".BlockCount" ) ;
      }

      BaseBp.RuleList.AddRule( ":Parent.ArrayOffsetX", "Max( #BasePump.MaxX - #DischargeMinLength1.MinX, #DischargeMinLength1.MaxX - #BasePump.MinX ) + .AccessSpace " ) ;

      EndTopTypePumpMinimumFlowJointsPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, infoDerived);

      EndTopTypePumpMinimumFlowBOPandOriFlowPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, infoDerived);

      EndTopTypeMinimumFlowSuctionNozzlePropertyAndRule.SetPropertiesAndRules(BpOwner, BaseBp, infoDerived);

      EndTopTypePumpMinimumFlowJointDistancePropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, infoDerived);

      //  DischargeAngle
      BaseBp.RegisterUserDefinedProperty( "DischargeDirection", 0, new Dictionary<string, double> { { "Right", 0 }, { "Left", 1 } } ) ;
      BaseBp.RegisterUserDefinedProperty("DischargeAngle", PropertyType.Angle, 0, -60, 45, stepValue: 15);
      BaseBp.RuleList.AddRule( "#DischargeGroup.HorizontalRotationDegree", "If( .DischargeDirection, 180 - .DischargeAngle, .DischargeAngle )" ) ;

      var range = DiameterRange.GetBlockPatternNpsMmRange();
      EndTopTypePumpMinimumFlowDiameterPropertiesAndRules.Set(BpOwner, BaseBp, infoDerived, null, range.min, range.max, range.min, range.max, range.min, range.max);
    }
  }
}
