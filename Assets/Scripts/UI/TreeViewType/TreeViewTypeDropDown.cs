using Chiyoda.CAD.Presenter ;
using UnityEngine ;
using UnityEngine.UI ;

namespace UI.TreeViewType
{
  public class TreeViewTypeDropDown : MonoBehaviour
  {
    [SerializeField] private Dropdown dropdown ;

    // Start is called before the first frame update
    void Start()
    {
      dropdown.value = (int)PresenterManager.Instance.TreeViewItemPresenter.Mode ;
    }

    public void ChangeTreeViewType()
    {
      var treeViewItemPresenter = PresenterManager.Instance.TreeViewItemPresenter ;
      switch ( dropdown.value ) {
        case 0 :
          treeViewItemPresenter.Mode = TreeViewMode.BasicView ;
          break ;
        case 1 :
          treeViewItemPresenter.Mode = TreeViewMode.LineView ;
          break ;
        case 2 :
          treeViewItemPresenter.Mode = TreeViewMode.StreamView ;
          break ;
      }
    }
  }
}
