using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chiyoda.UI
{
  [RequireComponent( typeof( Image ) )]
  class TreeViewItemSelection : MonoBehaviour, IPointerClickHandler
  {
    private Image _image;

    private bool _selected = false;

    /// <summary>
    /// クリックした際のイベント。
    /// </summary>
    public event EventHandler<MouseButtonEventArgs> Click;

    /// <summary>
    /// ダブルクリックした際のイベント。
    /// </summary>
    public event EventHandler<MouseButtonEventArgs> DoubleClick;

    public bool IsOn
    {
      get { return _selected; }
      set
      {
        _selected = value;

        var color = _image.color;
        color.a = _selected ? 1.0f: 0.0f;
        _image.color = color;
      }
    }

    private void Awake()
    {
      _image = GetComponent<Image>();
    }

    public void OnPointerClick( PointerEventData e )
    {
      if ( 1 == e.clickCount ) {
        OnClick( new MouseButtonEventArgs( (MouseButtonType)e.button ) );
      }
      else if ( 2 == e.clickCount ) {
        OnDoubleClick( new MouseButtonEventArgs( (MouseButtonType)e.button ) );
      }

      e.Use();
    }

    protected virtual void OnClick( MouseButtonEventArgs e )
    {
      if ( null != Click ) {
        Click( this, e );
      }
    }

    protected virtual void OnDoubleClick( MouseButtonEventArgs e )
    {
      if ( null != DoubleClick ) {
        DoubleClick( this, e );
      }
    }
  }
}
