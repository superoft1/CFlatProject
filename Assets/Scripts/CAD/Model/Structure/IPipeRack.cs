namespace Chiyoda.CAD.Model.Structure
{
  public interface IPipeRack : IStructure
  {
    bool IsHalfDownSideBeam { get ; set ; }
    
    void SetStandardBraces() ;
    
    (float h, float sideOffset) GetFloorHeight( int layer ) ;
    
    //　なくしたい
    double BeamHeight { get ; }
  } ;
}