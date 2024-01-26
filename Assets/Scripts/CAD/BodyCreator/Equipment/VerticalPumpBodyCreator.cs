using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class VerticalPumpBodyCreator : BodyCreator<VerticalPump, VerticalPumpBody>
  {
    public VerticalPumpBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( VerticalPumpBody body, VerticalPump heatExchanger )
    {
      body.Body.GetComponent<MeshRenderer>().material = GetMaterial( body, heatExchanger );
    }

    protected override void SetupGeometry(VerticalPumpBody body, VerticalPump heatExchanger)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateBody(go, heatExchanger, body);
    }

    void CreateBody(GameObject top, VerticalPump pump, VerticalPumpBody body)
    {
      var theBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      theBody.transform.parent = top.transform;
      theBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(0.5 * pump.Height));
      theBody.transform.localRotation = Quaternion.LookRotation(Vector3.up);
      theBody.transform.localScale = new Vector3((float)pump.Diameter, (float)(0.5 * pump.Height), (float)pump.Diameter);
      body.Body = theBody;
    }
  }
}
