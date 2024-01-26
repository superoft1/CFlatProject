using System;
using System.Collections.Generic;
using System.Globalization ;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core ;
using UnityEngine;
using UnityEngine.UI;

namespace Chiyoda.UI.PropertyUI
{
  class LabelPropertyItemView : PropertyItemView
  {
    public override string Label { get; protected set; }


    public override object Value
    {
      get { return base.Label ; }
      protected set { base.Label = ( null == value ? "" : value.ToString() ) ; }
    }

    public override bool IsReadOnly
    {
      get { return true; }
      set { }
    }



#if UNITY_EDITOR
    [UnityEditor.CustomEditor( typeof( LabelPropertyItemView ) )]
    public class LabelPropertyItemViewEditor : PropertyItemViewEditor<LabelPropertyItemView>
    {
      protected override void OnValueGUI( LabelPropertyItemView view )
      { }
    }
#endif
  }
}
