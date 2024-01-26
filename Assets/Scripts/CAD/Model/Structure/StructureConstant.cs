namespace Chiyoda.CAD.Model.Structure
{
  public static class StructureConstant
  {
    public static double GravitationalAcceleration = 9.80655 ;
    
    public static CostUnit StructureCostUnit = CostUnit.USD ;
    
    public static MassUnit RcMassUnit = MassUnit.Kilogram ;
    public static LengthUnit RcLengthUnit = LengthUnit.Meter ;
    
    public static double RcWeightPerVolume = 2450.0 ;  // WeightUnit / lengthUnit^3 ;

    public static MassUnit SteelMassUnit = MassUnit.Kilogram ;
    public static LengthUnit SteelLengthUnit = LengthUnit.Meter ;
    
    public static double SteelPrice = 0.800 ; // CostUnit / WeightUnit  

    public static double RcPrice = 0.0 ; // 
    
    // PipeRack設定上限値
    public static int MaxInterval = 100 ;
    public static double MaxWidth = 100 ;
    public static int MaxFloorCount = 20 ;
    public static double MaxBeamInterval = 100 ;
  }
}