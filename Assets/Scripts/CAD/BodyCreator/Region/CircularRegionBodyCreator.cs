using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Chiyoda.CAD.Body.Region
{
  public class CircularRegionBodyCreator: BodyCreator<CircularRegion, RegionBody>
  {
    public CircularRegionBodyCreator( Entity entity ) : base( entity )
    {
    }

    protected override RegionBody CreateGameObject()
    {
      var rootObject = new GameObject() ;
      var body = rootObject.AddComponent<RegionBody>() ;

      var plane = GameObject.CreatePrimitive( PrimitiveType.Quad ) ;
      body.MainObject = plane ;
      plane.transform.parent = body.transform ;
      return body ;
    }

    protected override Material GetMaterial( RegionBody body, CircularRegion region, bool isFoundation = false )
    {
      return BodyMaterialAccessor.Instance().GetRegionMaterial( body.RegionVisibility ) ;
    }

    protected override void SetupGeometry( RegionBody body, CircularRegion region )
    {
      var go = body.MainObject ;
      go.transform.localRotation = Quaternion.identity ;
      go.transform.localScale = new Vector3( (float) region.OuterRadius, (float) region.OuterRadius, 1 ) ;
    }
  }
}