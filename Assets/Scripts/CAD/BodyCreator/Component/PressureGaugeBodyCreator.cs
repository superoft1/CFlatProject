using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class PressureGaugeBodyCreator : BodyCreator<PressureGauge, Body>
  {
    public PressureGaugeBodyCreator( Entity _entity ) : base( _entity )
    {}

    protected override void SetupGeometry( Body body, PressureGauge entity )
    {
      var go = body.gameObject;

      var ratio = 1.3f ;//見た目を太くする
      go.transform.localScale = new Vector3((float)entity.Length / 2f, (float)entity.Diameter*ratio, (float)entity.Diameter*ratio) * ModelScale;
      go.transform.localPosition = (Vector3)entity.Origin;
      go.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;
    }
  }
}
