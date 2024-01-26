using System.Collections.Generic ;
using Chiyoda.CAD.Model ;

namespace Importer.BlockPattern.Equipment.Filter
{
  public class SingleBlockPatternIndexInfo
  {
    public int SuctionIndex { get ; set ; }
    public int DischargeIndex { get ; set ; }
    public int BasePumpIndex { get ; set ; }

    //! DischargeDetail
    public List<int> DischargeAngleGroupIndexList { get ; set ; }

    public enum DischargeIndexType
    {
      DischargeBOP,
      DischargeEnd,
      DischargeMinLength1,
      DischargeMinLength2,
    }

    public Dictionary<DischargeIndexType, int> DischargeIndexTypeValue = new Dictionary<DischargeIndexType, int>() ;


    public enum SuctionIndexType
    {
      SuctionNozzle,
      SuctionEnd,
      SuctionMinLength1,
      SuctionMinLength2,
    }

    public Dictionary<SuctionIndexType, int> SuctionIndexTypeValue = new Dictionary<SuctionIndexType, int>() ;

    public enum NextOfIndexType
    {
      NextOfDischargeEnd,
      NextOfSuctionEnd
    }

    public Dictionary<NextOfIndexType, int> NextOfIndexTypeValue = new Dictionary<NextOfIndexType, int>() ;

    //! IndexHelper
    public int[,]  SuctionFlexHelper{ get; set; }
    public int[,]  DischargeFlexHelper{ get; set; }
    public int[,]  SuctionPipeIndexRange{ get; set; }
    public int[,]  DischargePipeIndexRange{ get; set; }
 
    //! Indices for pipes
    public int[] DischargeNormalPipes;
    public int[] DischargeOletPrePostPipes;
    public int[] SuctionNormalPipes;
    public int[] SuctionOletPrePostPipes;

    //! SuctionFlex
    public List<int> SuctionFlexIndexList { get ; set ; }

    //! DischargeFlex
    public List<int> DischargeFlexIndexList { get ; set ; }

    //! Diameter
    public float SuctionDiameterNPSInch { get ; set ; }
    public float DischargeDiameterNPSInch { get ; set ; }

    //! Support
    public Dictionary<int, SupportType> SupportTypes { get ; set ; }
  }
}
