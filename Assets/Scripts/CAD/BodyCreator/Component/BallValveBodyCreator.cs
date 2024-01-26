using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class BallValveBodyCreator : BodyCreator<BallValve, Body>
  {
    public BallValveBodyCreator( Entity _entity ) : base( _entity )
    {}

    protected override void SetupGeometry( Body body, BallValve valve )
    {
      var go = body.gameObject;

      var diameterScale = (float)valve.Diameter * ModelScale;
      var heightScale = (float)valve.Length / 2f * ModelScale;
      go.transform.localScale = new Vector3( diameterScale, diameterScale, heightScale );
      go.transform.localPosition = (Vector3)valve.Origin;
      go.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;
    }
  }
}