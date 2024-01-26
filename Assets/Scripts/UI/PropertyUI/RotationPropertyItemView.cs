using System;
using System.Collections.Generic;
using System.Globalization ;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CAD.Math ;
using Chiyoda.CAD.Core ;
using DuoVia.FuzzyStrings ;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI.PropertyUI
{
  class RotationPropertyItemView : PropertyItemView
  {
    [SerializeField]
    private InputField input;
    [SerializeField]
    private Text minusText;
    [SerializeField]
    private Text plusText;
    
    private CircleRange _circleRange = new CircleRange( 0, 360 ) ;
    private double _center = 180 ;
    private double _stepValue = 90;

    public override object Value
    {
      get { return Unnormalize( UnitConverter.StringToDegree( input.text ) ?? 0.0 ) ; }
      protected set
      {
        if ( input.isFocused ) return ;

        input.text = ( null == value ? "" : (int) Math.Round( CircleRange.Normalize( (double) value ) ) + "°" ) ;
      }
    }

    protected override void OnPropertyChanged()
    {
      var prop = Property ;

      if ( prop is UserDefinedSteppedNamedProperty stprop ) {
        SetStep( stprop.MinValue, stprop.MaxValue, stprop.Step ) ;
      }
      else if ( prop is IUserDefinedRangedNamedProperty rprop ) {
        SetStep( rprop .MinValue, rprop .MaxValue, 90 ) ;
      }
      else {
        SetStep( 0, 360, 90 ) ;
      }
    }

    private void SetStep( double min, double max, double step )
    {
      _circleRange = new CircleRange(min, max);
      _center = ( min + max ) / 2 ;
      _stepValue = step;
      minusText.text = "-" + step;
      plusText.text = step.ToString();
    }

    public override bool IsReadOnly
    {
      get { return !input.interactable; }
      set { input.interactable = !value; }
    }

    private void Start()
    {
      input.onValueChanged.AddListener( ValueChangedListener );
      input.onEndEdit.AddListener( EndEditListener );
    }
    private void OnDestroy()
    {
      input.onValueChanged.RemoveListener( ValueChangedListener );
      input.onEndEdit.RemoveListener( EndEditListener );
    }

    private void EndEditListener( string newValue )
    {
      ReapplyCurrentValue();
      DocumentCollection.Instance.Current?.HistoryCommit() ;
    }

    private void ValueChangedListener( string newValue )
    {
      OnValueChanged( EventArgs.Empty );
    }

    public void OnPlusButtonClicked()
    {
      var oldVal = (double) Value ;
      if ( _circleRange.IsInside( oldVal ) ) {
        Value = Unnormalize( _circleRange.Clamp( true, oldVal + _stepValue ) ) ;
        DocumentCollection.Instance.Current?.HistoryCommit() ;
      }
    }

    public void OnMinusButtonClicked()
    {
      var oldVal = (double) Value ;
      if ( _circleRange.IsInside( oldVal) ) {
        Value = Unnormalize( _circleRange.Clamp( false, oldVal - _stepValue ) ) ;
        DocumentCollection.Instance.Current?.HistoryCommit() ;
      }
    }

    private double Unnormalize( double clamped )
    {
      if ( _center <= clamped - 180 ) {
        return clamped - 360 ;
      }

      return clamped ;
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor( typeof( RotationPropertyItemView ) )]
    public class RotationPropertyItemViewEditor : PropertyItemViewEditor<RotationPropertyItemView>
    {
      protected override void OnValueGUI( RotationPropertyItemView view )
      {
        if ( null != view.input ) {
          view.input.text = UnityEditor.EditorGUILayout.TextField( "Value", view.input.text );
        }
      }
    }
#endif
  }
}
