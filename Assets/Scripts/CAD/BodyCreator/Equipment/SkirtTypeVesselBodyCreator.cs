using System.Collections.Generic;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class SkirtTypeVesselBodyCreator : BodyCreator<SkirtTypeVessel, SkirtTypeVesselBody>
  {
    public SkirtTypeVesselBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( SkirtTypeVesselBody body, SkirtTypeVessel vessel )
    {
      SetupMaterial( body.TowerBody, body, vessel, false );
      SetupMaterial( body.SkirtBody, body, vessel, true );
      SetupMaterial( body.UpperCapBody, body, vessel, false );
      SetupMaterial( body.LowerCapBody, body, vessel, false );
    }

    private void SetupMaterial( GameObject go, SkirtTypeVesselBody body, SkirtTypeVessel vessel, bool isFoundation )
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial( body, vessel, isFoundation );
    }

    protected override void SetupGeometry(SkirtTypeVesselBody body, SkirtTypeVessel vessel)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateTower(go, vessel, body);
      CreateSkirt(go, vessel, body);
      CreateUpperCap(go, vessel, body);
      CreateLowerCap(go, vessel, body);
    }

    void CreateTower(GameObject top, SkirtTypeVessel vessel, SkirtTypeVesselBody body)
    {
      var towerBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      towerBody.transform.parent = top.transform;
      towerBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(vessel.HeightOfSkirt + 0.5 * vessel.HeightOfTower));
      towerBody.transform.localRotation = Quaternion.LookRotation(Vector3.up);
      towerBody.transform.localScale = new Vector3((float)vessel.DiameterOfTower, (float)(0.5 * vessel.HeightOfTower), (float)vessel.DiameterOfTower);
      body.TowerBody = towerBody;
    }

    void CreateSkirt(GameObject top, SkirtTypeVessel vessel, SkirtTypeVesselBody body)
    {
      var skirtBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      skirtBody.transform.parent = top.transform;
      skirtBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(0.5 * vessel.HeightOfSkirt));
      skirtBody.transform.localRotation = Quaternion.LookRotation(Vector3.up);
      skirtBody.transform.localScale = new Vector3((float)vessel.DiameterOfTower, (float)(0.5 * vessel.HeightOfSkirt), (float)vessel.DiameterOfTower);
      body.SkirtBody = skirtBody;
    }

    void CreateUpperCap(GameObject top, SkirtTypeVessel vessel, SkirtTypeVesselBody body)
    {
      var upperCapBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      upperCapBody.transform.parent = top.transform;
      upperCapBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(vessel.HeightOfSkirt + vessel.HeightOfTower));
      upperCapBody.transform.localRotation = Quaternion.identity;
      upperCapBody.transform.localScale = new Vector3((float)vessel.DiameterOfTower, (float)vessel.DiameterOfTower, (float)(2.0 * vessel.LengthOfUpperCap));
      body.UpperCapBody = upperCapBody;
    }

    void CreateLowerCap(GameObject top, SkirtTypeVessel vessel, SkirtTypeVesselBody body)
    {
      var lowerCapBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      lowerCapBody.transform.parent = top.transform;
      lowerCapBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(vessel.HeightOfSkirt));
      lowerCapBody.transform.localRotation = Quaternion.identity;
      lowerCapBody.transform.localScale = new Vector3((float)vessel.DiameterOfTower, (float)vessel.DiameterOfTower, (float)(2.0 * vessel.LengthOfLowerCap));
      body.LowerCapBody = lowerCapBody;
    }
  }
}
