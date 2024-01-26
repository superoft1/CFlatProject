using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class SlipOnFlanngeBodyCreator : BodyCreator<SlipOnFlange, Body>
  {
    public SlipOnFlanngeBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, SlipOnFlange slipOnFlange )
    {
      var go = body.gameObject;
      go.transform.localScale = new Vector3( (float)slipOnFlange.Length / 2f, (float)slipOnFlange.Diameter, (float)slipOnFlange.Diameter) * ModelScale;
      go.transform.localPosition = (Vector3)slipOnFlange.Origin;
      go.transform.up = (Vector3)slipOnFlange.Axis;

      body.MainObject.transform.localRotation = Quaternion.identity;
      body.MainObject.transform.localPosition = Vector3.zero;
    }
  }
}