//#define WRONG_DIAMETER

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Chiyoda;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using Chiyoda.DB;
using Chiyoda.Importer;
using IDF;
using UnityEngine;
using Group = Chiyoda.CAD.Topology.Group;

namespace Importer.BlockPattern.Equipment.EndTopTypePump.ET_A1_H_H_L {
  public class EndTopTypePumpET_A1_H_H_L : EndTopTypePumpBase<BlockPatternArray> {

    EndTopTypePumpLengthUpdater _lengthUpdater;

    public EndTopTypePumpET_A1_H_H_L(Document doc) : base(doc, "ET-A1-H-H-L") {
      Info = new SingleBlockPatternIndexInfo {
        DischargeIndex = 0,
        SuctionIndex = 1,
        BasePumpIndex = 0,
        DischargeAngleGroupIndexList = Enumerable.Range(3, 8).ToList(),
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
        {
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeNozzle, 1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 32 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPSpacer, 11 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark1,    0 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark2,    2 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark3,   10 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStart,30 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStop, 13 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexOrigin,   12 },
        },
        SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
        {
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 27 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, 0 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStart,  2 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStop,   25 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexOrigin, 26 }, //  suction origin (reducer)
        },
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 31 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 1 },
        },
        
        SuctionFlexHelper = new int[,]{ { 2, 25 }, },
        DischargeFlexHelper = new int[,]{ { 30, 13  },{ 1, 1 } },
        SuctionPipeIndexRange = new int[,]{ { 0,27  }, },
        DischargePipeIndexRange = new int[,]{ { 1,32 }, },
        
        SuctionFlexIndexList = new List<int> { /*2,6,11,12,16,17*/ },     //  
        SuctionDiameterNPSInch = 12,  
        DischargeDiameterNPSInch = 10,
      };
    }

    public Chiyoda.CAD.Topology.BlockPattern Create(Action<Edge> onFinish) {
      ImportIdfAndPump();
      foreach (var edge in BaseBp.NonEquipmentEdges) {
        edge.LocalCod = LocalCodSys3d.Identity;
      }

      EndTopTypePumpPipeIndexHelper.BuildIndexList(BpOwner, BaseBp, Info);
      //EndTopTypePumpLengthUpdater.AssortMinimumLengths(BaseBp, this.Info);  //  IDFの初期径に対応
      //_lengthUpdater = EndTopTypePumpLengthUpdater.Create(BpOwner, BaseBp, Info);
      //_lengthUpdater.KeepDischargePipeOrderForLaterAdjustment();

      PostProcess() ;
      
      var bpa = BpOwner ;
      
      if ( ! EndTopTypePumpBase.INDEXING_NOW ) { 
        //bpa.GetProperty( "HeaderInterval" ).Value = 0.5 ;
        // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
        bpa.SetVertexName( "DischargeEnd", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
        bpa.SetVertexName( "SuctionEnd", "N-1" , HalfVertex.FlowType.FromAnotherToThis ) ;
      }
      
      onFinish?.Invoke( (BlockEdge) BpOwner ?? BaseBp ) ;

      /*  無理やりソフト対応するならこう
      double dinch = Info.DischargeDiameterNPSInch;
      double sinch = Info.SuctionDiameterNPSInch;
      bpa.GetProperty("DischargeDiameter").Value = DiameterFactory.FromNpsInch(dinch + ((dinch<6)?1.0:2.0)).NpsMm;
      bpa.GetProperty("SuctionDiameter").Value = DiameterFactory.FromNpsInch(sinch + ((sinch<6)?1.0:2.0)).NpsMm;

      bpa.GetProperty("DischargeDiameter").Value = DiameterFactory.FromNpsInch(Info.DischargeDiameterNPSInch).NpsMm;
      bpa.GetProperty("SuctionDiameter").Value = DiameterFactory.FromNpsInch(Info.SuctionDiameterNPSInch).NpsMm;
      */

      return BaseBp;
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
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(1); //  MinLength 反映
    }

    protected override void RemoveExtraEdges(Group group, string file) {
      using (Group.ContinuityIgnorer(group)) {
        List<Edge> removeEdgeList = null;
        if (file.Contains("-DIS-A")) {
          removeEdgeList = group.EdgeList.Reverse().Take(3).ToList();
        }
        else if (file.Contains("-SUC-A")) {
          removeEdgeList = group.EdgeList.Take(3).ToList();
        }

        removeEdgeList?.ForEach(e => e.Unlink());
      }
    }


    /// <summary>
    /// IDFにノズル側にフランジ形状が潰れてしまっているので特別に追加する
    /// </summary>
    /// <param name="group"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    protected override LeafEdge GetNozzleSideFlange(Group group, string file) {

      return null;
    }
    #if false
    protected  double _DischargeSuctionDistanceSpacer1Length = 0.398;
    protected  double _DischargeSuctionDistanceSpacer2Length = 0.172;

        private double GetPipeLength(IGroup group, int index){
      var edge = GetEdge(group, index);
      var pipe = edge?.PipingPiece as Chiyoda.CAD.Model.Pipe;
      return (pipe != null)?pipe.Length:1.0;
    }

    private void PrepareInitialLength(SingleBlockPatternIndexInfo info ){ 
      var dischargeGroup = GetGroup( info.DischargeIndex ) ;
      var suctionGroup = GetGroup( info.SuctionIndex ) ;
      _DischargeSuctionDistanceSpacer1Length = GetPipeLength(dischargeGroup, info.DischargeIndexTypeValue[ SingleBlockPatternIndexInfo.DischargeIndexType.DischargeOletPre1 ] ) ;
      _DischargeSuctionDistanceSpacer2Length = GetPipeLength(dischargeGroup, info.DischargeIndexTypeValue[ SingleBlockPatternIndexInfo.DischargeIndexType.DischargeOletPost1 ] ) ;
    }
    #endif

    internal override void SetPropertyAndRule(SingleBlockPatternIndexInfo info)
    {
    #if true
      //PrepareInitialLength(info);
      BaseBp.RegisterUserDefinedProperty("AccessSpace", PropertyType.Length, 1.2);

      {
        var bpa = BpOwner;
        bpa.RegisterUserDefinedProperty("BlockCount", PropertyType.GeneralInteger, 1);
        bpa.RuleList.AddRule(".ArrayCount", ".BlockCount");
      }

      //  これは独自色が強いので共通化しない
      BaseBp.RuleList.AddRule(":Parent.ArrayOffsetX", "Max( #BasePump.MaxX - #DischargeSystemFlexStart.MinX, #DischargeSystemFlexStart.MaxX - #BasePump.MinX ) + .AccessSpace ");

      EndTopTypePumpBasicPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      EndTopTypePumpUpDownBOPPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      EndTopTypePumpBasicHeaderIntervalPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);


      //  Discharge Direction 設定
      BaseBp.RegisterUserDefinedProperty("DischargeDirection", 0, new Dictionary<string, double> { { "Right", 0 }, { "Left", 1 } });
      BaseBp.RegisterUserDefinedProperty("DischargeAngle", PropertyType.Angle, 0, -60, 45, stepValue: 15);
      BaseBp.RuleList.AddRule("#DischargeGroup.HorizontalRotationDegree", "If( .DischargeDirection, 180 - .DischargeAngle, .DischargeAngle )");

      var range = DiameterRange.GetBlockPatternNpsMmRange();
      #if ! WRONG_DIAMETER
      EndTopTypePumpBasicDiameterPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info,_lengthUpdater, range.min, range.max, range.min, range.max);
      #else
      EndTopTypePumpErrorDiameterPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info,_lengthUpdater, range.min, range.max, range.min, range.max);
      #endif
    #endif
    }
  }
}
