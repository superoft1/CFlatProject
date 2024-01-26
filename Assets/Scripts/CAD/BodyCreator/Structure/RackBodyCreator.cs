using System ;
using System.Collections.Generic ;
using System.Linq ;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Structure ;
using Chiyoda.CAD.Model.Structure.CommonEntities ;

namespace Chiyoda.CAD.Body
{
  public class StructureBodyCreator : BodyCreator<PlacementEntity, RackBody>
  {
    public StructureBodyCreator( Entity entity ) : base( entity ) {}

    protected override void SetupMaterials( RackBody body, PlacementEntity src )
    {
      foreach ( var go in body.Columns ) {
        go.GetComponent<MeshRenderer>().material = GetMaterial( body, src ) ;
      }
    }

    protected override void SetupGeometry( RackBody body, PlacementEntity src )
    {
      var go = body.gameObject ;
      if ( ! ( src is EmbodyStructure s ) ) {
        return ;
      }
      foreach ( var b in s.StructureElements ) {
        foreach ( var beamBody in CreateGameObjects( go, b ) ) {
          body.Columns.Add( beamBody ) ;
        }
      }
    }
    
    private static IEnumerable<GameObject> CreateGameObjects( GameObject parent, IStructurePart e )
    {
      switch ( e ) {
        case HSteel h :
          return CreateHSteel( parent, h ) ;
        case ShallowFoundations sh:
          return CreateShallowFoundation( parent, sh ) ; 
        default :
          return null ;
      }
    }

    private static IEnumerable<GameObject> CreateHSteel( GameObject top, HSteel steel ) 
    {
      var body = GameObject.CreatePrimitive( PrimitiveType.Cube ) ;
      body.transform.parent = top.transform ;
      body.transform.localScale = (Vector3) ( new Vector3d( steel.H, steel.B, steel.Length ) ) ;
      body.transform.localPosition = (Vector3) steel.LocalCod.Origin ;
      body.transform.localRotation = steel.LocalCod.Rotation ;
      yield return body ;
    }
    
    private static IEnumerable<GameObject> CreateShallowFoundation( GameObject top, ShallowFoundations foundation )
    {
      return foundation.LocalPositions
        .SelectMany( p => CreateShallowFoundation( top, foundation.Width, foundation.ColumnWidth, (Vector3)p ) ) ;
    }
    
    private static IEnumerable<GameObject> CreateShallowFoundation( GameObject top,
      double width, double columnWidth, Vector3 basePosition ) 
    {
      var w0 = (float) (columnWidth + 0.1) ;
      var w1 = (float) Math.Max( width, 0.5 * ( w0 + columnWidth ) ) ;
      
      var bodyUpper = GameObject.CreatePrimitive( PrimitiveType.Cube ) ;
      bodyUpper.transform.parent = top.transform ;
      bodyUpper.transform.position = basePosition + new Vector3( 0.0f, 0.0f, 0.1f ) ;
      bodyUpper.transform.localScale = new Vector3( w0, w0, 0.4f ) ;
      yield return bodyUpper ;

      var bodyLower = GameObject.CreatePrimitive( PrimitiveType.Cube ) ;
      bodyLower.transform.parent = top.transform ;
      bodyLower.transform.position = basePosition + new Vector3( 0.0f, 0.0f, -0.6f ) ;
      bodyLower.transform.localScale = new Vector3( w1, w1, 1.0f ) ;
      yield return bodyLower;
    }
  }
}