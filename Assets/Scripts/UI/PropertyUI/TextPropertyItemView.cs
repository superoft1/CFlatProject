using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core ;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI.PropertyUI
{
  class TextPropertyItemView : PropertyItemView
  {
    [SerializeField]
    private InputField input;

    public override object Value
    {
      get { return input.text; }
      protected set
      {
        if ( input.isFocused ) return ;

        input.text = (null == value ? "" : value.ToString()); 
      }
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



#if UNITY_EDITOR
    [UnityEditor.CustomEditor( typeof( TextPropertyItemView ) )]
    public class TextPropertyItemViewEditor : PropertyItemViewEditor<TextPropertyItemView>
    {
      protected override void OnValueGUI( TextPropertyItemView view )
      {
        if ( null != view.input ) {
          view.Value = UnityEditor.EditorGUILayout.TextField( "Value", (null == view.Value ? "" : view.Value.ToString()) );
        }
      }
    }
#endif
  }
}
