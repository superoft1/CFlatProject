using UnityEngine ;
using UnityEngine.EventSystems ;
using UnityEngine.UI ;

namespace Chiyoda.UI
{
  class ListBoxItem : MonoBehaviour
  {
    [SerializeField]
    private Toggle toggleButton ;

    [SerializeField]
    private Text label ;

    [SerializeField]
    private Image image ;
    
    public object Tag { get ; set ; }

    public bool IsOn
    {
      get => toggleButton.isOn ;
      set => toggleButton.isOn = value ;
    }

    public bool IsVisible
    {
      get => true ;
      set { }
    }

    public string Text
    {
      get => label.text ;
      set => label.text = value ;
    }

    public Color TextColor
    {
      get => label.color ;
      set => label.color = value ;
    }

    public Texture2D Image
    {
      set => image.sprite = Sprite.Create(value, new Rect(0.0f, 0.0f, value.width, value.height), Vector2.zero);
    }
  }
}