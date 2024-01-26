using UnityEngine ;
using UnityEngine.Events ;
using UnityEngine.EventSystems ;
using UnityEngine.UI ;

namespace Chiyoda.UI
{
  public class PropertySlider : Slider
  {
    [SerializeField]
    private UnityEvent m_OnSliderDetached = new UnityEvent() ;

    public UnityEvent onSliderDetached
    {
      get => this.m_OnSliderDetached ;
      set => this.m_OnSliderDetached = value ;
    }

    public override void OnPointerUp( PointerEventData eventData )
    {
      if ( IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left ) {
        m_OnSliderDetached?.Invoke() ;
      }
    }
  }
}