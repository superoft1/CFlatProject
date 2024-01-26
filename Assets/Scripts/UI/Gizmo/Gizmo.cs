using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Chiyoda.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class Gizmo : MonoBehaviour, IPointerClickHandler {

  [SerializeField]
  RectTransform gizmoTarget;

  [SerializeField]
  CameraOperator cameraOperator;

  [SerializeField]
  Camera gizmoCamera;

  [SerializeField]
  Camera targetCamera;

  public void OnPointerClick(PointerEventData eventData)
  {
    switch (eventData.pointerEnter.name)
    {
      case "East":
        cameraOperator.SetDirectionXMinus();
        break;
      case "West":
        cameraOperator.SetDirectionXPlus();
        break;
      case "North":
        cameraOperator.SetDirectionYPlus();
        break;
      case "South":
        cameraOperator.SetDirectionYMinus();
        break;
    }
  }

  private void Update()
  {
    gizmoCamera.transform.localRotation = targetCamera.transform.localRotation;

    var pos = new Vector2(gizmoTarget.position.x, gizmoTarget.position.y);
    var ray = RectTransformUtility.ScreenPointToRay(gizmoCamera, pos);
    transform.position = ray.origin + ray.direction * 50f;
    
    // ギズモのクリック検知
    if (Input.GetMouseButtonDown(0))
    {
      var bounds = new Bounds(transform.position, new Vector3(0.6f, 0.6f, 0.6f));
      var hitInfo = RayCastUtil.RayCastFromMousePosition(gizmoCamera, bounds);
      if (hitInfo != null)
      {
        switch (hitInfo.hitSurface)
        {
          case RayCastUtil.Surface.Top:
            cameraOperator.SetDirectionZPlus();
            break;
          case RayCastUtil.Surface.Bottom:
            cameraOperator.SetDirectionZMinus();
            break;
          case RayCastUtil.Surface.Front:
            cameraOperator.SetDirectionYMinus();
            break;
          case RayCastUtil.Surface.Back:
            cameraOperator.SetDirectionYPlus();
            break;
          case RayCastUtil.Surface.Right:
            cameraOperator.SetDirectionXMinus();
            break;
          case RayCastUtil.Surface.Left:
            cameraOperator.SetDirectionXPlus();
            break;
        }
      }
    }
  }


}
