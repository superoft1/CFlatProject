using UnityEngine ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Plotplan;

namespace Chiyoda.UI
{
  class ListBoxItemPresenter : MonoBehaviour
  {
    [SerializeField]
    private ListBoxItem _prefab ;
    
    public ListBoxItem CreateItem( IElement elm )
    {
      var prefab = GetPrefab( elm ) ;
      if ( null == prefab ) {
        return null ;
      }

      var item = Instantiate( prefab.gameObject )?.GetComponent<ListBoxItem>() ;
      if ( null == item ) return null ;

      UpdateItem( elm, item ) ;

      return item ;
    }

    public void UpdateItem( IElement elm, ListBoxItem item )
    {
      item.IsOn = elm.Document.IsSelected( elm ) ;
      item.IsVisible = elm.IsVisible ;
      
      item.Text = GetName( elm ) ;
      item.TextColor = elm.HasError ? new Color( 248f / 255f, 152f / 255f, 0f / 255f ) : Color.white ;
      item.Tag = elm;

      if (elm.GetType() == typeof(LeafUnit))
      {
        item.Image = UnitContentManager.GetTexture(((LeafUnit) elm).UnitType);
      }
    }

    public virtual string GetName( IElement elm )
    {
      return elm.Name ;
    }

    public virtual ListBoxItem GetPrefab( IElement elm )
    {
      return _prefab ;
    }
  }
}