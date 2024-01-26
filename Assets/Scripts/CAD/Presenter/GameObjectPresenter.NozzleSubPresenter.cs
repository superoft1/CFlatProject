using System;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Presenter
{
  partial class GameObjectPresenter
  {
    private class NozzleSubPresenter : SubPresenter<Nozzle>
    {
      public NozzleSubPresenter(GameObjectPresenter basePresenter) : base(basePresenter) { }

      protected override bool IsRaised(Nozzle nozzle)
      {
        return BodyMap.ContainsBody(nozzle);
      }

      protected override void Raise(Nozzle nozzle)
      { }

      protected override void Update(Nozzle nozzle)
      {
        Body.Body oldNozzleObject, nozzleObject;
        if (false == BodyMap.TryGetBody(nozzle, out var body))
        {
          oldNozzleObject = null;
          nozzleObject = BodyFactory.CreateBody(nozzle);
          BodyMap.Add(nozzle, nozzleObject);
        }
        else
        {
          oldNozzleObject = body as Body.Body;
          nozzleObject = BodyFactory.UpdateBody( nozzle, oldNozzleObject );
          if (nozzleObject != oldNozzleObject)
          {
            Destroy(nozzle);
            BodyMap.Add(nozzle, nozzleObject);
          }
        }

        var parent = nozzle.Parent as INozzlePlacement;
        BodyMap.TryGetBody(parent as Entity, out var iBody);
        var parentObject = iBody as Body.Body;
        var oldParentObject = GetParentBody(nozzleObject);
        if (oldParentObject != parentObject)
          if (null != nozzleObject)
          {
            var go = nozzleObject.gameObject;
            if (null != parentObject)
            {
              go.transform.SetParent( parentObject.transform, false );
            }
            else
            {
              go.transform.SetParent( null, false );
            }
            NozzleBodyCreator.SetTransform(nozzleObject, nozzle);
          }
      }

      protected override void TransformUpdate( Nozzle nozzle )
      {
        if ( ! BodyMap.TryGetBody( nozzle, out var body ) ) {
          throw new InvalidOperationException() ;
        }

        NozzleBodyCreator.SetTransform( body as Body.Body, nozzle ) ;
      }

      protected override void Destroy(Nozzle nozzle)
      {
        IBody body;
        if (BodyMap.TryGetBody(nozzle, out body))
        {
          BodyMap.Remove(nozzle);
        }

        body.RemoveFromView();
      }

      private static Body.Body GetParentBody(Body.Body body)
      {
        if (null == body) return null;

        Transform parent = body.transform.parent;
        if (null == parent) return null;

        return parent.GetComponent<Body.Body>();
      }
    }
  }
}