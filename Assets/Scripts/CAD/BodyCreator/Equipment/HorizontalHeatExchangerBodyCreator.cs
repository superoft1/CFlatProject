using System;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class HorizontalHeatExchangerBodyCreator : BodyCreator<HorizontalHeatExchanger, HorizontalHeatExchangerBody>
  {
    public HorizontalHeatExchangerBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( HorizontalHeatExchangerBody body, HorizontalHeatExchanger he )
    {
      //SetupMaterial( body.DrumBody, body, he );
      //SetupMaterial( body.TubeBody, body, he );
      //SetupMaterial( body.CapBody, body, he );
      //SetupMaterial( body.SaddleBody1, body, he );
      //SetupMaterial( body.SaddleBody2, body, he );
      //SetupMaterial( body.CapDrumFlange1, body, he );
      //SetupMaterial( body.CapDrumFlange2, body, he );
      //SetupMaterial( body.DrumTubeFlange1, body, he );
      //SetupMaterial( body.DrumTubeFlange2, body, he );
      //SetupMaterial( body.TubeTermFlange1, body, he );
      //SetupMaterial( body.TubeTermFlange2, body, he );
    }
    
    private void SetupMaterial( GameObject go, HorizontalHeatExchangerBody body, HorizontalHeatExchanger he )
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial( body, he );
    }

    protected override void SetupGeometry(HorizontalHeatExchangerBody body, HorizontalHeatExchanger he)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;
    }

    void CreateDrum(GameObject top, HorizontalHeatExchanger he, HorizontalHeatExchangerBody body) {
      //var drumBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      //drumBody.transform.parent = top.transform;
      //drumBody.transform.localPosition = new Vector3(0.0f, (float)(0.5 * he.LengthOfTube), (float)(he.HeightOfSaddle + 0.5 * he.DiameterOfTube));
      //drumBody.transform.localRotation = Quaternion.identity;
      //drumBody.transform.localScale = new Vector3((float)he.DiameterOfTube, (float)(0.5 * he.LengthOfTube), (float)he.DiameterOfTube);
      //body.DrumBody = drumBody;
    }

    void CreateTube(GameObject top, HorizontalHeatExchanger he, HorizontalHeatExchangerBody body)
    {
      //var tubeBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      //tubeBody.transform.parent = top.transform;
      //tubeBody.transform.localPosition = new Vector3(0.0f, (float)(he.LengthOfTube + 0.5 * he.LengthOfShell), (float)(he.HeightOfSaddle + 0.5 * he.DiameterOfTube));
      //tubeBody.transform.localRotation = Quaternion.identity;
      //tubeBody.transform.localScale = new Vector3((float)he.DiameterOfTube, (float)(0.5 * he.LengthOfShell), (float)he.DiameterOfTube);
      //body.TubeBody = tubeBody;
    }

    void CreateCap(GameObject top, HorizontalHeatExchanger he, HorizontalHeatExchangerBody body)
    {
      //var capBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      //capBody.transform.parent = top.transform;
      //capBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(he.HeightOfSaddle + 0.5 * he.DiameterOfTube));
      //capBody.transform.localRotation = Quaternion.identity;
      //capBody.transform.localScale = new Vector3((float)he.DiameterOfTube, (float)(2.0 * he.LengthOfCap), (float)he.DiameterOfTube);
      //body.CapBody = capBody;
    }

    void CreateSaddles(GameObject top, HorizontalHeatExchanger he, HorizontalHeatExchangerBody body)
    {
      //var saddleBody1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      //saddleBody1.transform.parent = top.transform;
      //saddleBody1.transform.localPosition = new Vector3(0.0f, (float)he.DistanceOf1stSaddle, (float)he.HeightOfSaddle);
      //saddleBody1.transform.localRotation = Quaternion.identity;
      //saddleBody1.transform.localScale = new Vector3((float)he.LengthOfSaddle, (float)he.WidthOfSaddle, (float)(2.0 * he.HeightOfSaddle));
      //body.SaddleBody1 = saddleBody1;

      //var saddleBody2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      //saddleBody2.transform.parent = top.transform;
      //saddleBody2.transform.localPosition = new Vector3(0.0f, (float)he.DistanceOf2ndSaddle, (float)he.HeightOfSaddle);
      //saddleBody2.transform.localRotation = Quaternion.identity;
      //saddleBody2.transform.localScale = new Vector3((float)he.LengthOfSaddle, (float)he.WidthOfSaddle, (float)(2.0 * he.HeightOfSaddle));
      //body.SaddleBody2 = saddleBody2;
    }

    void CreateFlanges(GameObject top, HorizontalHeatExchanger he, HorizontalHeatExchangerBody body)
    {
      //var capDrumFlange1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      //capDrumFlange1.transform.parent = top.transform;
      //capDrumFlange1.transform.localPosition = new Vector3(0.0f, (float)(-0.5 * he.FlangeThickness), (float)(he.HeightOfSaddle + 0.5 * he.DiameterOfTube));
      //capDrumFlange1.transform.localRotation = Quaternion.identity;
      //capDrumFlange1.transform.localScale = new Vector3((float)he.DiameterOfFlange, (float)(0.5 * he.FlangeThickness), (float)he.DiameterOfFlange);
      //body.CapDrumFlange1 = capDrumFlange1;

      //var capDrumFlange2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      //capDrumFlange2.transform.parent = top.transform;
      //capDrumFlange2.transform.localPosition = new Vector3(0.0f, (float)(0.5 * he.FlangeThickness), (float)(he.HeightOfSaddle + 0.5 * he.DiameterOfTube));
      //capDrumFlange2.transform.localRotation = Quaternion.identity;
      //capDrumFlange2.transform.localScale = new Vector3((float)he.DiameterOfFlange, (float)(0.5 * he.FlangeThickness), (float)he.DiameterOfFlange);
      //body.CapDrumFlange2 = capDrumFlange2;


      //var drumTubeFlange1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      //drumTubeFlange1.transform.parent = top.transform;
      //drumTubeFlange1.transform.localPosition = new Vector3(0.0f, (float)(he.LengthOfTube - 0.5 * he.FlangeThickness), (float)(he.HeightOfSaddle + 0.5 * he.DiameterOfTube));
      //drumTubeFlange1.transform.localRotation = Quaternion.identity;
      //drumTubeFlange1.transform.localScale = new Vector3((float)he.DiameterOfFlange, (float)(0.5 * he.FlangeThickness), (float)he.DiameterOfFlange);
      //body.DrumTubeFlange1 = drumTubeFlange1;

      //var drumTubeFlange2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      //drumTubeFlange2.transform.parent = top.transform;
      //drumTubeFlange2.transform.localPosition = new Vector3(0.0f, (float)(he.LengthOfTube + 0.5 * he.FlangeThickness), (float)(he.HeightOfSaddle + 0.5 * he.DiameterOfTube));
      //drumTubeFlange2.transform.localRotation = Quaternion.identity;
      //drumTubeFlange2.transform.localScale = new Vector3((float)he.DiameterOfFlange, (float)(0.5 * he.FlangeThickness), (float)he.DiameterOfFlange);
      //body.DrumTubeFlange2 = drumTubeFlange2;


      //var tubeTermFlange1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      //tubeTermFlange1.transform.parent = top.transform;
      //tubeTermFlange1.transform.localPosition = new Vector3(0.0f, (float)(he.LengthOfTube + he.LengthOfShell - 0.5 * he.FlangeThickness), (float)(he.HeightOfSaddle + 0.5 * he.DiameterOfTube));
      //tubeTermFlange1.transform.localRotation = Quaternion.identity;
      //tubeTermFlange1.transform.localScale = new Vector3((float)he.DiameterOfFlange, (float)(0.5 * he.FlangeThickness), (float)he.DiameterOfFlange);
      //body.TubeTermFlange1 = tubeTermFlange1;

      //var tubeTermFlange2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      //tubeTermFlange2.transform.parent = top.transform;
      //tubeTermFlange2.transform.localPosition = new Vector3(0.0f, (float)(he.LengthOfTube + he.LengthOfShell + 0.5 * he.FlangeThickness), (float)(he.HeightOfSaddle + 0.5 * he.DiameterOfTube));
      //tubeTermFlange2.transform.localRotation = Quaternion.identity;
      //tubeTermFlange2.transform.localScale = new Vector3((float)he.DiameterOfFlange, (float)(0.5 * he.FlangeThickness), (float)he.DiameterOfFlange);
      //body.TubeTermFlange2 = tubeTermFlange2;
    }
  }
}
