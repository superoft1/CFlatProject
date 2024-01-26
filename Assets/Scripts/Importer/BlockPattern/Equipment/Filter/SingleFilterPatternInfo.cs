using System.Collections.Generic;
using Chiyoda.CAD.Model;

namespace Importer.BlockPattern.Equipment.Filter
{
  public class SingleFilterPatternInfo : SingleBlockPatternIndexInfo
  {
    public enum DischargeAdditionalIndexType
    {
      DisPostNozzlePipe,
      DisFlangeA,
      DisPipeB,
      DisElbowA0Elbow,
      DisElbowA1Pipe,
      DisElbowA2Elbow,
      DisPipeC,
      DisPreValveAPipe,
      DisValveA0Flange,
      DisValveA1Valve,
      DisValveA2Flange,
      DisPostValveAPipe,
      DisPreValveBPipe,
      DisValveB0Flange,
      DisValveB1Valve,
      DisValveB2Flange,
      DisPreTeePipe,
      DisTee,
    }

    public Dictionary<DischargeAdditionalIndexType, int> DischargeAdditionalIndexTypeValue = new Dictionary<DischargeAdditionalIndexType, int>();

    public enum SuctionAdditionalIndexType
    {
      SucPostNozzlePipe,
      SucMainFlangeA,
      SucMainPipeB,
      SucPreMainValveAPipe,
      SucMainValveA0Flange,
      SucMainValveA1Valve,
      SucMainValveA2Flange,
      SucPostMainValveA,
      SucPreMainValveBPipe,
      SucMainValveB0Flange,
      SucMainValveB1Valve,
      SucMainValveB2Flange,
      SucMainPipeF,
      SucPreTeePipe,
      SucTee,
      SucBranchPipeA,
      SucBranchElbowA0Elbow,
      SucBranchElbowA1Pipe,
      SucBranchElbowA2Elbow,
      SucBypassPipeA,
      SucBypassPipeB,
      SucBypassPipeC,
      SucBypassValveA0Flange,
      SucBypassValveA1Valve,
      SucBypassValveA2Flange,
      SucBranchElbowB,
      SucBranchPipeB,
    }


    public Dictionary<SuctionAdditionalIndexType, int> SuctionAdditionalIndexTypeValue = new Dictionary<SuctionAdditionalIndexType, int>();
  }
}