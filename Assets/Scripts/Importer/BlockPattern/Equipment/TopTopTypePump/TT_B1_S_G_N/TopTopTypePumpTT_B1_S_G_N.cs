using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment.TopTopTypePump.TT_B1_S_G_N
{
  public class TopTopTypePumpTT_B1_S_G_N : TopTopTypePumpBase<BlockPatternArray>
  {
    //TopTopTypePumpPipeLengthUpdater _lengthUpdater;

    public TopTopTypePumpTT_B1_S_G_N( Document doc ) : base( doc, "TT-B1-S-G-N")
    {
      Info = new SingleBlockPatternIndexInfo
      {
        DischargeIndex = 0,
        SuctionIndex = 1,
        BasePumpIndex = 0,
        DischargeAngleGroupIndexList = Enumerable.Range( 3, 8 ).ToList(),
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
        {
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 40 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeNozzle, 1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPSpacer, 11 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark1,    0 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark2,    2 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark3,   10 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStart,38 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStop, 13 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexOrigin,   12 },
        },
        SuctionIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.SuctionIndexType, int>
        {
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzleFlange, 40 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzle, 39 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionNozzleReducer, 38 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionEnd, 0 },

          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStart,  2 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexStop,   34 },
          { SingleBlockPatternIndexInfo.SuctionIndexType.SuctionSystemFlexOrigin, 35 }, //  suction origin (reducer)

        },
        NextOfIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.NextOfIndexType, int>
        {
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 39 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 1 }
        },

        SuctionFlexHelper = new int[,]{ { 2, 34 }, },
        DischargeFlexHelper = new int[,]{ { 38, 13  },{ 1, 1 } },
        SuctionPipeIndexRange = new int[,]{ { 0, 39  }, },
        DischargePipeIndexRange = new int[,]{ { 1,40 }, },

        SuctionDiameterNPSInch = 14,
        DischargeDiameterNPSInch = 14
      } ;
    }
    
    public Chiyoda.CAD.Topology.BlockPattern Create( Action<Edge> onFinish )
    {
      ImportIdfAndPump() ;
      foreach ( var edge in BaseBp.NonEquipmentEdges ) {
        edge.LocalCod = LocalCodSys3d.Identity ;
      }

      TopTopTypePumpIndexingHelper.BuildIndexList(BpOwner, BaseBp, Info);
      //_lengthUpdater = TopTopTypePumpPipeLengthUpdater.Create(BpOwner,BaseBp,Info);
      //_lengthUpdater.KeepDischargePipeOrderForLaterAdjustment();

      PostProcess() ;

      // vertexにflowを設定
      // 最終的には配管全体に向きを設定する事になるが、とりあえず暫定的に設定
      BpOwner.SetVertexName( "DischargeEnd", "N-2", HalfVertex.FlowType.FromThisToAnother ) ;
      BpOwner.SetVertexName( "SuctionEnd", "N-1" , HalfVertex.FlowType.FromAnotherToThis ) ;

      onFinish?.Invoke( (BlockEdge) BpOwner ?? BaseBp ) ;

      return BaseBp ;
    }
    internal override void OnGroupChanged()
    {
      BaseBp.SetMinimumLengthRatioByDiameterForAllPipes(1); //  MinLength 反映
      //_lengthUpdater.AdjustDischargePipeIndexesAfterReconstructingGroups();
      //_lengthUpdater.Activate(true);
      //TopTopTypePumpPipeLengthUpdater.AssortMinimumLengths(BaseBp, this.Info);
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

      TopTopTypePumpBasicDiameterPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info, null);
    }
  }
}
