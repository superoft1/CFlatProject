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
  class TreeViewItemExpander : MonoBehaviour, IPointerClickHandler
  {
    [SerializeField]
    private TreeViewItem treeViewItem;

    [SerializeField]
    private Sprite expandedImageSprite;

    private Image _image;

    private Sprite _collapsedImageSprite;

    private bool _canCollapse = true;
    public bool CanCollapse
    {
      get { return _canCollapse; }
      set
      {
        _canCollapse = value;

        if ( !_canCollapse ) {
          IsOn = true;
        }
      }
    }

    private bool _expandVisible = false;
    public bool ExpandVisible
    {
      get { return _expandVisible; }
      set
      {
        _expandVisible = value;

        _image.enabled = _expandVisible;
      }
    }

    private bool _expanded = false;
    public bool IsOn
    {
      get { return _expanded; }
      set
      {
        if ( _canCollapse || value ) {
          _expanded = value;

          _image.sprite = _expanded ? expandedImageSprite : _collapsedImageSprite;
        }

        OnValueChanged( _expanded );
      }
    }

    private void Awake()
    {
      _image = GetComponent<Image>();

      _collapsedImageSprite = _image.sprite;
    }

    public void OnPointerClick( PointerEventData e )
    {
      if ( PointerEventData.InputButton.Left != e.button ) {
        return;
      }

      IsOn = !IsOn;

      e.Use();
    }

    private void OnValueChanged( bool value )
    {
      treeViewItem.OnExpandedChanged();
    }
  }
}
