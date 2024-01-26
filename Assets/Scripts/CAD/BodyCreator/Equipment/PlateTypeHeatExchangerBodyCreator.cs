using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class PlateTypeHeatExchangerBodyCreator : BodyCreator<PlateTypeHeatExchanger, PlateTypeHeatExchangerBody>
  {
    public PlateTypeHeatExchangerBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( PlateTypeHeatExchangerBody body, PlateTypeHeatExchanger heatExchanger )
    {
      body.Body.GetComponent<MeshRenderer>().material = GetMaterial( body, heatExchanger );
    }

    protected override void SetupGeometry(PlateTypeHeatExchangerBody body, PlateTypeHeatExchanger heatExchanger)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateBody(go, heatExchanger, body);
    }

    void CreateBody(GameObject top, PlateTypeHeatExchanger heatExchanger, PlateTypeHeatExchangerBody body)
    {
      var theBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      theBody.transform.parent = top.transform;
      theBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(0.5 * heatExchanger.Height));
      theBody.transform.localRotation = Quaternion.identity;
      theBody.transform.localScale = new Vector3((float)heatExchanger.Length, (float)heatExchanger.Width, (float)heatExchanger.Height);
      body.Body = theBody;
    }
  }
}
