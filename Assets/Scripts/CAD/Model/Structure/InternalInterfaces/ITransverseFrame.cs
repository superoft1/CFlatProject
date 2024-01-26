using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure
{
  internal interface ITransverseFrame : IWeightChangedEventEntity, IMemorableObject
  {
    ITransverseFrame CreateNextFrame() ;

    int FloorNumber { get ; }
    
    void ChangeFloorNumber( int value ) ;

    double Width { get ; set ; }
    
    double PositionOffset { get ; set ; }
    
    double HeightFromGround( int layer ) ;

    IFrameFloor this[int floor] { get ; }
    
    IEnumerable<IFrameFloor> Items { get ; }

    IEnumerable<IStructurePart> Elements { get ; }
    
    IEnumerable<Vector3> ColumnPositions { get ; }
  }
}