using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class KettleTypeHeatExchangerBodyCreator : BodyCreator<KettleTypeHeatExchanger, KettleTypeHeatExchangerBody>
  {
    public KettleTypeHeatExchangerBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( KettleTypeHeatExchangerBody body, KettleTypeHeatExchanger he )
    {
      SetupMaterial( body.DrumBody, body, he );
      SetupMaterial( body.TipBody, body, he );
      SetupMaterial( body.TaperBody, body, he );
      SetupMaterial( body.DrumCapBody, body, he );
      SetupMaterial( body.TipCapBody, body, he );
      SetupMaterial( body.SaddleBody1, body, he );
      SetupMaterial( body.SaddleBody2, body, he );
    }
    
    private void SetupMaterial( GameObject go, KettleTypeHeatExchangerBody body, KettleTypeHeatExchanger he )
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial( body, he );
    }

    protected override void SetupGeometry(KettleTypeHeatExchangerBody body, KettleTypeHeatExchanger he)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateDrum(go, he, body);
      CreateTip(go, he, body);
      CreateTaper(go, he, body);
      CreateDrumCap(go, he, body);
      CreateTipCap(go, he, body);
      CreateSaddles(go, he, body);
    }

    void CreateDrum(GameObject top, KettleTypeHeatExchanger he, KettleTypeHeatExchangerBody body)
    {
      var drumBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      drumBody.transform.parent = top.transform;
      drumBody.transform.localPosition = new Vector3(0.0f, (float)(0.5 * he.LengthOfTube), (float)(he.HeightOfSaddles + 0.5 * he.DiameterOfTube));
      drumBody.transform.localRotation = Quaternion.identity;
      drumBody.transform.localScale = new Vector3((float)he.DiameterOfTube, (float)(0.5 * he.LengthOfTube), (float)he.DiameterOfTube);
      body.DrumBody = drumBody;
    }

    void CreateTip(GameObject top, KettleTypeHeatExchanger he, KettleTypeHeatExchangerBody body)
    {
      var tipBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      tipBody.transform.parent = top.transform;
      tipBody.transform.localPosition = new Vector3(0.0f, (float)(he.LengthOfTube + he.LengthOfTaper + 0.5 * he.LengthOfTip), (float)(he.HeightOfSaddles + 0.5 * he.DiameterOfTube));
      tipBody.transform.localRotation = Quaternion.identity;
      tipBody.transform.localScale = new Vector3((float)he.DiameterOfTip, (float)(0.5 * he.LengthOfTip), (float)he.DiameterOfTip);
      body.TipBody = tipBody;
    }

    void CreateTaper(GameObject top, KettleTypeHeatExchanger he, KettleTypeHeatExchangerBody body)
    {
      var taperBody = GameObjectUtil.CreateTaperBody((float)(he.DiameterOfTube/2.0), (float)(he.DiameterOfTip/2.0), (float)he.LengthOfTaper);
      taperBody.transform.parent = top.transform;
      taperBody.transform.localPosition = new Vector3(0.0f, (float)he.LengthOfTube, (float)(he.HeightOfSaddles + 0.5 * he.DiameterOfTube));
      taperBody.transform.localRotation = Quaternion.identity;
      taperBody.transform.localScale = Vector3.one;
      body.TaperBody = taperBody;
    }

    void CreateDrumCap(GameObject top, KettleTypeHeatExchanger he, KettleTypeHeatExchangerBody body)
    {
      var capBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      capBody.transform.parent = top.transform;
      capBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(he.HeightOfSaddles + 0.5 * he.DiameterOfTube));
      capBody.transform.localRotation = Quaternion.identity;
      capBody.transform.localScale = new Vector3((float)he.DiameterOfTube, (float)(2.0 * he.LengthOfCaps), (float)he.DiameterOfTube);
      body.DrumCapBody = capBody;
    }

    void CreateTipCap(GameObject top, KettleTypeHeatExchanger he, KettleTypeHeatExchangerBody body)
    {
      var capBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      capBody.transform.parent = top.transform;
      capBody.transform.localPosition = new Vector3(0.0f, (float)(he.LengthOfTube + he.LengthOfTaper + he.LengthOfTip), (float)(he.HeightOfSaddles + 0.5 * he.DiameterOfTube));
      capBody.transform.localRotation = Quaternion.identity;
      capBody.transform.localScale = new Vector3((float)he.DiameterOfTip, (float)(2.0 * he.LengthOfCaps), (float)he.DiameterOfTip);
      body.TipCapBody = capBody;
    }

    void CreateSaddles(GameObject top, KettleTypeHeatExchanger he, KettleTypeHeatExchangerBody body)
    {
      var saddleBody1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      saddleBody1.transform.parent = top.transform;
      saddleBody1.transform.localPosition = new Vector3(0.0f, (float)he.DistanceOfSaddle, (float)he.HeightOfSaddles);
      saddleBody1.transform.localRotation = Quaternion.identity;
      saddleBody1.transform.localScale = new Vector3((float)he.LengthOfSaddles, (float)he.WidthOfSaddle, (float)(2.0 * he.HeightOfSaddles));
      body.SaddleBody1 = saddleBody1;

      var saddleBody2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      saddleBody2.transform.parent = top.transform;
      saddleBody2.transform.localPosition = new Vector3(0.0f, (float)(he.LengthOfTube - he.DistanceOfSaddle), (float)he.HeightOfSaddles);
      saddleBody2.transform.localRotation = Quaternion.identity;
      saddleBody2.transform.localScale = new Vector3((float)he.LengthOfSaddles, (float)he.WidthOfSaddle, (float)(2.0 * he.HeightOfSaddles));
      body.SaddleBody2 = saddleBody2;
    }
  }
}
