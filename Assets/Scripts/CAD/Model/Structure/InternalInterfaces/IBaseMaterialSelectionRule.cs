namespace Chiyoda.CAD.Model.Structure
{
  internal interface IBaseMaterialSelectionRule
  {
    double CalcStandardBeamSize( double width ) ;
    double CalcStandardColumnSize( double beam ) ;
  }
}