using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment.TopTopTypePump.TT_A1_S_S_N
{
  public class TopTopTypePumpTT_A1_S_S_N : TopTopTypePumpBase<BlockPatternArray>
  {

    public TopTopTypePumpTT_A1_S_S_N( Document doc ) : base( doc, "TT-A1-S-S-N" )
    {
      Info = new SingleBlockPatternIndexInfo
      {
        DischargeIndex = 0,
        SuctionIndex = 1,
        BasePumpIndex = 0,
        //DischargeAngleGroupIndexList = Enumerable.Range( 3, 3 ).ToList(),
        DischargeIndexTypeValue = new Dictionary<SingleBlockPatternIndexInfo.DischargeIndexType, int>
        {
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOP, -1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeEnd, 33 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeNozzle, 1 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPSpacer, 11 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark1,    0 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark2,    2 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeBOPMark3,   10 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStart,31 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexStop,  6 },
          { SingleBlockPatternIndexInfo.DischargeIndexType.DischargeSystemFlexOrigin, 5 },

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
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfDischargeEnd, 32 },
          { SingleBlockPatternIndexInfo.NextOfIndexType.NextOfSuctionEnd, 1 }
        },

        SuctionFlexHelper = new int[,]{ { 2, 34 }, },
        DischargeFlexHelper = new int[,]{ { 31, 6  },{ 1, 1 } },
        SuctionPipeIndexRange = new int[,]{ { 0, 39  }, },
        DischargePipeIndexRange = new int[,]{ { 1,33 }, },

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

      BaseBp.RegisterUserDefinedProperty( "AccessSpace", PropertyType.Length, 3.7 ) ;
      {
        var bpa = BpOwner ;
        bpa.RegisterUserDefinedProperty( "BlockCount", PropertyType.GeneralInteger, 1 ) ;
        bpa.RuleList.AddRule( ".ArrayCount", ".BlockCount" ) ;
      }

      BaseBp.RuleList.AddRule( ":Parent.ArrayOffsetX", "#BasePump.MaxX - #BasePump.MinX + .AccessSpace " ) ;

      TopTopTypePumpBasicJointPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);
      
      TopTopTypePumpHeaderBOPFromStage.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      TopTopTypePumpBasicHeaderIntervalPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      BaseBp.RegisterUserDefinedProperty( "StageHeight", PropertyType.Length, 3.55 ) ;

      TopTopTypePumpStageBOPPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info);

      TopTopTypePumpBasicDiameterPropertiesAndRules.SetPropertiesAndRules(BpOwner, BaseBp, Info, null);
    }
  }
}
