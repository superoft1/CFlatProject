using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure
{
  public interface IStructure 
  {
    double Rotation { get ; set ; }
    Vector3d Position { get ; set ; }

    int IntervalCount { get ; set ; }
    int FloorCount { get ; set ; }

    double BeamInterval { get ; }
    double Width { get ; }
    void SetWidthAndStandardMaterials( double width, double beamInterval ) ;
    
    void SetFloorHeight( int layer, double value ) ;
    void SetSideBeamOffset( int layer, double value ) ;
  }
}
