using System;
using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Manager;
using Chiyoda.CAD.Model;
using UnityEngine;

public class PickableGizmo : MonoBehaviour
{
  [SerializeField]
  private GizmoArrow xCone;
  [SerializeField]
  private GameObject xCylinder;

  [SerializeField]
  private GizmoArrow yCone;
  [SerializeField]
  private GameObject yCylinder;

  [SerializeField]
  private GizmoArrow zCone;
  [SerializeField]
  private GameObject zCylinder;

  private bool _isDraggingAnyAxis = false;

  public bool Dragging
  {
    get => _isDraggingAnyAxis;
    set
    {
      if ( _isDraggingAnyAxis != value ) {
        _isDraggingAnyAxis = value;
        DragStateChanged?.Invoke( this, EventArgs.Empty );
      }
    }
  }

  public event EventHandler DragStateChanged;
  public event EventHandler Destroyed;

  public bool XVisible { get => xCone.gameObject.activeSelf; set { xCone.gameObject.SetActive( value ); xCylinder.SetActive( value ); } }
  public bool YVisible { get => yCone.gameObject.activeSelf; set { yCone.gameObject.SetActive( value ); yCylinder.SetActive( value ); } }
  public bool ZVisible { get => zCone.gameObject.activeSelf; set { zCone.gameObject.SetActive( value ); zCylinder.SetActive( value ); } }

  public bool XDragging => xCone.IsDragging;
  public bool YDragging => yCone.IsDragging;
  public bool ZDragging => zCone.IsDragging;

  public Entity Entity { get; internal set; }

  private void Start()
  {
    xCone.DragStateChanged += Cone_DragStateChanged;
    yCone.DragStateChanged += Cone_DragStateChanged;
    zCone.DragStateChanged += Cone_DragStateChanged;
  }

  private void OnDestroy()
  {
    xCone.DragStateChanged -= Cone_DragStateChanged;
    yCone.DragStateChanged -= Cone_DragStateChanged;
    zCone.DragStateChanged -= Cone_DragStateChanged;

    Destroyed?.Invoke( this, EventArgs.Empty );
  }

  private void Update()
  {
    var manager = ElementDragManager.Instance;
    if ( null != manager ) {
      var scale = CalcScale( manager.DraggerCamera );
      transform.localScale = new Vector3( scale, scale, scale );
    }
  }

  private float CalcScale( Camera camera )
  {
    return camera.orthographicSize * 0.11f ;
  }

  private void Cone_DragStateChanged( object sender, EventArgs e )
  {
    Dragging = xCone.IsDragging || yCone.IsDragging || zCone.IsDragging;
  }
}
