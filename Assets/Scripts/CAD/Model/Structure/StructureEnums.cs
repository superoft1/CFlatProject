namespace Chiyoda.CAD.Model.Structure
{
  public enum LengthUnit
  {
    Meter,
    MillMeter,
    Inch,
    Yard,
  }

  public enum MassUnit
  {
    Kilogram,
    Tonne,
    Pound,
    Ton, 
  }

  public enum CostUnit
  {
    USD,
    JPY,
    EUR,
  }
  
  public enum SteelSpecificationType
  {
    Unknown,
    ASTM,
    JIS,
    EURO,
    BS, //British Standard
  }
  
  internal enum SteelShapeType
  {
    Unknown,
    H,
    L,
    C,
    RB, // Filled Cylinder
    Pipe,
    T,
    FB,
  }
  
  internal enum MaterialRotation
  {
    Rot0,
    Rot90,
    Rot180,
    Rot270
  }
}