using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class StubInReinforcingWeldBodyCreator : BodyCreator<StubInReinforcingWeld, Body>
  {
    public StubInReinforcingWeldBodyCreator( Entity _entity ) : base( _entity )
    { }

    protected override void SetupGeometry( Body body, StubInReinforcingWeld stubInReinforcingWeld )
    {
      var go = body.gameObject;

      go.transform.localScale = new Vector3((float)stubInReinforcingWeld.LengthFromPipeCenter / 2f, (float)stubInReinforcingWeld.Diameter, (float)stubInReinforcingWeld.Diameter) * ModelScale;
      go.transform.localPosition = (Vector3)stubInReinforcingWeld.Origin;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;
    }
  }
}