using System.Collections.Generic ;
using Chiyoda.CAD.Model ;

namespace Importer.BlockPattern.Equipment.SideSideTypePump
{
  public class IndexRange{
    public int start;
    public int end;
  }
  public class SingleBlockPatternIndexInfo
  {
    public int SuctionIndex { get; set; }
    public int DischargeIndex { get; set; }
    public int BasePumpIndex { get; set; }

    //! DischargeDetail
    public List<int> DischargeAngleGroupIndexList { get; set; }

    public enum DischargeIndexType
    {
      DischargeBOP,
      DischargeEnd,
      DischargeNozzle,
      DischargeMinLength1,
      DischargeMinLength2,
      DischargeOletPre1,
      DischargeOletPost1,
      DischargeOletPre2,
      DischargeOletPost2,
      DischargeOletPre3,
      DischargeOletPost3,
      DischargeOletPre4,
      DischargeOletPost4,
      DischargeOletPre5,
      DischargeOletPost5,
      DischargeSystemFlexStart,   //  設定必須 Flex 連動の停止 Edge（IK Stop のように使用する。以降Pump側は影響を受けない）
      DischargeSystemFlexStop,    //  設定必須 Flex 連動の停止 Edge（IK Stop のように使用する。以降Pump側は影響を受けない）
      DischargeSystemFlexOrigin,  //  設定必須 Flex を連動させるDischarge 系のStopの一つPump側のLeafEdge（Endの座標算出基準可能な限りPipe 不可）
    }

    public Dictionary<DischargeIndexType, int> DischargeIndexTypeValue = new Dictionary<DischargeIndexType, int>();


    public enum SuctionIndexType
    {
      SuctionNozzle,
      SuctionEnd,
      SuctionMinLength1,
      SuctionMinLength2,
      SuctionOletPre1,  //  Suction 側Olet 用Index
      SuctionOletPost1, //
      SuctionOletPre2,  //
      SuctionOletPost2, //
      SuctionOletPre3,  //
      SuctionOletPost3, //
      SuctionOletPre4,  //
      SuctionOletPost4, //
      SuctionSystemFlexStart,    //  Flex 連動の停止 Edge（以降Pump側は影響を受けない）
      SuctionSystemFlexStop,    //  Flex 連動の停止 Edge（以降Pump側は影響を受けない）
      SuctionSystemFlexOrigin,  //  Flex を連動させるSuction 系のStopの一つPump側のLeafEdge（End の座標算出基準可能な限りPipe 不可）
    }

    public Dictionary<SuctionIndexType, int> SuctionIndexTypeValue = new Dictionary<SuctionIndexType, int>();

    public enum NextOfIndexType
    {
      NextOfDischargeEnd,
      NextOfSuctionEnd
    }

    public Dictionary<NextOfIndexType, int> NextOfIndexTypeValue = new Dictionary<NextOfIndexType, int>();

    //! IndexHelper
    public int[,]  SuctionFlexHelper{ get; set; }
    public int[,]  DischargeFlexHelper{ get; set; }
    public int[,]  SuctionPipeIndexRange{ get; set; }
    public int[,]  DischargePipeIndexRange{ get; set; }

    //! SuctionFlex
    public List<int> SuctionFlexIndexList { get ; set ; }

    //! DischargeFlex
    public List<int> DischargeFlexIndexList { get ; set ; }

    //! Diameter
    public float SuctionDiameterNPSInch { get ; set ; }
    public float DischargeDiameterNPSInch { get ; set ; }

    //! Indices for pipes
    public int[] DischargeNormalPipes;
    public int[] DischargeOletPrePostPipes;
    public int[] SuctionNormalPipes;
    public int[] SuctionOletPrePostPipes;

    //! Support
    public Dictionary<int, SupportType> SupportTypes { get ; set ; }

  }
}
