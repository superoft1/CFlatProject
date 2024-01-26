using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core ;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI.PropertyUI
{
  class CheckBoxPropertyItemView : PropertyItemView
  {
    [SerializeField]
    private Toggle toggle;

    public override object Value
    {
      get { return toggle.isOn; }
      protected set { toggle.isOn = ( value != null ) && Convert.ToBoolean( value ) ; }
    }

    public override bool IsReadOnly
    {
      get { return !toggle.interactable; }
      set { toggle.interactable = !value; }
    }

    private void Start()
    {
      toggle.onValueChanged.AddListener( ValueChangedListener );
    }
    private void OnDestroy()
    {
      toggle.onValueChanged.RemoveListener( ValueChangedListener );
    }

    private void ValueChangedListener( bool newValue )
    {
      OnValueChanged( EventArgs.Empty );

      DocumentCollection.Instance.Current?.HistoryCommit() ;
    }
  }
}
