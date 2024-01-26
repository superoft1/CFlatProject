﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class InstrumentAngleControlValveBodyCreator : BodyCreator<InstrumentAngleControlValve, Body>
  {
    public InstrumentAngleControlValveBodyCreator( Entity _entity ) : base( _entity )
    {}

    protected override void SetupGeometry( Body body, InstrumentAngleControlValve entity )
    {
      var go = body.gameObject;

      go.transform.localScale = new Vector3((float)entity.Length / 2f, (float)entity.Diameter, (float)entity.Diameter) * ModelScale;
      go.transform.localPosition = (Vector3)entity.Origin;
      go.transform.localRotation = Quaternion.identity;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;
    }
  }
}