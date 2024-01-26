namespace Chiyoda.CAD.Model.Structure
{
  internal class RcRule : IBaseMaterialSelectionRule
  {
    public double CalcStandardBeamSize( double width ) => width / 12.0 ;

    public double CalcStandardColumnSize( double size ) => size + 0.1 ;
  }
}