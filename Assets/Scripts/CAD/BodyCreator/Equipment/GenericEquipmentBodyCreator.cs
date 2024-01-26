using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class GenericEquipmentBodyCreator : BodyCreator<GenericEquipment, GenericEquipmentBody>
  {
    public GenericEquipmentBodyCreator(Entity _entity) : base(_entity)
    {
    }
    
    protected override void SetupMaterials( GenericEquipmentBody body, GenericEquipment equipment )
    {
      body.Body.GetComponent<MeshRenderer>().material = GetMaterial( body, equipment );
    }

    protected override void SetupGeometry(GenericEquipmentBody body, GenericEquipment equipment)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateBody(go, equipment, body);
    }

    void CreateBody(GameObject top, GenericEquipment equipment, GenericEquipmentBody body)
    {
      var theBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      theBody.transform.parent = top.transform;
      theBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(0.5 * equipment.HeightOfEquipment));
      theBody.transform.localRotation = Quaternion.identity;
      theBody.transform.localScale = new Vector3((float)equipment.LengthOfEquipment, (float)equipment.WidthOfEquipment, (float)equipment.HeightOfEquipment);
      body.Body = theBody;
    }
  }
}
