namespace Chiyoda.CAD.Model.Structure
{
  internal class LightCaseRule : IBaseMaterialSelectionRule
  {
    public double CalcStandardBeamSize( double width ) => width / 25.0 ;
    public double CalcStandardColumnSize( double beam ) => beam + 0.1 ;
  }
}