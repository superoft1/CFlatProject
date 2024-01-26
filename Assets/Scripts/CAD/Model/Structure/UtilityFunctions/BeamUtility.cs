using System.Collections.Generic;
using System.Linq ;
using Chiyoda.CAD.Model.Structure.Entities;
using UnityEngine;

namespace Chiyoda.CAD.Model.Structure
{
  internal static class BeamUtility
  {
    public static IStructurePart GetBeam( IStructuralMaterial material, double length, Vector3d p )
    {
      var elm = MaterialDataService.GetElement( material, length ) ;
      elm.LocalCod = new LocalCodSys3d( p,Quaternion.LookRotation( Vector3.right ), false ) ;
      return elm ;
    }

    public static IStructurePart GetSideBeam( IStructuralMaterial material, double length, Vector3d p )
    {
      var beam = MaterialDataService.GetElement( material, length ) ;
      beam.LocalCod = new LocalCodSys3d( p, Quaternion.LookRotation( Vector3.up, Vector3.left ), false ) ;
      return beam ;
    }

    public static IStructurePart GetSideBeam( IStructuralMaterial material, double length, double x, double height )
    {
      return GetSideBeam( material, length, new Vector3d( x, length * 0.5, height - 0.5 * material.MainSize ) ) ;
    }

    public static IEnumerable<IStructurePart> ShiftY<T>(
      this IEnumerable<T> src,
      System.Func<T, IEnumerable<IStructurePart>> positions,
      System.Func<T, double> step )
    {
      return Shift( src, positions, step, v => new Vector3d( 0.0, v, 0.0 ) ) ;
    }
    
    public static IEnumerable<IStructurePart> ShiftZ<T>(
      this IEnumerable<T> src,
      System.Func<T, IEnumerable<IStructurePart>> positions,
      System.Func<T, double> step )
    {
      return Shift( src, positions, step, v => new Vector3d( 0.0, 0.0, v ) ) ;
    }
    
    private static IEnumerable<IStructurePart> Shift(
      IEnumerable<IStructurePart> src, Vector3d dir )
    {
      foreach ( var b in src ) {
        b.LocalCod = b.LocalCod.Translate( dir ) ;
        yield return b ;
      }
    }

    private static IEnumerable<IStructurePart> Shift<T>(
      IEnumerable<T> src,
      System.Func<T, IEnumerable<IStructurePart>> positions,
      System.Func<T, double> step,
      System.Func<double, Vector3d> dir)
    {
      var offset = 0.0 ;
      foreach ( var t in src ) {
        var offsetVec = dir( offset ) ;
        foreach ( var b in Shift( positions( t ), offsetVec ) ) {
          yield return b ;
        }

        offset += step( t ) ;
      }
    }
  }
}