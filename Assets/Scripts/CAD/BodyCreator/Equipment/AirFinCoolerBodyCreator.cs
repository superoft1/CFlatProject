using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class AirFinCoolerBodyCreator : BodyCreator<AirFinCooler, AirFinCoolerBody>
  {
    public AirFinCoolerBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( AirFinCoolerBody body, AirFinCooler heatExchanger )
    {
      body.MainBody.GetComponent<MeshRenderer>().material = GetMaterial( body, heatExchanger );
      body.UpperBody.GetComponent<MeshRenderer>().material = GetMaterial( body, heatExchanger );
      body.LowerBody.GetComponent<MeshRenderer>().material = GetMaterial( body, heatExchanger );
    }

    protected override void SetupGeometry(AirFinCoolerBody body, AirFinCooler heatExchanger)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateMainBody(go, heatExchanger, body);
      CreateUpperBody( go, heatExchanger, body ) ;
      CreateLowerBody( go, heatExchanger, body ) ;
    }

    void CreateMainBody(GameObject top, AirFinCooler cooler, AirFinCoolerBody body)
    {
      var theBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      theBody.transform.parent = top.transform;
      theBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(0.5 * cooler.HeightOfAirCooler + cooler.LowerHeightOfAirCooler));
      theBody.transform.localRotation = Quaternion.identity;
      theBody.transform.localScale = new Vector3((float)cooler.LengthOfAirCooler, (float)cooler.WidthOfAirCooler, (float)cooler.HeightOfAirCooler);
      body.MainBody = theBody;
    }
    
    void CreateUpperBody(GameObject top, AirFinCooler cooler, AirFinCoolerBody body)
    {
      var theBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      theBody.transform.parent = top.transform;
      theBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(0.5 * cooler.UpperHeightOfAirCooler + cooler.HeightOfAirCooler + cooler.LowerHeightOfAirCooler));
      theBody.transform.localRotation = Quaternion.identity;
      theBody.transform.localScale = new Vector3((float)cooler.UpperLengthOfAirCooler, (float)cooler.UpperWidthOfAirCooler, (float)cooler.UpperHeightOfAirCooler);
      body.UpperBody = theBody;
    }
    
    void CreateLowerBody(GameObject top, AirFinCooler cooler, AirFinCoolerBody body)
    {
      var theBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      theBody.transform.parent = top.transform;
      theBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(0.5 * cooler.LowerHeightOfAirCooler));
      theBody.transform.localRotation = Quaternion.identity;
      theBody.transform.localScale = new Vector3((float)cooler.LowerLengthOfAirCooler, (float)cooler.LowerWidthOfAirCooler, (float)cooler.LowerHeightOfAirCooler);
      body.LowerBody = theBody;
    }
  }
}
