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
    private class PipingPieceSubPresenter : SubPresenter<PipingPiece>
    {
      public PipingPieceSubPresenter(GameObjectPresenter basePresenter) : base(basePresenter) { }

      protected override bool IsRaised(PipingPiece pipingPiece)
      {
        return BodyMap.ContainsBody(pipingPiece);
      }

      protected override void Raise(PipingPiece pipingPiece)
      { }

      protected override void Update(PipingPiece pipingPiece)
      {
        IBody body;
        Body.Body oldPipingPieceObject, pipingPieceObject;
        if (false == BodyMap.TryGetBody(pipingPiece, out body))
        {
          oldPipingPieceObject = null;
          pipingPieceObject = BodyFactory.CreateBody(pipingPiece);
          BodyMap.Add(pipingPiece, pipingPieceObject);
        }
        else
        {
          oldPipingPieceObject = body as Body.Body;
          pipingPieceObject = BodyFactory.UpdateBody( pipingPiece, oldPipingPieceObject );
          if (pipingPieceObject != oldPipingPieceObject)
          {
            Destroy(pipingPiece);
            BodyMap.Add(pipingPiece, pipingPieceObject);
          }
        }

        var leafEdge = pipingPiece.Parent as LeafEdge;
        var leafEdgeObject = TopologyObjectMap.GetEdgeObject(leafEdge);
        var oldLeafEdgeObject = GetLeafEdgeObject(pipingPieceObject);
        if (oldLeafEdgeObject != leafEdgeObject)
        {
          if (null != oldLeafEdgeObject)
          {
            oldLeafEdgeObject.PipingPiece = null;
          }

          if (null != pipingPieceObject)
          {
            var transform = pipingPieceObject.transform;
            if (null != leafEdgeObject) {
              transform.SetParent( leafEdgeObject.transform, false ) ;
            }
            else {
              transform.SetParent( null, false ) ;
            }

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            //if (pipingPieceObject.MainObject != null)
            //{
            //  pipingPieceObject.MainObject.transform.localPosition = Vector3.zero;
            //  pipingPieceObject.MainObject.transform.localRotation = Quaternion.identity;
            //}
          }

          if (null != leafEdgeObject)
          {
            leafEdgeObject.PipingPiece = pipingPieceObject;
          }
        }
      }

      protected override void TransformUpdate( PipingPiece pipingPiece )
      {
        // 不要
      }

      protected override void Destroy(PipingPiece pipingPiece)
      {
        IBody body;
        if (BodyMap.TryGetBody(pipingPiece, out body))
        {
          BodyMap.Remove(pipingPiece);
        }

        var oldLeafEdgeObject = GetLeafEdgeObject(body as Body.Body);
        if (null != oldLeafEdgeObject)
        {
          oldLeafEdgeObject.PipingPiece = null;
        }

        body.RemoveFromView();
      }

      private static EdgeObject GetLeafEdgeObject(Body.Body body)
      {
        if (null == body) return null;

        Transform parent = body.transform.parent;
        if (null == parent) return null;

        return parent.GetComponent<EdgeObject>();
      }
    }
  }
}