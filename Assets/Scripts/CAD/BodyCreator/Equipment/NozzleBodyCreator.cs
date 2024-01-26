using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class NozzleBodyCreator : BodyCreator<Nozzle, Body> 
  {
    public NozzleBodyCreator(Entity _entity) : base(_entity)
    { }

    protected override void SetupGeometry(Body body, Nozzle entity)
    {
      body.MainObject.transform.localRotation = Quaternion.identity;
      SetTransform(body, entity);
    }

    public static void SetTransform(Body body, Nozzle entity)
    {
      var t = body.gameObject.transform;
      var parent = entity.Parent as INozzlePlacement;
      t.localScale = new Vector3((float)entity.Length * 0.5f, (float)entity.Diameter.OutsideMeter, (float)entity.Diameter.OutsideMeter) * ModelScale;
      var origin = parent.GetNozzleOriginPosition( entity ) ;
      var direction = parent.GetNozzleDirection( entity ) ;
      t.localPosition = (Vector3)(origin + direction * entity.Length * 0.5);
      t.transform.localRotation = Quaternion.FromToRotation(Vector3.right, (Vector3)direction);
    }

  }
}