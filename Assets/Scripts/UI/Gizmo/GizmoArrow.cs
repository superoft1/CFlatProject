using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( MeshCollider ) )]
public class GizmoArrow : MonoBehaviour
{
  private bool _isDragging = false;

  public bool IsDragging
  {
    get => _isDragging;
    private set
    {
      if ( _isDragging == value ) return;
      _isDragging = value;

      DragStateChanged?.Invoke( this, EventArgs.Empty );
    }
  }

  public event EventHandler DragStateChanged;

  void OnMouseDown()
  {
    IsDragging = true;
  }

  void OnMouseDrag()
  {
    IsDragging = true;
  }

  void OnMouseUp()
  {
    IsDragging = false;
  }
}
