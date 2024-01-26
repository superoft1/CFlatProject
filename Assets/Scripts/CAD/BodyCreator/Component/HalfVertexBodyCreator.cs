using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

namespace Chiyoda.CAD.Body
{
  public class HalfVertexBodyCreator : BodyCreator<HalfVertex, Body>
  {
    public HalfVertexBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupMaterials(Body body, HalfVertex entity)
    {
      //マテリアルは暫定
      if (entity.CalculatedFlow == HalfVertex.FlowType.FromThisToAnother)
      {
        ChangeMaterialColor(body, UnityEngine.Color.blue);
      }
      else if(entity.CalculatedFlow == HalfVertex.FlowType.FromAnotherToThis)
      {
        ChangeMaterialColor(body, UnityEngine.Color.red);
      }
      else if(entity.CalculatedFlow == HalfVertex.FlowType.Undefined)
      {
        ChangeMaterialColor(body, UnityEngine.Color.gray);
      }
    }

    protected override void SetupGeometry( Body body, HalfVertex entity)
    {
      var go = body.gameObject;
      //値は暫定
      var adjustOffset = entity.LeafEdge.GlobalCod.GlobalizeVector(entity.GetConnectVector().normalized) * 0.1;

      go.transform.localScale = ScaledLocalScale();
      go.transform.localPosition = (Vector3)(entity.GlobalPoint + adjustOffset);
      go.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localRotation = Quaternion.identity;
      body.MainObject.transform.localPosition = Vector3.zero;
    }

    static public Vector3 ScaledLocalScale()
    {
      //値は暫定
      return new Vector3(0.0025f, 0.0025f, 0.0025f) * ModelScale;
    }
  }
}