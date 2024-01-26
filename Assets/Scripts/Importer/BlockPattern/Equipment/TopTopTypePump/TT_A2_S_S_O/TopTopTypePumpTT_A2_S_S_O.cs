using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment.TopTopTypePump.TT_A2_S_S_O
{
  public class TopTopTypePumpTT_A2_S_S_O : TopTopTypePumpBase<BlockPatternArray>
  {
    TopTopTypeMinimumFlowPipeLengthUpdater _lengthUpdater;
    TopTopTypePumpPipeIndicesKeeper _keeper;
    public TopTopTypePumpTT_A2_S_S_O( Document doc ) : base( doc, "TT-A2-S-S-O")
    {
      Info = new SingleBlockPatternIndexInfoWithMinimumFlow
      {
        DischargeIndex = 0,
        MinimumFlowPreIndex = 1,
        MinimumFlowIndex = 2,
        SuctionIndex = 3,
        BasePumpIndex = 0,
        //DischargeAngleGroupIndexList = Enumerable.Range( 3, 8 ).ToList(),
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
        {
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 57 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeNozzle, 1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeMinLength1, 14 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeMinLength2, 18 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStart,  55 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStop,   35 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexOrigin, 34 },

        },
        SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
        {
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzleFlange, 41 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 40 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzleReducer, 39 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, 0 },

          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStart,2 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStop,  37 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexOrigin, 38 },

        },
        MinimumFlowIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType, int>
        {
          { SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.NextOfMinimumFlowEnd, 6 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.MinimumFlowEnd, 7 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.MinimumFlowHeaderBOPSpacer, -1 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.NextOfMinimumFlowHeaderBOPSpacer, -1 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.MinimumFlowSystemFlexStart, 5 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.MinimumFlowSystemFlexStop, 1 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowIndexType.MinimumFlowSystemFlexOrigin, 0 },
        },
        MinimumFlowPreIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfoWithMinimumFlow.MinimumFlowPreIndexType, int>
        {
        },
        SuctionAdditionalIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfoWithMinimumFlow.SuctionAdditionalIndexType,int>{
          { SingleBlockPatternIndexInfoWithMinimumFlow.SuctionAdditionalIndexType.SuctionEndEdge, -1 },
        },
        DischargeAdditionalIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType, int>
        {
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.DischargeNozzleReducer,  2 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.LowerDischargeBOP,  11 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.DischargeBOPSpacer, 33 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.NextOfDischargeBOPSpacer, 32 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.NextOfMinimumFlowStageBOPSpacer, 27 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.MinimumFlowStageBOPSpacer, 28 },

          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.PreDischargeOriFlowAdjuster, 21 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.DischargeOriFlowAdjuster, 22 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.PostDischargeOriFlowAdjuster, 23 },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.DischargeMinimumFlowSpacer, 24 },

          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.MinimumFlowSpacingOrigin,    10  },
          { SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.MinimumFlowAdjustingStart,    9  },


        },
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 56 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 1 }
        },

        SuctionFlexHelper = new int[,]{ { 2, 37 }, },
        DischargeFlexHelper = new int[,]{ { 55, 35  },{ 4, 9 },{ 1, 1 } },
        SuctionPipeIndexRange = new int[,]{ { 0, 40  }, },
        DischargePipeIndexRange = new int[,]{ { 1, 11 },{ 20,57 } },  //  ommit pipes from 14 to 18 to prevent their min-length being changed because they are changed in another rule.

        MinimumFlowInDischargeGroupPipeIndexRange = new int[,]{ { 24,30 } },  //  Remarks: They are in discharge group!

        MinimumFlowPrePipeIndexRange = new int[,]{ { 2,12 } },
        MinimumFlowPipeIndexRange = new int[,] { { 1, 7 } },

        MinimumFlowFlexIndexList = new List<int> { 1,5  },  //  no helper for this

        SuctionDiameterNPSInch = 14,
        DischargeDiameterNPSInch = 14,
        MinimumFlowDiameterNPSInch = 6
      } ;
    }
    internal override void PostProcess()
    {
      base.PostProcess();
      TopTopTypeMinimumFlowExtention.PostProcessMinimumFlow(this);
    }
    
    internal override void SetEdgeNames( SingleBlockPatternIndexInfo info )
    {
      base.SetEdgeNames(info);
      TopTopTypeMinimumFlowExtention.AppendEdgeNames(this,info);
    }

    protected override void SetBlockPatternInfo(SingleBlockPatternIndexInfo info){
      TopTopTypeMinimumFlowExtention.SetBlockPatternInfoMinimumFlow(this,info);
    }

    public Chiyoda.CAD.Topology.BlockPattern Create( Action<Edge> onFinish )
    {
      ImportIdfAndPump() ;
      foreach ( var edge in BaseBp.NonEquipmentEdges ) {
        edge.LocalCod = LocalCodSys3d.Identity ;
      }

      if (!TopTopTypePumpBase.INDEXING_NOW){
        TopTopTypePumpWithMinimumFlowIndexingHelper.BuildIndexList(BpOwner, BaseBp, Info);
        _lengthUpdater = TopTopTypeMinimumFlowPipeLengthUpdater.Create(TopTopTypePumpPipeLengthUpdater.Create(BpOwner, BaseBp, Info));
        if (Info.DischargeAngleGroupIndexList != null) {
          _lengthUpdater.KeepDischargePipeOrderForLaterAdjustment();
          var group = BaseBp.NonEquipmentEdges.ElementAtOrDefault(Info.DischargeIndex) as IGroup;
          if (group != null && Info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived) {
            _keeper = new TopTopTypePumpPipeIndicesKeeper(group, 
              infoDerived.MinimumFlowInDischargeGroupNormalPipes,
              infoDerived.MinimumFlowInDischargeGroupOletPrePostPipes
            );
          }
        }
      }

      PostProcess() ;

      // vertexにflowを設定
      // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
      BpOwner.SetVertexName( "MinimumFlowEnd", "N-3", HalfVertex.FlowType.FromThisToAnother ) ;
      BpOwner.SetVertexName( "DischargeEnd", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
      BpOwner.SetVertexName( "SuctionEnd", "N-1" , HalfVertex.FlowType.FromAnotherToThis ) ;

      onFinish?.Invoke( (BlockEdge) BpOwner ?? BaseBp ) ;

      return BaseBp ;
    }
    internal override void OnGroupChanged()
    {
      if (!TopTopTypePumpBase.INDEXING_NOW) {
        if (Info.DischargeAngleGroupIndexList != null) {
          //  if a group change was made.
          _lengthUpdater.AdjustDischargePipeIndexesAfterReconstructingGroups();
          _lengthUpdater.Activate(true);
          int[][] newindices = _keeper.ReassignIndices();
          if (newindices.Length == 2 && Info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived) {
            infoDerived.MinimumFlowInDischargeGroupNormalPipes = newindices[0];
            infoDerived.MinimumFlowInDischargeGroupOletPrePostPipes = newindices[1];
          }
        }
        TopTopTypeMinimumFlowPipeLengthUpdater.AssortMinimumLengths(BaseBp, this.Info);
      }
    }

    protected override bool SelectIdf( string idf )
    {
       
      if ( ! idf.Contains( base.PumpShapeName ) ) {
        return false ;
      }
      if ( idf.Contains( "-DIS-A" ) || idf.Contains( "-SUC-A" ) || idf.Contains("-MIN-A" )|| idf.Contains("-MIN_-0" )) {
        return true ;
      }
      return false ;
    }

    protected override void RemoveExtraEdges(Group group, string file)
    {
      using ( Group.ContinuityIgnorer( group ) ) {
        List<Edge> removeEdgeList = null ;
        if ( file.Contains( "-DIS-A" ) ) {
          removeEdgeList = group.EdgeList.Reverse().Take( 3 ).ToList() ;
        }
        else if ( file.Contains( "-SUC-A" ) ) {
          removeEdgeList = group.EdgeList.Take( 3 ).ToList() ;
        }
        else if ( file.Contains( "-MIN_-0" ) ) {
          removeEdgeList = group.EdgeList.Reverse().Take( 13 ).ToList() ;
        }
        removeEdgeList?.ForEach( e => e.Unlink() ) ;
      }
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


    internal override void SetPropertyAndRule(SingleBlockPatternIndexInfo info)
    {
      IRule rule;

      //base.PrepareOriflowProperty(info);

      BaseBp.RegisterUserDefinedProperty("AccessSpace", PropertyType.Length, 4.8);

#if false
      bp.RegisterUserDefinedProperty( "PipeMinLength", PropertyType.Length, 0.5 ) ;
      bp.RuleList.AddRule( "#SuctionMinLengthPipe1.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#SuctionMinLengthPipe2.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#DischargeMinLengthPipe1.MinLength", ".PipeMinLength" ) ;
      bp.RuleList.AddRule( "#DischargeMinLengthPipe2.MinLength", ".PipeMinLength" ) ;
#endif

      {
        var bpa = BpOwner;
        bpa.RegisterUserDefinedProperty("BlockCount", PropertyType.GeneralInteger, 1);
        bpa.RuleList.AddRule(".ArrayCount", ".BlockCount");
      }
      BaseBp.RuleList.AddRule(":Parent.ArrayOffsetX", "#BasePump.MaxX - #BasePump.MinX + .AccessSpace ");

      BaseBp.RegisterUserDefinedProperty("SuctionJoinType", CompositeBlockPattern.JoinType.Independent)
        .AddUserDefinedRule(CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule("SuctionEnd"));
      BaseBp.RegisterUserDefinedProperty("DischargeJoinType", CompositeBlockPattern.JoinType.Independent)
        .AddUserDefinedRule(CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule("DischargeEnd"));
      BaseBp.RegisterUserDefinedProperty("MinimumFlowJoinType", CompositeBlockPattern.JoinType.Independent)
        .AddUserDefinedRule(CompositeBlockPattern.UserDefinedRules.GetChangeJointTypeRule("MinimumFlowEnd"));

      TopTopTypePumpHeaderBOPFromStageWithMinimumFlowOverTOP.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      //TopTopTypePumpBasicHeaderIntervalPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);
      TopTopTypePumpMiniFlowVerticalEndHeaderIntervalPropertiesAndRules.Set(BpOwner, BaseBp, Info);

      if (info is SingleBlockPatternIndexInfoWithMinimumFlow infoDerived) {
        TopTopTypeOriFlowTOPPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, infoDerived);

        BaseBp.RegisterUserDefinedProperty( "OriFlowReturnPipeLen", PropertyType.Length,  6.0 ) ;
        rule = BaseBp.RuleList.AddRule("#DischargeOriFlowAdjusterPipe.Length", ".OriFlowReturnPipeLen");
        rule.AddTriggerSourcePropertyName( "DischargeDiameter" ) ;  

        var dischargeGroup = BaseBp.NonEquipmentEdges.ElementAtOrDefault(info.DischargeIndex) as IGroup;
        var edge = dischargeGroup?.EdgeList.ElementAtOrDefault(infoDerived.DischargeAdditionalIndexTypeValue[SingleBlockPatternIndexInfoWithMinimumFlow.DischargeAdditionalIndexType.MinimumFlowSpacingOrigin]) as LeafEdge;
        if (edge != null) {
          edge.PositionMode = PositionMode.FixedX;
        }
        BaseBp.RegisterUserDefinedProperty("StageHeight", PropertyType.Length, 3.55);

        TopTopTypePumpSuctionOnlyStageBOPPropertiesAndRules.Set(BpOwner, BaseBp, Info);
        TopTopTypePumpDischargeStageBOPOverTOPPropertiesAndRules.Set(BpOwner, BaseBp, Info);
        TopTopTypePumpMinimumFlowStageBOPOverTOPPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

        BaseBp.RegisterUserDefinedProperty("MiniFlowSuctionDistance", PropertyType.Length, 1.0);
        BaseBp.RegisterUserDefinedProperty("MFSD_Offset", PropertyType.TemporaryValue, 1.0);
        rule = BaseBp.RuleList.AddRule(".MFSD_Offset",
          "0.0- DebugLog(#MinimumFlowAdjustingStart.MinX)+DebugLog(#NextOfMinimumFlowStageBOPSpacer.MaxX)");
        
        rule = BaseBp.RuleList.AddRule("#MinimumFlowSpacingOrigin.MaxX", 
          "#SuctionSystemFlexOrigin.MinX - .MiniFlowSuctionDistance - .MFSD_Offset"
        );
        
        TopTopTypeMiniFlowDischargeDistancePropertiesAndRules.Set(BpOwner, BaseBp, Info);
      }

      TopTopTypePumpMinimumFlowDiameterPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info, _lengthUpdater);
    }
  }
}
