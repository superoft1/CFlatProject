using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaterialUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chiyoda.UI
{
  /// <summary>
  /// ツリービュー要素内チェックボックス クラス。
  /// </summary>
  [RequireComponent( typeof( Image ) )]
  class TreeViewItemCheckBox : MonoBehaviour, IPointerClickHandler
  {
    [SerializeField]
    private Sprite uncheckedImageSprite;

    [SerializeField]
    private Sprite indeterminateImageSprite;

    private Image _image;

    private Sprite _checkedImageSprite;

    /// <summary>
    /// ツリービュー要素内チェックボックスをクリックした際のイベント。
    /// </summary>
    public event EventHandler<MouseButtonEventArgs> Click;

    private void Awake()
    {
      _image = GetComponent<Image>();

      _checkedImageSprite = _image.sprite;
    }

    public void SetState( CheckState checkState )
    {
      switch ( checkState ) {
        case CheckState.Unchecked:
          _image.sprite = uncheckedImageSprite;
          _image.color = new Color( 200f/255f, 200f/255f, 200f/255f, 128f/255f );
          break;
        case CheckState.Indeterminate:
          _image.sprite = indeterminateImageSprite;
          _image.color = Color.white;
          break;
        case CheckState.Checked:
          _image.sprite = _checkedImageSprite;
          _image.color = Color.white;
          break;
      }
    }

    public void OnPointerClick( PointerEventData e )
    {
      OnClick( new MouseButtonEventArgs( (MouseButtonType)e.button ) );

      e.Use();
    }

    protected virtual void OnClick( MouseButtonEventArgs e )
    {
      if ( null != Click ) {
        Click( this, e );
      }
    }
  }
}
