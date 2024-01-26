using System.Collections.Generic;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class LegTypeVesselBodyCreator : BodyCreator<LegTypeVessel, LegTypeVesselBody>
  {
    public LegTypeVesselBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( LegTypeVesselBody body, LegTypeVessel vessel )
    {
      SetupMaterial( body.CylinderBody, body, vessel );
      SetupMaterial( body.NorthLegBody, body, vessel );
      SetupMaterial( body.SouthLegBody, body, vessel );
      SetupMaterial( body.EastLegBody, body, vessel );
      SetupMaterial( body.WestLegBody, body, vessel );
      SetupMaterial( body.UpperCapBody, body, vessel );
      SetupMaterial( body.LowerCapBody, body, vessel );
    }
    
    private void SetupMaterial( GameObject go, LegTypeVesselBody body, LegTypeVessel vessel )
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial( body, vessel );
    }

    protected override void SetupGeometry(LegTypeVesselBody body, LegTypeVessel vessel)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateCylinder(go, vessel, body);
      CreateLegs(go, vessel, body);
      CreateUpperCap(go, vessel, body);
      CreateLowerCap(go, vessel, body);
    }

    void CreateCylinder(GameObject top, LegTypeVessel vessel, LegTypeVesselBody body)
    {
      var cylinderBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      cylinderBody.transform.parent = top.transform;
      cylinderBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(vessel.HeightOfLeg + 0.5 * vessel.HeightOfCylinder));
      cylinderBody.transform.localRotation = Quaternion.LookRotation(Vector3.up);
      cylinderBody.transform.localScale = new Vector3((float)vessel.DiameterOfCylinder, (float)(0.5 * vessel.HeightOfCylinder), (float)vessel.DiameterOfCylinder);
      body.CylinderBody = cylinderBody;
    }

    void CreateLegs(GameObject top, LegTypeVessel vessel, LegTypeVesselBody body)
    {
      var northLegBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      northLegBody.transform.parent = top.transform;
      northLegBody.transform.localPosition = new Vector3(0.0f, (float)(0.5 * vessel.DiameterOfCylinder), (float)(0.5 * vessel.HeightOfLeg));
      northLegBody.transform.localRotation = Quaternion.identity;
      northLegBody.transform.localScale = new Vector3((float)vessel.WidthOfLegs, (float)vessel.LegThickness, (float)vessel.HeightOfLeg);
      body.NorthLegBody = northLegBody;

      var southLegBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      southLegBody.transform.parent = top.transform;
      southLegBody.transform.localPosition = new Vector3(0.0f, -(float)(0.5 * vessel.DiameterOfCylinder), (float)(0.5 * vessel.HeightOfLeg));
      southLegBody.transform.localRotation = Quaternion.identity;
      southLegBody.transform.localScale = new Vector3((float)vessel.WidthOfLegs, (float)vessel.LegThickness, (float)vessel.HeightOfLeg);
      body.SouthLegBody = southLegBody;

      var eastLegBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      eastLegBody.transform.parent = top.transform;
      eastLegBody.transform.localPosition = new Vector3((float)(0.5 * vessel.DiameterOfCylinder), 0.0f, (float)(0.5 * vessel.HeightOfLeg));
      eastLegBody.transform.localRotation = Quaternion.identity;
      eastLegBody.transform.localScale = new Vector3((float)vessel.LegThickness, (float)vessel.WidthOfLegs, (float)vessel.HeightOfLeg);
      body.EastLegBody = eastLegBody;

      var westLegBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      westLegBody.transform.parent = top.transform;
      westLegBody.transform.localPosition = new Vector3(-(float)(0.5 * vessel.DiameterOfCylinder), 0.0f, (float)(0.5 * vessel.HeightOfLeg));
      westLegBody.transform.localRotation = Quaternion.identity;
      westLegBody.transform.localScale = new Vector3((float)vessel.LegThickness, (float)vessel.WidthOfLegs, (float)vessel.HeightOfLeg);
      body.WestLegBody = westLegBody;
    }

    void CreateUpperCap(GameObject top, LegTypeVessel vessel, LegTypeVesselBody body)
    {
      var upperCapBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      upperCapBody.transform.parent = top.transform;
      upperCapBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(vessel.HeightOfLeg + vessel.HeightOfCylinder));
      upperCapBody.transform.localRotation = Quaternion.identity;
      upperCapBody.transform.localScale = new Vector3((float)vessel.DiameterOfCylinder, (float)vessel.DiameterOfCylinder, (float)(2.0 * vessel.LengthOfUpperCap));
      body.UpperCapBody = upperCapBody;
    }

    void CreateLowerCap(GameObject top, LegTypeVessel vessel, LegTypeVesselBody body)
    {
      var lowerCapBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      lowerCapBody.transform.parent = top.transform;
      lowerCapBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(vessel.HeightOfLeg));
      lowerCapBody.transform.localRotation = Quaternion.identity;
      lowerCapBody.transform.localScale = new Vector3((float)vessel.DiameterOfCylinder, (float)vessel.DiameterOfCylinder, (float)(2.0 * vessel.LengthOfLowerCap));
      body.LowerCapBody = lowerCapBody;
    }
  }
}
