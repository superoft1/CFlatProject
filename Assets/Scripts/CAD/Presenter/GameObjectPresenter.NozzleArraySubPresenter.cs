using System;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Presenter
{
  partial class GameObjectPresenter
  {
    private class NozzleArraySubPresenter : SubPresenter<NozzleArray>
    {
      public NozzleArraySubPresenter(GameObjectPresenter basePresenter) : base(basePresenter) { }

      protected override bool IsRaised(NozzleArray nozzleArray)
      {
        return BodyMap.ContainsBody(nozzleArray);
      }

      protected override void Raise( NozzleArray nozzleArray )
      {
        var go = new GameObject() ;
        go.name = "NozzleArray" ;

        var body = go.AddComponent<Chiyoda.CAD.Body.Body>() ;
        BodyMap.Add( nozzleArray, body ) ;

        if ( nozzleArray is INozzlePlacement np ) {
          np.NozzlePositionChanged += NozzlePlacement_NozzlePositionChanged ;
        }
      }

      protected override void Update(NozzleArray nozzleArray)
      {
        if ( false == BodyMap.TryGetBody( nozzleArray, out var body ) ) return ;

        BodyMap.TryGetBody( nozzleArray.Parent as Entity, out var parentBody ) ;
        var transform = ( body as Body.Body ).transform ;
        transform.parent = (parentBody as Body.Body).transform;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero ;
      }

      protected override void TransformUpdate( NozzleArray nozzleArray )
      {
      }

      protected override void Destroy(NozzleArray nozzleArray)
      {
        if ( nozzleArray is INozzlePlacement np ) {
          np.NozzlePositionChanged -= NozzlePlacement_NozzlePositionChanged ;
        }

        if (BodyMap.TryGetBody(nozzleArray, out var body))
        {
          BodyMap.Remove(nozzleArray);
        }

        body.RemoveFromView();
      }

      private void NozzlePlacement_NozzlePositionChanged( object sender, EventArgs e )
      {
        var np = ( sender as INozzlePlacement ) ;
        foreach ( var nozzle in np.Nozzles ) {
          if ( ! BodyMap.TryGetBody( nozzle, out var body ) ) {
            continue ;
          }
          NozzleBodyCreator.SetTransform( body as Body.Body, nozzle ) ;
        }
      }
    }
  }
}