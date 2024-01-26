using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body.Region
{
  public class SquareRegionBodyCreator : BodyCreator<SquareRegion, RegionBody>
  {
    public SquareRegionBodyCreator( Entity entity ) : base( entity )
    {
    }

    protected override RegionBody CreateGameObject()
    {
      var rootObject = new GameObject() ;
      var body = rootObject.AddComponent<RegionBody>() ;

      var plane = GameObject.CreatePrimitive( PrimitiveType.Plane ) ;
      body.MainObject = plane ;
      plane.transform.parent = body.transform ;
      return body ;
    }

    protected override Material GetMaterial( RegionBody body, SquareRegion region, bool isFoundation = false )
    {
      return BodyMaterialAccessor.Instance().GetRegionMaterial( body.RegionVisibility ) ;
    }

    protected override void SetupGeometry( RegionBody body, SquareRegion region )
    {
      var go = body.MainObject ;
      go.transform.localRotation = Quaternion.identity ;
      go.transform.localScale = region.Size ;
    }
  }
}
