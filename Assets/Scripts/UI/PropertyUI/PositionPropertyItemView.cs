using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI.PropertyUI
{
  class PositionPropertyItemView : PropertyItemView
  {
    [SerializeField]
    private NumericPropertyItemView xValue;

    [SerializeField]
    private NumericPropertyItemView yValue;

    [SerializeField]
    private NumericPropertyItemView zValue;

    public override object Value
    {
      get
      {
        double x = (double)xValue.Value;
        double y = -(double)yValue.Value;
        double z = (double)zValue.Value;
        return new Vector3d( x, y, z );
      }
      protected set
      {
        if ( value is Vector3 vec ) {
          xValue.SetProperty( null, vec.x ) ;
          yValue.SetProperty( null, -vec.y ) ;
          zValue.SetProperty( null, vec.z ) ;
        }
        else if ( value is Vector3d vec3d ) {
          xValue.SetProperty( null, vec3d.x ) ;
          yValue.SetProperty( null, -vec3d.y ) ;
          zValue.SetProperty( null, vec3d.z ) ;
        }
        else {
          xValue.SetProperty( null, null ) ;
          yValue.SetProperty( null, null ) ;
          zValue.SetProperty( null, null ) ;
        }
      }
    }

    public override bool IsReadOnly
    {
      get { return xValue.IsReadOnly; }
      set
      {
        xValue.IsReadOnly = value;
        yValue.IsReadOnly = value;
        zValue.IsReadOnly = value;
      }
    }

    private void Start()
    {
      xValue.ValueChanged += Input_ValueChanged;
      yValue.ValueChanged += Input_ValueChanged;
      zValue.ValueChanged += Input_ValueChanged;
      xValue.EndEdit += Input_EndEdit;
      yValue.EndEdit += Input_EndEdit;
      zValue.EndEdit += Input_EndEdit;
    }

    private void OnDestroy()
    {
      xValue.ValueChanged -= Input_ValueChanged;
      yValue.ValueChanged -= Input_ValueChanged;
      zValue.ValueChanged -= Input_ValueChanged;
      xValue.EndEdit -= Input_EndEdit;
      yValue.EndEdit -= Input_EndEdit;
      zValue.EndEdit -= Input_EndEdit;
    }

    private void Input_EndEdit( object sender, EventArgs e )
    {
      ReapplyCurrentValue();
    }

    private void Input_ValueChanged( object sender, EventArgs e )
    {
      OnValueChanged( EventArgs.Empty );
    }
  }
}
