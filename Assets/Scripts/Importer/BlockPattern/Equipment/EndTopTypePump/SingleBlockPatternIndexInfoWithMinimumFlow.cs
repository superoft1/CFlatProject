using System.Collections.Generic ;
using Chiyoda.CAD.Model ;

namespace Importer.BlockPattern.Equipment.EndTopTypePump
{
  public class SingleBlockPatternIndexInfoWithMinimumFlow : SingleBlockPatternIndexInfo
  {
    public int MinimumFlowIndex { get ; set ; }

    public enum MinimumFlowIndexType
    {
      NextOfMinimumFlowEnd,
      MinimumFlowEnd,
      MinimumFlowHeaderBOPSpacer,
      NextOfMinimumFlowHeaderBOPSpacer,
      MinimumFlowMinLength1,
      MinimumFlowMinLength2,
      MinimumFlowInletReducer,
    }

    public enum SuctionAdditionalIndexType{
      SuctionEndEdge,

    }

    public enum DischargeAdditionalIndexType{
      
      DischargeHeaderBOPSpacer,
      NextOfDischargeHeaderBOPSpacer,
      NextOfDischargeOriFlowPath,
      DischargeOriFlowDownTube,
      DischargeOriFlowHeaderBOPGoal,        //  OriFlowHeaderBOP 調整用 Goal(Elbow)
    }
    public double OriFlowHeaderBOPSpacerRatioForSpacer1;  //   

    public Dictionary<SuctionAdditionalIndexType, int> SuctionAdditionalIndexTypeValue = new Dictionary<SuctionAdditionalIndexType, int>() ;
    public Dictionary<DischargeAdditionalIndexType, int> DischargeAdditionalIndexTypeValue = new Dictionary<DischargeAdditionalIndexType, int>() ;
    public Dictionary<MinimumFlowIndexType, int> MinimumFlowIndexTypeValue = new Dictionary<MinimumFlowIndexType, int>() ;

    public int[,]  MinimumFlowPipeIndexRange{ get; set; }
    
    //! MinimumFlowFlex
    public List<int> MinimumFlowFlexIndexList { get ; set ; }

    //! OriFlowInOutPipes
    public int[] OriFlowInOutPipeIndexList;

    //! Indices for MinimumFlow pipes.
    public int[] MinimumFlowNormalPipes;
    public int[] MinimumFlowOletPrePostPipes;

    //! Diameter
    public float MinimumFlowDiameterNPSInch { get ; set ; }

  }
}
