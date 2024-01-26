using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
  public class VenturiTubeBodyCreator : BodyCreator<VenturiTube, Body>
  {

    public VenturiTubeBodyCreator(Entity _entity) : base(_entity)
    { }

    protected override Body CreateGameObject()
    {
      var body = base.CreateGameObject();
      var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      body.MainObject = cyl;
      cyl.transform.parent = body.transform;
      return body;
    }

    protected override void SetupGeometry(Body body, VenturiTube entity)
    {
      var go = body.gameObject;

      var rate = 1.3f;
      go.transform.localScale = new Vector3((float)entity.Diameter * rate, (float)entity.Length / 2f, (float)entity.Diameter * rate);
      go.transform.position = (Vector3)entity.Origin;
      go.transform.up = (Vector3)entity.Direction;
    }
  }
}
