using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core ;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI.PropertyUI
{
  class NumericPropertyItemView : PropertyItemView
  {
    [SerializeField]
    private InputField input;

    public event EventHandler EndEdit;

    // TODO: 長さ・距離モードの場合は単位つきの数字を用いるように
    public override object Value
    {
      get
      {
        if ( double.TryParse( input.text, out var d ) ) return d ;
        return 0d ;
      }
      protected set
      {
        if ( input.isFocused ) return ;

        input.text = ( null == value ? "" : $"{value:0.000}" ) ;
      }
    }

    protected override void OnPropertyChanged()
    {
      var prop = Property ;

      if ( prop is UserDefinedSteppedNamedProperty stprop ) {
        SetStep( stprop.MinValue, stprop.MaxValue, stprop.Step ) ;
      }
      else if ( prop is IUserDefinedRangedNamedProperty rprop ) {
        SetStep( rprop.MinValue, rprop.MaxValue, 0 ) ;
      }
      else {
        SetStep( -double.MinValue, double.MaxValue, 0 ) ;
      }
    }

    private void SetStep( double min, double max, double step )
    {
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

    private void ValueChangedListener( string newValue )
    {
      OnValueChanged( EventArgs.Empty );
    }

    private void EndEditListener( string newValue )
    {
      EndEdit?.Invoke( this, EventArgs.Empty );

      ReapplyCurrentValue();
      DocumentCollection.Instance.Current?.HistoryCommit() ;
    }



#if UNITY_EDITOR
    [UnityEditor.CustomEditor( typeof( NumericPropertyItemView ) )]
    public class NumericPropertyItemViewEditor : PropertyItemViewEditor<NumericPropertyItemView>
    {
      protected override void OnValueGUI( NumericPropertyItemView view )
      {
        if ( null != view.input ) {
          view.input.text = UnityEditor.EditorGUILayout.TextField( "Value", view.input.text );
        }
      }
    }
#endif
  }
}
