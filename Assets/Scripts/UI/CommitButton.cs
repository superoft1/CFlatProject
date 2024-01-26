using Chiyoda.CAD.Core ;
#if UNITY_EDITOR
using UnityEditor ;
#endif
using UnityEngine ;
using UnityEngine.EventSystems ;
using UnityEngine.UI ;

namespace Chiyoda.UI
{
  public class CommitButton : Button
  {
    [SerializeField]
    private bool historyCommit = true ;

    public override void OnPointerClick( PointerEventData eventData )
    {
      if ( eventData.button != PointerEventData.InputButton.Left ) return ;

      base.OnPointerClick( eventData ) ;

      if ( historyCommit ) {
        DocumentCollection.Instance.Current?.HistoryCommit() ;
      }
    }
  }
}