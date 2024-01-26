using System;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.Model.Routing;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Presenter
{
  partial class GameObjectPresenter
  {
    private class ThroughPointSubPresenter : SubPresenter<Point>
    {
      public ThroughPointSubPresenter(GameObjectPresenter basePresenter) : base(basePresenter) { }

      protected override bool IsRaised(Point element)
      {
        return BodyMap.ContainsBody(element);
      }

      protected override void Raise(Point element)
      { }

      protected override void Update(Point element)
      {
        IBody body;
        Body.Body oldPointObject, pointObject;
        if (false == BodyMap.TryGetBody(element, out body))
        {
          oldPointObject = null;
          pointObject = BodyFactory.CreateBody(element);
          BodyMap.Add(element, pointObject);
        }
        else
        {
          oldPointObject = body as Body.Body;
          pointObject = BodyFactory.UpdateBody( element, oldPointObject );
          if (pointObject != oldPointObject)
          {
            Destroy(element);
            BodyMap.Add(element, pointObject);
          }
        }

        Route route = element.Parent as Route;
        var routeObject = TopologyObjectMap.GetEdgeObject(route);
        var oldRouteObject = GetRouteEdgeObject(pointObject);
        if (oldRouteObject != routeObject)
        {
          if (null != pointObject)
          {
            var go = pointObject.gameObject;
            if (null != routeObject)
            {
              go.transform.parent = routeObject.transform;
            }
            else
            {
              go.transform.parent = null;
            }
          }
        }

        var mainTransform = pointObject.MainObject.transform ;
        mainTransform.localPosition = (Vector3) element.Origin ;
        mainTransform.localScale = Vector3.one ;
        if ( element is DirectionalPoint dp ) {
          mainTransform.localRotation = Quaternion.FromToRotation( Vector3.right, (Vector3)dp.Direction ) ;
        }
      }
      protected override void TransformUpdate( Point element )
      {
        if ( ! BodyMap.TryGetBody( element, out var body ) ) {
          throw new InvalidOperationException() ;
        }

        var mainTransform = ( body as Body.Body ).MainObject.transform ;
        
        mainTransform.localPosition = (Vector3)element.Origin;
        mainTransform.localScale = Vector3.one ;
        if ( element is DirectionalPoint dp ) {
          mainTransform.localRotation = Quaternion.FromToRotation( Vector3.right, (Vector3)dp.Direction ) ;
        }
      }

      protected override void Destroy(Point element)
      {
        IBody body;
        if (BodyMap.TryGetBody(element, out body))
        {
          BodyMap.Remove(element);
        }

        body.RemoveFromView();
      }

      private EdgeObject GetRouteEdgeObject(Body.Body body)
      {
        if (null == body) return null;

        Transform parent = body.transform.parent;
        if (null == parent) return null;

        return parent.GetComponent<EdgeObject>();
      }
    }
  }
}
