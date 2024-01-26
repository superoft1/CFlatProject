using System ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Structure
{
  internal interface IFrameConnection : IRelocatable, IWeightChangedEventEntity
  {
    void SetReferenceFrame( ITransverseFrame referenceFrame, int floorNum ) ;
    void SetBottomConnection( IFrameConnection bottom ) ;
    
    IFrameConnection CreateCopy() ;

    double FloorHeight { get ; }
    double HeightOffset { get ; }

    bool HBrace { get ; }
    
    IStructuralMaterial BeamMaterial { get ; set ; }
    
    void SetHeightOffset( double value ) ;
    void SetHorizontalBrace( bool use ) ; 
    
    bool Brace { get ; set ; }
    
    double Weight { get ; }
  }
}