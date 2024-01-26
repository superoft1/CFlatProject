//#define SET_PREFERRED_LENGTH_AS_MINIMUM_LENGTH

using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment.TopTopTypePump.TT_A1_S_G_N
{
  public class TopTopTypePumpTT_A1_S_G_N : TopTopTypePumpBase<BlockPatternArray>
  {
    TopTopTypePumpPipeIndicesKeeper _keeper;
    public TopTopTypePumpTT_A1_S_G_N(Document doc) : base(doc, "TT-A1-S-G-N")
    {
      Info = new SingleBlockPatternIndexInfo {
        DischargeIndex = 0,
        SuctionIndex = 1,
        BasePumpIndex = 0,
        DischargeAngleGroupIndexList = Enumerable.Range(3, 8).ToList(),
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
        {
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, 30 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 32 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeNozzle,  1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPSpacer, 11 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark1,    0 }, //  nozzle-side flange
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark2,    2 }, //  reducer
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark3,   10 }, //  go down elbow
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStart,30 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStop, 13 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexOrigin,   12 },
        },
        SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
        {
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzleFlange, 40 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 39 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzleReducer, 33 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, 0 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStart,  2 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStop,   34 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexOrigin, 35 }, //  suction origin (reducer)
        },
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 31 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 1 },
        },

        SuctionFlexHelper = new int[,] { { 2, 34 }, },
        DischargeFlexHelper = new int[,] { { 30, 13 }, { 1, 1 } },
        SuctionPipeIndexRange = new int[,] { { 0, 39 }, },
        DischargePipeIndexRange = new int[,] { { 1, 32 }, },


        SuctionDiameterNPSInch = 14,
        DischargeDiameterNPSInch = 14
      };
    }

    public Chiyoda.CAD.Topology.BlockPattern Create(Action<Edge> onFinish)
    {
      ImportIdfAndPump();
      foreach (var edge in BaseBp.NonEquipmentEdges) {
        edge.LocalCod = LocalCodSys3d.Identity;
      }

      TopTopTypePumpIndexingHelper.BuildIndexList(BpOwner, BaseBp, Info);
 

      var dischargeGroup = GetGroup(Info.DischargeIndex);

      //  Test 用にインデックス再現を行う
      _keeper = new TopTopTypePumpPipeIndicesKeeper(dischargeGroup, Info.DischargeNormalPipes, Info.DischargeOletPrePostPipes);

      PostProcess();


      // vertexにflowを設定
      // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
      BpOwner.SetVertexName("DischargeEnd", "N-2", HalfVertex.FlowType.FromThisToAnother);
      BpOwner.SetVertexName("SuctionEnd", "N-1", HalfVertex.FlowType.FromAnotherToThis);

      onFinish?.Invoke((BlockEdge)BpOwner ?? BaseBp);

      return BaseBp;
    }

    internal override void OnGroupChanged()
    {
      //  グループ替え後のインデックス再配置に対応
      int[][]result = _keeper.ReassignIndices();
      Info.DischargeNormalPipes = result[0];
      Info.DischargeOletPrePostPipes = result[1];
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(1); //  MinLength 反映

#if SET_PREFERRED_LENGTH_AS_MINIMUM_LENGTH
      var dg = BaseBp.NonEquipmentEdges.ElementAtOrDefault( Info.DischargeIndex ) as IGroup ;
      var sg = BaseBp.NonEquipmentEdges.ElementAtOrDefault( Info.SuctionIndex ) as IGroup ;

      LeafEdge[] les = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(dg);
      int[] arrayMerged = new int[Info.DischargeNormalPipes.Length + Info.DischargeOletPrePostPipes.Length];
      Info.DischargeNormalPipes.CopyTo(arrayMerged, 0);
      Info.DischargeOletPrePostPipes.CopyTo(arrayMerged, Info.DischargeNormalPipes.Length);
      foreach (int i in arrayMerged){
        if (!(les[i].PipingPiece is Pipe pipe)) continue;
        pipe.PreferredLength = pipe.MinimumLength;
      }

      les = TopTopTypePumpGroupLeafEdgeExtractor.ExtractLeafEdgeArrayFromGroupHierarachy(sg);
      arrayMerged = new int[Info.SuctionNormalPipes.Length + Info.SuctionOletPrePostPipes.Length];
      Info.SuctionNormalPipes.CopyTo(arrayMerged, 0);
      Info.SuctionOletPrePostPipes.CopyTo(arrayMerged, Info.SuctionNormalPipes.Length);
      foreach (int i in arrayMerged){
        if (!(les[i].PipingPiece is Pipe pipe)) continue;
        pipe.PreferredLength = pipe.MinimumLength;
      }
#endif
    }

    protected override void ImportIdf() {
      base.ImportIdf();
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(0); //  MinLength 停止
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


    internal override void SetPropertyAndRule( SingleBlockPatternIndexInfo info )
    {
      {
        var bpa = BpOwner ;
        bpa.RegisterUserDefinedProperty( "BlockCount", PropertyType.GeneralInteger, 1 ) ;
        bpa.RuleList.AddRule( ".ArrayCount", ".BlockCount" ) ;
      }
      BaseBp.RegisterUserDefinedProperty( "AccessSpace", PropertyType.Length, 2.7 ) ;
      BaseBp.RuleList.AddRule( ":Parent.ArrayOffsetX", "Max( #BasePump.MaxX - #DischargeSystemFlexStart.MinX, #DischargeSystemFlexStart.MaxX - #BasePump.MinX ) + .AccessSpace " ) ;

      TopTopTypePumpBasicJointPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      TopTopTypePumpDischargeHeaderBOPFromGL.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      TopTopTypePumpSuctionHeaderBOPFromStage.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      TopTopTypePumpBasicHeaderIntervalPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      TopTopTypePumpUpDownBOPPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      BaseBp.RegisterUserDefinedProperty( "StageHeight", PropertyType.Length, 3.55 ) ;

      TopTopTypePumpSuctionStageBOPPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      BaseBp.RegisterUserDefinedProperty("DischargeAngle", PropertyType.Angle, 0, -60, 45, stepValue: 15);
      BaseBp.RuleList.AddRule( "#DischargeGroup.HorizontalRotationDegree", ".DischargeAngle" ) ;

      TopTopTypePumpBasicDiameterPropertiesAndRules.Set(BpOwner, BaseBp, Info, null);

      BaseBp.RegisterUserDefinedProperty("Dummy", PropertyType.TemporaryValue, 0.0);   // Y座標不足分
      BaseBp.RuleList.AddRule(".Dummy", "DebugLog(#SuctionEnd.PosY)");

    }
  }
}
