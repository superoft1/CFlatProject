using System;
using System.Collections.Generic;
using System.Linq;
using System.Net ;
using System.Text;
using Chiyoda.CAD.Core ;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI.PropertyUI
{
  class IntegerPropertyItemView : PropertyItemView
  {
    [SerializeField]
    private InputField input;

    public override object Value
    {
      get
      {
        if ( int.TryParse( input.text, out var i ) ) return i;
        return 0;
      }
      protected set
      {
        if ( input.isFocused ) return ;

        input.text = ( null == value ? "" : Convert.ToInt32( value ).ToString() ) ;
      }
    }
    
    protected override void OnPropertyChanged()
    {
      var prop = Property ;

      if ( prop is UserDefinedSteppedNamedProperty stprop ) {
        SetStep( (int) stprop.MinValue, (int) stprop.MaxValue, (int) stprop.Step ) ;
      }
      else if ( prop is IUserDefinedRangedNamedProperty rprop ) {
        SetStep( (int) rprop.MinValue, (int) rprop.MaxValue, 0 ) ;
      }
      else {
        SetStep( int.MinValue, int.MaxValue, 0 ) ;
      }
    }

    private void SetStep( int min, int max, int step )
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
      ReapplyCurrentValue();
      DocumentCollection.Instance.Current?.HistoryCommit() ;
    }



#if UNITY_EDITOR
    [UnityEditor.CustomEditor( typeof( IntegerPropertyItemView ) )]
    public class IntegerPropertyItemViewEditor : PropertyItemViewEditor<IntegerPropertyItemView>
    {
      protected override void OnValueGUI( IntegerPropertyItemView view )
      {
        if ( null != view.input ) {
          view.input.text = UnityEditor.EditorGUILayout.TextField( "Value", view.input.text );
        }
      }
    }
#endif
  }
}
