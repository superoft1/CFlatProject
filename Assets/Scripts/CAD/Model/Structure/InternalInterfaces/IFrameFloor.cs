using System ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Structure
{
  internal interface IFrameFloor : IMemorableObject, IWeightChangedEventEntity
  {
    IFrameFloor CreateCopy() ;
    void CopyFrom( IFrameFloor src ) ;

    double Height { get ; set ; }

    IStructuralMaterial ColumnMaterial { get ; set ; }
    
    IStructuralMaterial BeamMaterial { get; set ; }
  }
}