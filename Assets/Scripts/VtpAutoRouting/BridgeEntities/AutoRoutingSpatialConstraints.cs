using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Model.Routing ;
using UnityEngine;
using Routing;
using Routing.Model;

namespace VtpAutoRouting.BridgeEntities
{
  internal class AutoRoutingSpatialConstraints : IAutoRoutingSpatialConstraints
  {
    private readonly (IEndPoint, AEndPoint) _startPoint ;
    private readonly List<(IEndPoint, AEndPoint)> _endPoints ;

    public AutoRoutingSpatialConstraints( (IEndPoint, AEndPoint) fromPoint, IEnumerable<(IEndPoint, AEndPoint)> toPoints )
    {
      _startPoint = fromPoint ;
      _endPoints = toPoints.ToList() ;
    }

    public AEndPoint Start => _startPoint.Item2 ;
    
    public IEnumerable<AEndPoint> Destination => _endPoints.Select( pair => pair.Item2 ) ;

    public int DestinationCount => _endPoints.Count ;

    public void SyncModificationToSourceModel()
    {
      SyncResult( _startPoint );
      _endPoints.ForEach( SyncResult );
    }
    
    public Box3d Box3D
    {
      get
      {
        if ( _startPoint.Item1 != null || ! _endPoints.Any() ) {
          return new Box3d() ;
        }

        return new [] { _startPoint }.Concat( _endPoints )
          .Aggregate( new Box3d(), ( b, p ) => new Box3d( b, p.Item2.Position ) ) ;
      }
    }

    private static void SyncResult( (IEndPoint, AEndPoint) pair )
    {
      var (e, p) = pair ;
      e.SyncWith( new TermPointConstraints( p.Position, p.Direction ) );
    }
    

  }
}
