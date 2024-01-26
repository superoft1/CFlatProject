using System ;
using System.Collections.Generic ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Structure
{
  internal interface IFrameConnections : IRelocatable
  {
    event EventHandler BeamIntervalChanged ;
    
    void Initialize( ITransverseFrame frame ) ;
    
    IFrameConnections ExpandConnection( ITransverseFrame frame ) ;
    
    IEnumerable<IFrameConnection> SyncConnectionsToRefFrame() ;

    int FloorNumber { get ; }    

    double BeamInterval { get ; set ; }

    void UseHalfDownSideBeam( bool use ) ;
    
    IFrameConnection this[int floor] { get ; }

    void SetHeight( int floor, double h ) ;
  }
}