using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Electricals ;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class LocalPanelBodyCreator : BodyCreator<LocalPanel, LocalPanelBody>
  {
    public LocalPanelBodyCreator(Entity _entity) : base(_entity)
    {
    }
    protected override void SetupMaterials( LocalPanelBody body, LocalPanel entity )
    {
    }
    protected override void SetupGeometry( LocalPanelBody body, LocalPanel entity )
    {
      var transform = body.transform ;
      transform.position = (Vector3) entity.LocalCod.Origin ;
      transform.rotation = entity.LocalCod.Rotation ;

      if (body.PanelBody == null) {
        body.PanelBody = CreateBody( body.gameObject, entity ) ;
      }
      body.PanelBody.transform.localScale = (Vector3)entity.Size;
    }

    GameObject CreateBody(GameObject parent, LocalPanel entity)
    {
      var theBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      theBody.transform.parent = parent.transform;
      theBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(entity.LocalCod.Scale.z/2.0));
      theBody.transform.localRotation = Quaternion.identity;
      return theBody;
    }
  }
}