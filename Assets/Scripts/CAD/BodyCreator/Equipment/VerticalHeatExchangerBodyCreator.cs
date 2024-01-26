using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class VerticalHeatExchangerBodyCreator : BodyCreator<VerticalHeatExchanger, VerticalHeatExchangerBody>
  {
    public VerticalHeatExchangerBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( VerticalHeatExchangerBody body, VerticalHeatExchanger heatExchanger )
    {
      body.Body.GetComponent<MeshRenderer>().material = GetMaterial( body, heatExchanger );
    }

    protected override void SetupGeometry(VerticalHeatExchangerBody body, VerticalHeatExchanger heatExchanger)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateBody(go, heatExchanger, body);
    }

    void CreateBody(GameObject top, VerticalHeatExchanger heatExchanger, VerticalHeatExchangerBody body)
    {
      var theBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      theBody.transform.parent = top.transform;
      theBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(0.5 * heatExchanger.Height));
      theBody.transform.localRotation = Quaternion.LookRotation(Vector3.up);
      theBody.transform.localScale = new Vector3((float)heatExchanger.Diameter, (float)(0.5 * heatExchanger.Height), (float)heatExchanger.Diameter);
      body.Body = theBody;
    }
  }
}
