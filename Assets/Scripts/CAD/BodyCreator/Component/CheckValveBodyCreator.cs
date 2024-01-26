using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class CheckValveBodyCreator : BodyCreator<CheckValve, Body>
  {
    public CheckValveBodyCreator( Entity _entity ) : base( _entity ) { }

    protected override void SetupGeometry( Body body, CheckValve valve )
    {
      var go = body.gameObject;

      var diameterScale = (float)valve.Diameter * 1.2f;
      var heightScale = (float)valve.Length;
      go.transform.localScale = new Vector3( heightScale, diameterScale, diameterScale );
      go.transform.localPosition = (Vector3)valve.Origin;
      go.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity; // Quaternion.AngleAxis(90f, Vector3.right);
    }
  }

}