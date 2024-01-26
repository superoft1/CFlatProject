using System;
using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [System.Serializable]
  public abstract class Component : PipingPiece
  {
    protected Component( Document document ) : base( document )
    {
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as Component;
      StockNumber = entity.StockNumber;
    }

    public Vector3d Origin
    {
      get { return Vector3d.zero; }
    }
    
    public Vector3d Axis
    {
      get
      {
        return Vector3d.right;
      }
    }

    public Vector3d SecondAxis
    {
      get
      {
        return Vector3d.up ;
      }
    }
    
    public Vector3d ThirdAxis
    {
      get
      {
        return Vector3d.forward ;
      }
    }


    [UI.Property(UI.PropertyCategory.ComponentName, "ID",ValueType = UI.ValueType.Label, Visibility = UI.PropertyVisibility.ReadOnly)]
    public string ComponentName { get; set; }

    [UI.Property(UI.PropertyCategory.StockNumber, "ID", Visibility = UI.PropertyVisibility.Editable)]
    public string StockNumber { get; set; }

  }

}