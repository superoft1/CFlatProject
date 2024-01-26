namespace Chiyoda.CAD.Model.Structure.CommonEntities
{
  internal class PhysicalProperty
  {    
    public PhysicalProperty( double steelMass, double rcVolume )
    {
      SteelMass = steelMass ;
      RcVolume = rcVolume ;
    }
        
    public double SteelWeight => SteelMass * StructureConstant.GravitationalAcceleration ;
    
    public double SteelMass { get; }
    
    public double RcVolume { get; }
    
    public double Load => SteelWeight + StructureConstant.RcWeightPerVolume * RcVolume ;      
  }
}