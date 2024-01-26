using System.Linq;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Routing;

namespace Chiyoda.CAD.Body
{
  public class DirectionalPointBodyCreater : BodyCreator<DirectionalPoint, Body>
  {
    public DirectionalPointBodyCreater(Entity _entity) : base(_entity)
    { }

    protected override void SetupMaterials(Body body, DirectionalPoint entity)
    {
      foreach ( var childTF in body.MainObject.transform.OfType<Transform>() )
      {
        switch (childTF.name)
        {
          case "Cube":
            break;
          case "ConePositive":
          case "CylinderPositive":
            childTF.gameObject.SetActive(ShouldArrowEnable(body, entity));
            if (entity.ShowArrow == DirectionalPoint.ShowArrowMode.InHighlight) // 通過点の場合は色を変える
            {
              SetColor(childTF.gameObject, Color.green);
            }
            break;
          case "ConeNegative": // マイナス方向は常に見せない
          case "CylinderNegative":
            childTF.gameObject.SetActive(false);
            break;
          default:
            break;
        }
      }
    }

    private static void SetColor(GameObject go, Color color)
    {
      if (go == null) return;
      var renderes = go.GetComponentsInChildren<MeshRenderer>();
      foreach (var render in renderes)
      {
        if (render.material != null)
        {
          render.material.color = color;
        }
      }
    }

    protected override void SetupGeometry(Body body, DirectionalPoint entity)
    {
      var d = (float)entity.OutsideDiameter;

      for (int i = 0; i < body.MainObject.transform.childCount; i++)
      {
        var childTF = body.MainObject.transform.GetChild(i);

        switch (childTF.name)
        {
          case "Cube":
            if (entity.Direction.magnitude < 1e-3) //　方向が定まっていない
            {
              childTF.localScale = new Vector3(d, d, d);
            }
            else
            {
              childTF.localScale = new Vector3(d / 5.0f, d, d);
            }
            break;
          default:
            break;
        }
      }
    }

    bool ShouldArrowEnable(Body body, DirectionalPoint entity)
    {
      switch (entity.ShowArrow)
      {
        case DirectionalPoint.ShowArrowMode.Always:
          return true;
        case DirectionalPoint.ShowArrowMode.None:
          return false;
        case DirectionalPoint.ShowArrowMode.InHighlight:
          if (body.IsHighlighted)
          {
            return true;
          }
          else
          {
            return false;
          }
        default:
          return false;
      }
    }
  }
}
