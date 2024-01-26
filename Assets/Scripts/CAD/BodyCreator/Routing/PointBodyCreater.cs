using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Routing;

namespace Chiyoda.CAD.Body
{
  public class PointBodyCreater : BodyCreator<Point, Body>
  {
    public PointBodyCreater( Entity _entity ) : base( _entity )
    {}

    protected override void SetupMaterials(Body body, Point entity)
    {
    }
    //    protected override bool RecreateOnUpdate { get { return true; } }

    protected override void SetupGeometry(Body body, Point entity)
    {
      var d = (float)entity.OutsideDiameter;

      body.MainObject.transform.localPosition = (Vector3)entity.Origin;

      for (int i = 0; i < body.MainObject.transform.childCount; i++)
      {
        var childTF = body.MainObject.transform.GetChild(i);
        switch (childTF.name)
        {
          //直方体を表示する
          case "Cube":
            childTF.localScale = new Vector3(d, d, d); 
            break;
          //矢印は非表示
          case "ConePositive":
          case "CylinderPositive":
          case "ConeNegative":
          case "CylinderNegative":
            childTF.gameObject.SetActive( false );
            break;
          default:
            break;
        }
      }
    }
  }
}
