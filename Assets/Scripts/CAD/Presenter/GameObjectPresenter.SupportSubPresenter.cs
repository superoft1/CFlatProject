using System;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.Manager;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Presenter
{
  partial class GameObjectPresenter
  {
    private class SupportSubPresenter : SubPresenter<Support>
    {
      public SupportSubPresenter( GameObjectPresenter basePresenter ) : base( basePresenter ) { }

      protected override bool IsRaised( Support support )
      {
        return BodyMap.ContainsBody( support );
      }

      protected override void Raise( Support support )
      { }

      protected override void Update( Support support )
      {
        IBody body;
        Body.Body oldSupportObject, supportObject;
        if ( false == BodyMap.TryGetBody( support, out body ) ) {
          oldSupportObject = null;
          supportObject = BodyFactory.CreateBody( support );
          BodyMap.Add( support, supportObject );
        }
        else {
          oldSupportObject = body as Body.Body;
          supportObject = BodyFactory.UpdateBody( support, oldSupportObject );
          if ( supportObject != oldSupportObject ) {
            Destroy( support );
            BodyMap.Add( support, supportObject );
          }
        }
        if ( null == supportObject ) {
          return;
        }

        var supportTransform = supportObject.transform;

        var edge = support.Parent as Edge;
        var edgeObject = TopologyObjectMap.GetEdgeObject( edge );
        var oldEdgeObject = GetEdgeObject( supportObject );
        if ( oldEdgeObject != edgeObject ) {
          if ( null != edgeObject ) {
            supportTransform.SetParent( edgeObject.transform ) ;
          }
          else {
            supportTransform.SetParent( RootGameObject.transform ) ;
          }
        }
        else if ( null == edgeObject && null == supportTransform.parent ) {
          supportTransform.SetParent( RootGameObject.transform );
        }

        supportTransform.localPosition = (Vector3)support.SupportOrigin;
        supportTransform.localRotation = Quaternion.identity;
        supportTransform.localScale = Vector3.one ;
      }

      protected override void TransformUpdate( Support support )
      {
        if ( false == BodyMap.TryGetBody( support, out var body ) ) {
          throw new InvalidOperationException() ;
        }

        var supportTransform = ( body as Body.Body ).transform ;
        supportTransform.localPosition = (Vector3)support.SupportOrigin;
        supportTransform.localRotation = Quaternion.identity;
        supportTransform.localScale = Vector3.one ;
      }

      protected override void Destroy( Support support )
      {
        IBody body;
        if ( BodyMap.TryGetBody( support, out body ) ) {
          BodyMap.Remove( support );
        }

        if ( null != body ) {
          body.RemoveFromView();
        }
      }

      private static EdgeObject GetEdgeObject( Body.Body body )
      {
        if ( null == body ) return null;

        Transform parent = body.transform.parent;
        if ( null == parent ) return null;

        return parent.GetComponent<EdgeObject>();
      }
    }
  }
}