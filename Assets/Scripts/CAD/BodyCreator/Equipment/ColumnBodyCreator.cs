using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class ColumnBodyCreator : BodyCreator<Column, ColumnBody>
  {
    public ColumnBodyCreator(Entity _entity) : base(_entity)
    {
    }

    protected override void SetupMaterials( ColumnBody body, Column column )
    {
      SetupMaterial( body.TowerBody, body, column, false );
      SetupMaterial( body.SkirtBody, body, column, true );
      SetupMaterial( body.UpperCapBody, body, column, false );
      SetupMaterial( body.LowerCapBody, body, column, false );
    }

    private void SetupMaterial( GameObject go, ColumnBody body, Column column, bool isFoundation )
    {
      go.GetComponent<MeshRenderer>().material = GetMaterial( body, column, isFoundation );
    }

    protected override void SetupGeometry(ColumnBody body, Column column)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;

      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;

      CreateTower(go, column, body);
      CreateSkirt(go, column, body);
      CreateUpperCap(go, column, body);
      CreateLowerCap(go, column, body);
    }

    void CreateTower(GameObject top, Column column, ColumnBody body)
    {
      var towerBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      towerBody.transform.parent = top.transform;
      towerBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(column.HeightOfSkirt + 0.5 * column.HeightOfTower));
      towerBody.transform.localRotation = Quaternion.LookRotation(Vector3.up);
      towerBody.transform.localScale = new Vector3((float)column.DiameterOfTower, (float)(0.5 * column.HeightOfTower), (float)column.DiameterOfTower);
      body.TowerBody = towerBody;
    }

    void CreateSkirt(GameObject top, Column column, ColumnBody body)
    {
      var skirtBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      skirtBody.transform.parent = top.transform;
      skirtBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(0.5 * column.HeightOfSkirt));
      skirtBody.transform.localRotation = Quaternion.LookRotation(Vector3.up);
      skirtBody.transform.localScale = new Vector3((float)column.DiameterOfTower, (float)(0.5 * column.HeightOfSkirt), (float)column.DiameterOfTower);
      body.SkirtBody = skirtBody;
    }

    void CreateUpperCap(GameObject top, Column column, ColumnBody body)
    {
      var upperCapBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      Object.Destroy( upperCapBody.GetComponent<SphereCollider>() ) ;
      upperCapBody.AddComponent<MeshCollider>() ;
      upperCapBody.transform.parent = top.transform;
      upperCapBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(column.HeightOfSkirt + column.HeightOfTower));
      upperCapBody.transform.localRotation = Quaternion.identity;
      upperCapBody.transform.localScale = new Vector3((float)column.DiameterOfTower, (float)column.DiameterOfTower, (float)(2.0 * column.LengthOfUpperCap));
      body.UpperCapBody = upperCapBody;
    }

    void CreateLowerCap(GameObject top, Column column, ColumnBody body)
    {
      var lowerCapBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      lowerCapBody.transform.parent = top.transform;
      lowerCapBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(column.HeightOfSkirt));
      lowerCapBody.transform.localRotation = Quaternion.identity;
      lowerCapBody.transform.localScale = new Vector3((float)column.DiameterOfTower, (float)column.DiameterOfTower, (float)(2.0 * column.LengthOfLowerCap));
      body.LowerCapBody = lowerCapBody;
    }
  }
}
