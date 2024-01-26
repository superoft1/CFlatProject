using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{

  public class ThreeWayInstrumentRootValveBodyCreator : BodyCreator<ThreeWayInstrumentRootvalve, Body>
  {
    public ThreeWayInstrumentRootValveBodyCreator(Entity _entity) : base(_entity)
    {

    }

    protected override void SetupGeometry(Body body, ThreeWayInstrumentRootvalve valve)
    {
      var go = body.gameObject;

      go.transform.localScale = Vector3.one;
      go.transform.localPosition = (Vector3)valve.Origin;

      body.MainObject.transform.localPosition = Vector3.zero;
      body.MainObject.transform.localRotation = Quaternion.identity;

      var impl = body.MainObject.GetComponent<ThreeWayInstrumentRootValveBodyImpl>();
      impl.MainValve1.transform.localScale = new Vector3((float)valve.AxisLength1, (float)valve.AxisDiameter, (float)valve.AxisDiameter) * ModelScale;
      impl.MainValve2.transform.localScale = new Vector3((float)valve.AxisLength2, (float)valve.AxisDiameter, (float)valve.AxisDiameter) * ModelScale;
      impl.ReferenceValve.transform.localScale = new Vector3((float)valve.ReferenceLength, (float)valve.ReferenceDiameter, (float)valve.ReferenceDiameter) * ModelScale;
    }
  }

}