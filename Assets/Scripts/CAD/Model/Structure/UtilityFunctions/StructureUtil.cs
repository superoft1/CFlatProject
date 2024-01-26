using System.Collections.Generic ;
using System.Linq ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure
{
  internal static class StructureUtil
  {
    public static LocalCodSys3d CalcNextConnectionCodSys( this LocalCodSys3d codSys, double length )
    {      
      var localDir = new Vector3( 0.0f, 1.0f, 0.0f ) ;
      //var offsetDir = codSys.Rotation * localDir ;
      return codSys.Translate( (float) length * localDir ) ;
    }
    
    public static double CalcSteelWeight( this ITransverseFrame frame )
    {
      return frame.Floors().Sum( f => CalcSteelWeight( frame.Width, f ) ) ;
    }

    public static double CalcSteelWeight( this IFrameConnections src )
    {
      return src.Connections().Sum( c => c.Weight ) ;
    }

    public static double RcVolume( this ITransverseFrame src )
    {
      return src.Floors().Sum( f => RcVolume( src.Width, f ) ) ;
    }

    public static double RcVolume( this IFrameConnections src )
    {
      var interval = src.BeamInterval ;
      return src.Connections().Sum( c =>
        interval * MaterialDataService.SectionArea( c.BeamMaterial ) ) ;
    }

    public static IEnumerable<IFrameConnection> Connections( this IFrameConnections src )
    {
      return Enumerable.Range( 0, src.FloorNumber )
        .Select( i => src[ i ] ) ;
    }

    public static IEnumerable<IFrameFloor> Floors( this ITransverseFrame src )
    {
      return Enumerable.Range( 0, src.FloorNumber ).Select( i => src[ i ] ) ;
    }

    private static double CalcSteelWeight( double width, IFrameFloor floor )
    {
      var beamWeight = MaterialDataService.SteelWeightPerLength( floor.BeamMaterial ) * width ;
      var columnWeight = 2.0 * MaterialDataService.SteelWeightPerLength( floor.ColumnMaterial ) * floor.Height ;
      return beamWeight + columnWeight ;
    }

    private static double RcVolume( double width, IFrameFloor floor )
    {
      var beamVolume = MaterialDataService.SectionArea( floor.BeamMaterial ) * width ;
      var columnVolume = 2.0 * MaterialDataService.SectionArea( floor.ColumnMaterial ) * width ;
      return beamVolume + columnVolume ;
    }

  }
}