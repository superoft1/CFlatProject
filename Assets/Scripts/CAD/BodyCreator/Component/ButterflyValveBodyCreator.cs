using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class ButterflyValveBodyCreator : BodyCreator<ButterflyValve, Body>
  {
    public ButterflyValveBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, ButterflyValve butterflyValve )
    {
      var go = body.gameObject;

      var diameterScale = (float)butterflyValve.Diameter * ModelScale;
      var heightScale = (float)butterflyValve.Length / 2f * ModelScale;
      go.transform.localScale = new Vector3( diameterScale, diameterScale, heightScale );
      go.transform.localPosition = (Vector3)butterflyValve.Origin;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;
    }
  }
}