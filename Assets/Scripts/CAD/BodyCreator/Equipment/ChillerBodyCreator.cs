using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class ChillerBodyCreator : BodyCreator<Chiller, ChillerBody>
  {
    public ChillerBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( ChillerBody body, Chiller chiller )
    {
      SetupMaterial( body.TubeBody, body, chiller );
      SetupMaterial( body.CapBody1, body, chiller );
      SetupMaterial( body.CapBody2, body, chiller );
      SetupMaterial( body.TipCylinderBody1, body, chiller );
      SetupMaterial( body.TipCylinderBody2, body, chiller );
      SetupMaterial( body.TipTaperBody1, body, chiller );
      SetupMaterial( body.TipTaperBody2, body, chiller );
      SetupMaterial( body.SaddleBody1, body, chiller );
      SetupMaterial( body.SaddleBody2, body, chiller );
    }
    
    private void SetupMaterial( GameObject go, ChillerBody body, Chiller chiller )
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial( body, chiller );
    }
    
    protected override void SetupGeometry(ChillerBody body, Chiller chiller)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateTube(go, chiller, body);
      CreateCaps(go, chiller, body);
      CreateTipCylinders(go, chiller, body);
      CreateTipTapers(go, chiller, body);
      CreateSuddles(go, chiller, body);
    }

    void CreateTube(GameObject top, Chiller chiller, ChillerBody body)
    {
      var tubeBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      tubeBody.transform.parent = top.transform;
      tubeBody.transform.localPosition = new Vector3(0.0f, -(float)(0.5 * chiller.LengthOfTube), (float)(chiller.HeightOfSaddle + 0.5 * chiller.DiameterOfTube));
      tubeBody.transform.localRotation = Quaternion.identity;
      tubeBody.transform.localScale = new Vector3((float)chiller.DiameterOfTube, (float)(0.5 * chiller.LengthOfTube), (float)chiller.DiameterOfTube);
      body.TubeBody = tubeBody;
    }

    void CreateCaps(GameObject top, Chiller chiller, ChillerBody body)
    {
      var capBody1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      capBody1.transform.parent = top.transform;
      capBody1.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(chiller.HeightOfSaddle + 0.5 * chiller.DiameterOfTube));
      capBody1.transform.localRotation = Quaternion.identity;
      capBody1.transform.localScale = new Vector3((float)chiller.DiameterOfTube, (float)(2.0 * chiller.LengthOfCaps), (float)chiller.DiameterOfTube);
      body.CapBody1 = capBody1;

      var capBody2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      capBody2.transform.parent = top.transform;
      capBody2.transform.localPosition = new Vector3(0.0f, -(float)chiller.LengthOfTube, (float)(chiller.HeightOfSaddle + 0.5 * chiller.DiameterOfTube));
      capBody2.transform.localRotation = Quaternion.identity;
      capBody2.transform.localScale = new Vector3((float)chiller.DiameterOfTube, (float)(2.0 * chiller.LengthOfCaps), (float)chiller.DiameterOfTube);
      body.CapBody2 = capBody2;
    }

    void CreateTipCylinders(GameObject top, Chiller chiller, ChillerBody body)
    {
      // シリンダ部分の長さだけでモデリングすると隙間が空くためCap部分の長さを加味してモデリング
      var tipCylinderBody1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      tipCylinderBody1.transform.parent = top.transform;
      tipCylinderBody1.transform.localPosition = new Vector3(0.0f, (float)(0.5 * (chiller.LengthOfCaps + chiller.LengthOfTipCylinder)), (float)chiller.HeightOfTip);
      tipCylinderBody1.transform.localRotation = Quaternion.identity;
      tipCylinderBody1.transform.localScale = new Vector3((float)chiller.DiameterOfTip, (float)(0.5 * (chiller.LengthOfCaps + chiller.LengthOfTipCylinder)), (float)chiller.DiameterOfTip);
      body.TipCylinderBody1 = tipCylinderBody1;

      var tipCylinderBody2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      tipCylinderBody2.transform.parent = top.transform;
      tipCylinderBody2.transform.localPosition = new Vector3(0.0f, -(float)(chiller.LengthOfTube + 0.5 * (chiller.LengthOfCaps + chiller.LengthOfTipCylinder)), (float)chiller.HeightOfTip);
      tipCylinderBody2.transform.localRotation = Quaternion.identity;
      tipCylinderBody2.transform.localScale = new Vector3((float)chiller.DiameterOfTip, (float)(0.5 * (chiller.LengthOfCaps + chiller.LengthOfTipCylinder)), (float)chiller.DiameterOfTip);
      body.TipCylinderBody2 = tipCylinderBody2;
    }

    void CreateTipTapers(GameObject top, Chiller chiller, ChillerBody body)
    {
      var tipTaperBody1 = GameObjectUtil.CreateTaperBody((float)(0.5 * chiller.DiameterOfTip), (float)(0.5 * chiller.DiameterOfTerm), (float)chiller.LengthOfTaper, false, true);
      tipTaperBody1.transform.parent = top.transform;
      tipTaperBody1.transform.localPosition = new Vector3(0.0f, (float)(chiller.LengthOfCaps + chiller.LengthOfTipCylinder), (float)chiller.HeightOfTip);
      tipTaperBody1.transform.localRotation = Quaternion.identity;
      tipTaperBody1.transform.localScale = Vector3.one;
      body.TipTaperBody1 = tipTaperBody1;

      var tipTaperBody2 = GameObjectUtil.CreateTaperBody((float)(0.5 * chiller.DiameterOfTip), (float)(0.5 * chiller.DiameterOfTerm), (float)chiller.LengthOfTaper, false, true);
      tipTaperBody2.transform.parent = top.transform;
      tipTaperBody2.transform.localPosition = new Vector3(0.0f, -(float)(chiller.LengthOfTube + chiller.LengthOfCaps + chiller.LengthOfTipCylinder), (float)chiller.HeightOfTip);
      tipTaperBody2.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.down);
      tipTaperBody2.transform.localScale = Vector3.one;
      body.TipTaperBody2 = tipTaperBody2;
    }

    void CreateSuddles(GameObject top, Chiller chiller, ChillerBody body)
    {
      // Saddle部分だけでモデリングすると隙間が空くためTube半径分を加味してモデリング
      var saddleBody1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      saddleBody1.transform.parent = top.transform;
      saddleBody1.transform.localPosition = new Vector3(0.0f, -(float)chiller.DistanceOfSaddle, (float)(0.5 * (chiller.HeightOfSaddle + 0.5 * chiller.DiameterOfTube)));
      saddleBody1.transform.localRotation = Quaternion.identity;
      saddleBody1.transform.localScale = new Vector3((float)chiller.LengthOfSaddle, (float)chiller.WidthOfSaddle, (float)(chiller.HeightOfSaddle + 0.5 * chiller.DiameterOfTube));
      body.SaddleBody1 = saddleBody1;

      var saddleBody2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      saddleBody2.transform.parent = top.transform;
      saddleBody2.transform.localPosition = new Vector3(0.0f, -(float)(chiller.LengthOfTube - chiller.DistanceOfSaddle), (float)(0.5 * (chiller.HeightOfSaddle + 0.5 * chiller.DiameterOfTube)));
      saddleBody2.transform.localRotation = Quaternion.identity;
      saddleBody2.transform.localScale = new Vector3((float)chiller.LengthOfSaddle, (float)chiller.WidthOfSaddle, (float)(chiller.HeightOfSaddle + 0.5 * chiller.DiameterOfTube));
      body.SaddleBody2 = saddleBody2;
    }
  }
}
