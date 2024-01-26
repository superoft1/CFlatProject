using System ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal static class TransverseFrameFactory
  {
    public static ITransverseFrame Create( History h, Type type, double width )
    {
      var beamMaterial = MaterialDataService.Steel( SteelShapeType.H ).Beam( width ) ;
      if ( type == typeof( FrameConnectionsSingle ) ) {
        return new TransverseFrame<FrameFloor>( h, new FrameFloor( h, beamMaterial ), width ) ;
      }
      
      if( type == typeof( FrameConnections1Plus1 ) ) {
        return new TransverseFrame<FrameFloor>( h, new FrameFloor( h, beamMaterial ), width ) ;
      }

      return new TransverseFrame<FrameFloor>( h, new FrameFloor( h, beamMaterial ), width ) ;
    }
  }
}