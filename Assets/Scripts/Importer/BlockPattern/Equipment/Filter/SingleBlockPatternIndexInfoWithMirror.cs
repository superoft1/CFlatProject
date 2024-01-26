using System.Collections.Generic ;
using Chiyoda.CAD.Model ;

namespace Importer.BlockPattern.Equipment.Filter
{
  public class SingleBlockPatternIndexInfoWithMirror : SingleBlockPatternIndexInfo
  {

    public enum DischargeAdditionalIndexType
    {
      DischargeDistanceOrigin,
      DischargeOletPre1,
      DischargeOletPost1,
      DischargeOletPre2,
      DischargeOletPost2,
      DischargeOletPre3,
      DischargeOletPost3,

      DischargeSpacer1,
      DischargeSpacer2,

      PreDischargeBOPSpacer,
      DischargeBOPSpacer,
      PostDischargeBOPSpacer,

      DischargePreValveAFlange,   //  pump側
      DischargeValveA,            //  valve本体
      DischargePostValveAFlange,  //  joint側

      DischargePreValveBFlange,   //  pump側
      DischargeValveB,            //  valve本体
      DischargePostValveBFlange,  //  joint側

      DischargeJointDistanceGoal,
    }
    public Dictionary<DischargeAdditionalIndexType, int> DischargeAdditionalIndexTypeValue = new Dictionary<DischargeAdditionalIndexType, int>() ;

    public enum SuctionAdditionalIndexType
    {
      SuctionOletPre1,
      SuctionOletPost1,
      SuctionOletPre2,
      SuctionOletPost2,
      SuctionOletPre3,
      SuctionOletPost3,
      SuctionPreValveAFlange,   //  pump側
      SuctionValveA,            //  valve本体
      SuctionPostValveAFlange,  //  joint側

      SuctionPreValveBFlange,   //  pump側
      SuctionValveB,            //  valve本体
      SuctionPostValveBFlange,  //  joint側

      SuctionSpacer1,
      SuctionSpacer2,
      SuctionDistanceOrigin,
    }

    public Dictionary<SuctionAdditionalIndexType, int> SuctionAdditionalIndexTypeValue = new Dictionary<SuctionAdditionalIndexType, int>() ;

  }
}
