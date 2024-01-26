using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI.PropertyUI
{
  class Position2DPropertyItemView : PropertyItemView
  {
    [SerializeField]
    private NumericPropertyItemView xValue;

    [SerializeField]
    private NumericPropertyItemView yValue;

    public override object Value
    {
      get
      {
        double x = (double)xValue.Value;
        double y = -(double)yValue.Value;
        return new Vector2( (float)x, (float)y );
      }
      protected set
      {
        if ( value is Vector2 vec ) {
          xValue.SetProperty( null, vec.x ) ;
          yValue.SetProperty( null, -vec.y ) ;
        }
        else {
          xValue.SetProperty( null, null ) ;
          yValue.SetProperty( null, null ) ;
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
      }
    }

    private void Start()
    {
      xValue.ValueChanged += Input_ValueChanged;
      yValue.ValueChanged += Input_ValueChanged;
      xValue.EndEdit += Input_EndEdit;
      yValue.EndEdit += Input_EndEdit;
    }

    private void OnDestroy()
    {
      xValue.ValueChanged -= Input_ValueChanged;
      yValue.ValueChanged -= Input_ValueChanged;
      xValue.EndEdit -= Input_EndEdit;
      yValue.EndEdit -= Input_EndEdit;
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
