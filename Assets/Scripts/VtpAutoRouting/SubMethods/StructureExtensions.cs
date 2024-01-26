using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Model.Structure ;
using UnityEngine ;

namespace VtpAutoRouting.SubMethods
{
  internal static class StructureExtensions
  {
    public static IStructure CreateDefaultRack( this Chiyoda.CAD.Core.Document doc, string name )
    {
      var rack = StructureFactory.CreatePipeRack( doc, PipeRackFrameType.Single ) ;
      rack.IsHalfDownSideBeam = true ;
      return rack ;
    }

    public static IStructure SetPosition( this IStructure rack, Vector3d position, double rotation )
    {
      rack.Position = position ;
      rack.Rotation = rotation ;
      return rack ;
    }

    public static IStructure SetFloors( this IStructure rack, params double [] floorsHeights )
    {
      rack.FloorCount = floorsHeights.Length ;
      for ( var i = 0 ; i < floorsHeights.Length ; ++i ) {
        var h = floorsHeights[ i ] ;
        rack.SetFloorHeight( i, h ) ;
        rack.SetSideBeamOffset( i, ( i == 0 ) ? 1.0 : 0.5 * h ) ;
      }
      return rack ;
    }
    
    public static IStructure SetDimensions(
      this IStructure rack, double beamInterval, int intervalNum, double width )
    {
      rack.IntervalCount = intervalNum ;
      rack.SetWidthAndStandardMaterials( width, beamInterval ) ;      
      return rack ;
    }
  }
}