using System;
namespace Chiyoda.CAD.BP
{

  public class BlockPatternType
  {
    public enum Type
    {
      Unknown,
      EndTopTypePump,
      TopTopTypePump,
      SideSideTypePump,
      Column,
      SkirtTypeVessel,
      LegTypeVessel,
      SphericalTypeTank,
      ConeRoofTypeTank,
      HorizontalVessel,
      HorizontalHeatExchanger,
      KettleTypeHeatExchanger,
      VerticalHeatExchanger,
      PlateTypeHeatExchanger,
      AirFinCooler,
      VerticalPump,
      GenericEquipment,
      Chiller,
      Idf,
      Filter,
      PressureReliefValve,
      ControlValve,
      ActuatorControlValve
    }

    public static Type Parse(string typeStr)
    {
      foreach (Type val in Enum.GetValues(typeof(Type)))
      {
        if (val.ToString() == typeStr) return val;
      }
      return Type.Unknown;
    }
  }

}