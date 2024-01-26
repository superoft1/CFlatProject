using System ;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Manager;
using Chiyoda.CAD.Model;
using UnityEngine ;

namespace Chiyoda.CAD.Presenter
{
  partial class GameObjectPresenter
  {
    private class RegionSubPresenter : SubPresenter<Region>
    {
      public RegionSubPresenter( GameObjectPresenter basePresenter ) : base( basePresenter )
      {
      }

      protected override bool IsRaised( Region region )
      {
        return BodyMap.ContainsBody( region ) ;
      }

      protected override void Raise( Region region )
      {
      }

      protected override void Update( Region region )
      {
        Body.Body oldRegionObject, regionObject ;
        if ( false == BodyMap.TryGetBody( region, out var body ) ) {
          oldRegionObject = null ;
          regionObject = BodyFactory.CreateBody( region ) ;
          BodyMap.Add( region, regionObject ) ;
        }
        else {
          oldRegionObject = body as Body.Body ;
          regionObject = BodyFactory.UpdateBody( region, oldRegionObject ) ;
          if ( regionObject != oldRegionObject ) {
            Destroy( region ) ;
            BodyMap.Add( region, regionObject ) ;
          }
        }

        var transform = regionObject.transform ;
        transform.parent = RootGameObject.transform ;
        transform.localPosition = region.Center ;
        transform.localRotation = Quaternion.identity ;
        transform.localScale = Vector3.one ;
      }

      protected override void TransformUpdate( Region region )
      {
        if ( ! BodyMap.TryGetBody( region, out var body ) ) {
          throw new InvalidOperationException() ;
        }

        var transform = ( body as Body.Body ).transform ;
        transform.localPosition = region.Center ;
        transform.localRotation = Quaternion.identity ;
        transform.localScale = Vector3.one ;
      }

      protected override void Destroy( Region region )
      {
        if ( BodyMap.TryGetBody( region, out var body ) ) {
          BodyMap.Remove( region ) ;
        }

        body.RemoveFromView() ;
      }
    }
  }
}