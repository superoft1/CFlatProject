using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class HorizontalVesselBodyCreator : BodyCreator<HorizontalVessel, HorizontalVesselBody>
  {
    public HorizontalVesselBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( HorizontalVesselBody body, HorizontalVessel vessel )
    {
      SetupMaterial( body.DrumBody, body, vessel );
      SetupMaterial( body.SaddleBody1, body, vessel );
      SetupMaterial( body.SaddleBody2, body, vessel );
      SetupMaterial( body.CapBody1, body, vessel );
      SetupMaterial( body.CapBody2, body, vessel );
    }
    
    private void SetupMaterial( GameObject go, HorizontalVesselBody body, HorizontalVessel vessel )
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial( body, vessel );
    }
    
    protected override void SetupGeometry(HorizontalVesselBody body, HorizontalVessel vessel)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateDrum(go, vessel, body);
      CreateSaddles(go, vessel, body);
      CreateCaps(go, vessel, body);
    }

    void CreateDrum(GameObject top, HorizontalVessel vessel, HorizontalVesselBody body)
    {
      var drumBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      drumBody.transform.parent = top.transform;
      drumBody.transform.localPosition = new Vector3(0.0f, (float)(0.5 * vessel.LengthOfDrum), (float)(vessel.HeightOfSaddle + 0.5 * vessel.DiameterOfDrum));
      drumBody.transform.localRotation = Quaternion.identity;
      drumBody.transform.localScale = new Vector3((float)vessel.DiameterOfDrum, (float)(0.5 * vessel.LengthOfDrum), (float)vessel.DiameterOfDrum);
      body.DrumBody = drumBody;
    }

    void CreateSaddles(GameObject top, HorizontalVessel vessel, HorizontalVesselBody body)
    {
      var saddleBody1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      saddleBody1.transform.parent = top.transform;
      saddleBody1.transform.localPosition = new Vector3(0.0f, (float)(vessel.LengthOfDrum - vessel.DistanceOfSaddle), (float)vessel.HeightOfSaddle);
      saddleBody1.transform.localRotation = Quaternion.identity;
      saddleBody1.transform.localScale = new Vector3((float)vessel.LengthOfSaddle, (float)vessel.WidthOfSaddle, (float)(2.0 * vessel.HeightOfSaddle));
      body.SaddleBody1 = saddleBody1;

      var saddleBody2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      saddleBody2.transform.parent = top.transform;
      saddleBody2.transform.localPosition = new Vector3(0.0f, (float)(vessel.DistanceOfSaddle), (float)vessel.HeightOfSaddle);
      saddleBody2.transform.localRotation = Quaternion.identity;
      saddleBody2.transform.localScale = new Vector3((float)vessel.LengthOfSaddle, (float)vessel.WidthOfSaddle, (float)(2.0 * vessel.HeightOfSaddle));
      body.SaddleBody2 = saddleBody2;
    }

    void CreateCaps(GameObject top, HorizontalVessel vessel, HorizontalVesselBody body)
    {
      var capBody1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      capBody1.transform.parent = top.transform;
      capBody1.transform.localPosition = new Vector3(0.0f, (float)(vessel.LengthOfDrum), (float)(vessel.HeightOfSaddle + 0.5 * vessel.DiameterOfDrum));
      capBody1.transform.localRotation = Quaternion.identity;
      capBody1.transform.localScale = new Vector3((float)vessel.DiameterOfDrum, (float)(2.0 * vessel.LengthOfCaps), (float)vessel.DiameterOfDrum);
      body.CapBody1 = capBody1;

      var capBody2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      capBody2.transform.parent = top.transform;
      capBody2.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(vessel.HeightOfSaddle + 0.5 * vessel.DiameterOfDrum));
      capBody2.transform.localRotation = Quaternion.identity;
      capBody2.transform.localScale = new Vector3((float)vessel.DiameterOfDrum, (float)(2.0 * vessel.LengthOfCaps), (float)vessel.DiameterOfDrum);
      body.CapBody2 = capBody2;
    }
  }
}
