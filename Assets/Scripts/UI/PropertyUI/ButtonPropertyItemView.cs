using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core ;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI.PropertyUI
{
  class ButtonPropertyItemView : PropertyItemView
  {
    [SerializeField]
    private Button button;

    public override object Value
    {
      get => null ;
      protected set {}
    }

    public override bool IsReadOnly
    {
      get { return !button.interactable; }
      set { button.interactable = !value; }
    }

    private void Start()
    {
      button.onClick.AddListener( ClickListener );
    }
    private void OnDestroy()
    {
      button.onClick.RemoveListener( ClickListener );
    }

    private void ClickListener()
    {
      OnValueChanged( EventArgs.Empty );

      DocumentCollection.Instance.Current?.HistoryCommit() ;
    }
  }
}
