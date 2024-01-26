using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class PipingElbow45BodyCreator : BodyCreator<PipingElbow45, Body>
  {
    public PipingElbow45BodyCreator( Entity _entity ) : base( _entity ) { }

    protected override void SetupGeometry( Body body, PipingElbow45 elbow )
    {
      var go = body.gameObject;

      go.transform.localScale = Vector3.one * (float)elbow.Diameter * ModelScale;
      go.transform.localPosition = (Vector3)elbow.Origin;
      go.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localRotation = Quaternion.identity;
      body.MainObject.transform.localPosition = Vector3.zero;
    }
  }

}