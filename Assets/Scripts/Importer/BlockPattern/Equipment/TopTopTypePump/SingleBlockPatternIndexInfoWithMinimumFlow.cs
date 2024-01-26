using System.Collections.Generic ;
using Chiyoda.CAD.Model ;

namespace Importer.BlockPattern.Equipment.TopTopTypePump
{
  public class SingleBlockPatternIndexInfoWithMinimumFlow : SingleBlockPatternIndexInfo
  {
    public int MinimumFlowIndex { get ; set ; }
    public int MinimumFlowPreIndex { get ; set ; }

        //! DischargeDetail
    public List<int> MinimumFlowAngleGroupIndexList { get ; set ; }

    public enum MinimumFlowIndexType
    {
      NextOfMinimumFlowEnd,
      MinimumFlowEnd,
      MinimumFlowHeaderBOPSpacer,
      NextOfMinimumFlowHeaderBOPSpacer,
      MinimumFlowMinLength1,
      MinimumFlowMinLength2,
      MinimumFlowSystemFlexStart,
      MinimumFlowSystemFlexStop,
      MinimumFlowSystemFlexOrigin,
      MinimumFlowOletPre1,  //  MinimumFlow 後段 側Olet 用Index
      MinimumFlowOletPost1, //
      MinimumFlowOletPre2,  //
      MinimumFlowOletPost2, //
    }

    public enum MinimumFlowPreIndexType
    {
      MinimumFlowPreOletPre1,  //  MinimumFlow 前段 側Olet 用Index
      MinimumFlowPreOletPost1, //
      MinimumFlowPreOletPre2,  //
      MinimumFlowPreOletPost2, //
    }

    public enum SuctionAdditionalIndexType{
      SuctionEndEdge,
    }

    public enum DischargeAdditionalIndexType{
      //DischargeHeaderBOPSpacer,
      //NextOfDischargeHeaderBOPSpacer,
      DischargeNozzleReducer,
      LowerDischargeBOP, 
      DischargeBOPSpacer,
      NextOfDischargeBOPSpacer,
      MinimumFlowStageBOPSpacer,
      NextOfMinimumFlowStageBOPSpacer,
      PreDischargeOriFlowAdjuster,
      DischargeOriFlowAdjuster,
      PostDischargeOriFlowAdjuster,
      DischargeMinimumFlowSpacer,
      MinimumFlowBranchTee,
      MinimumFlowSpacingOrigin,   //  elbow
      MinimumFlowAdjustingStart,
      MinimumFlowAdjustingStop,
      MinimumFlowAdjustingOrigin,
    }

    public Dictionary<SuctionAdditionalIndexType, int> SuctionAdditionalIndexTypeValue = new Dictionary<SuctionAdditionalIndexType, int>() ;
    public Dictionary<DischargeAdditionalIndexType, int> DischargeAdditionalIndexTypeValue = new Dictionary<DischargeAdditionalIndexType, int>() ;
    public Dictionary<MinimumFlowIndexType, int> MinimumFlowIndexTypeValue = new Dictionary<MinimumFlowIndexType, int>() ;
    public Dictionary<MinimumFlowPreIndexType, int> MinimumFlowPreIndexTypeValue = new Dictionary<MinimumFlowPreIndexType, int>() ;
    
    public int[,]  MinimumFlowInDischargeGroupPipeIndexRange{ get; set; }

    public int[,]  MinimumFlowPrePipeIndexRange{ get; set; }

    public int[,]  MinimumFlowPipeIndexRange{ get; set; }

    //! MinimumFlowFlex
    public List<int> MinimumFlowFlexIndexList { get ; set ; }

    //! Indices for MinimumFlow pipes.
    public int[] MinimumFlowInDischargeGroupNormalPipes;
    public int[] MinimumFlowInDischargeGroupOletPrePostPipes;

    //! Indices for MinimumFlow pipes.
    public int[] MinimumFlowPreNormalPipes;
    public int[] MinimumFlowPreOletPrePostPipes;

    //! Indices for MinimumFlow pipes.
    public int[] MinimumFlowNormalPipes;
    public int[] MinimumFlowOletPrePostPipes;

    //! Indices for MinimumFlowTeeAdjustSystem
    public int[] MinimumFlowTeeAdjusterIndices;
    //! Diameter
    public float MinimumFlowDiameterNPSInch { get ; set ; }

  }
}
