using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Electricals ;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class SubStationBodyCreator : BodyCreator<SubStation, SubStationBody>
  {
    public SubStationBodyCreator(Entity _entity) : base(_entity)
    {
    }
    protected override void SetupMaterials( SubStationBody body, SubStation entity )
    {
    }
    protected override void SetupGeometry( SubStationBody body, SubStation entity )
    {
      var transform = body.transform ;
      transform.position = (Vector3) entity.LocalCod.Origin ;
      transform.rotation = entity.LocalCod.Rotation ;

      if (body.MainBody == null) {
        body.MainBody = CreateBody( body.gameObject, entity ) ;
      }
      body.MainBody.transform.localScale = (Vector3)entity.Size;
    }

    GameObject CreateBody(GameObject parent, SubStation entity)
    {
      var theBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      theBody.transform.parent = parent.transform;
      theBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(entity.Size.z/2.0));
      theBody.transform.localRotation = Quaternion.identity;
      return theBody;
    }
  }
}