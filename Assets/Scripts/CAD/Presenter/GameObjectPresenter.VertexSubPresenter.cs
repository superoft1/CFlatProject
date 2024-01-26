using Chiyoda.CAD.Topology;
using Chiyoda.CAD.Body;
using UnityEngine;

namespace Chiyoda.CAD.Presenter
{
  partial class GameObjectPresenter
  {
    private class VertexSubPresenter : SubPresenter<HalfVertex>
    {
      public VertexSubPresenter( GameObjectPresenter basePresenter ) : base( basePresenter )
      {
      }

      protected override bool IsRaised( HalfVertex vertex)
      {
        return BodyMap.ContainsBody(vertex);
      }

      protected override void Raise(HalfVertex vertex)
      {
      }

      protected override void Update( HalfVertex vertex)
      {
        if(vertex.Partner == null)
        {
          var leafEdgeObject = (null != vertex.LeafEdge) ? TopologyObjectMap.GetEdgeObject(vertex.LeafEdge) : null;
          Body.Body halfVertexObject;
          if (false == BodyMap.TryGetBody(vertex, out var body))
          {
            halfVertexObject = BodyFactory.CreateBody(vertex);
            if ( leafEdgeObject != null ) halfVertexObject.transform.parent = leafEdgeObject.transform ;
            BodyMap.Add(vertex, halfVertexObject);
          }
          else
          {
            var oldHalfVertexObject = body as Body.Body;
            halfVertexObject = BodyFactory.UpdateBody(vertex, oldHalfVertexObject);
            if ( leafEdgeObject != null ) halfVertexObject.transform.parent = leafEdgeObject.transform ;
            if ( halfVertexObject == oldHalfVertexObject ) return ;
            Destroy(vertex);
            BodyMap.Add(vertex, halfVertexObject);
          }
        }
        else {
          Destroy(vertex);
          Destroy( vertex.Partner ) ;
        }
      }

      protected override void TransformUpdate( HalfVertex vertex )
      {
      }

      protected override void Destroy( HalfVertex vertex )
      {
        if (BodyMap.TryGetBody(vertex, out _))
        {
          BodyMap.Remove(vertex);
        }
      }

      public override void UpdateScale(Body.Body bodyObject, bool highlight)
      {
        //HalfVertexの倍率(値は暫定)
        var magnification = highlight ? 2.0f : 1.0f;
        bodyObject.transform.localScale = HalfVertexBodyCreator.ScaledLocalScale() * magnification;
      }
    }
  }
}