using System.Collections.Generic;
using Chiyoda.CAD.Model;

namespace Importer.BlockPattern.Equipment.PressureReliefValve
{
  public class SingleBlockPatternIndexInfo
  {
    public int InletIndex { get; set; }
    public int OutletIndex { get; set; }
    public int BasePumpIndex { get; set; }

    public enum InletIndexType
    {
      Inlet00Elbow,
      Inlet01Pipe,
      Inlet02Elbow,
      Inlet03Pipe,
      Inlet04Pipe,
      Inlet05Flange,
      Inlet06Valve,
      Inlet07Flange,
      Inlet08Pipe,
      Inlet15Pipe,
      Inlet16Elbow,
      Inlet17Pipe,
      Inlet18Reducer,
      Inlet19Flange,
    }

    public Dictionary<InletIndexType, int> InletIndexTypeValue = new Dictionary<InletIndexType, int>();

    public enum OutletIndexType
    {
      Outlet00Flange,
      Outlet01Reducer,
      Outlet02Reducer,
      Outlet03Pipe,
      Outlet10Pipe,
      Outlet11Elbow,
      Outlet12Pipe,
      Outlet13Flange,
      Outlet14Valve,
      Outlet15Flange,
      Outlet16Pipe,
      Outlet17Pipe,
      Outlet18Pipe,
      Outlet19Elbow,
    }

    public Dictionary<OutletIndexType, int> OutletIndexTypeValue = new Dictionary<OutletIndexType, int>();

    // InletFlex
    public List<int> InletFlexIndexList { get; set; }

    // OutletFlex
    public List<int> OutletFlexIndexList { get; set; }
  }
}