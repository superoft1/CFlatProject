using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Structure;
using Chiyoda.CAD.Model.Structure.CommonEntities ;
using UnityEngine;

namespace Chiyoda.UI
{
  public abstract class PipeRackIcon : DragPlacementIcon<PlacementEntity>
  {
    [SerializeField]
    protected RackSettingDialog _rackSettingDialog ;

    protected override void RemoveFromParent( PlacementEntity placement )
    {
      placement.Document.Structures.Remove( placement ) ;
    }

    protected override void ShowDialog()
    {
      _rackSettingDialog.Show() ;
      AddOkEventHandler() ;
      AddValueChangedEventHandler() ;
      AddCancelEventHandler() ;
      _rackSettingDialog.InitDialogInfo() ;
    }

    private void AddOkEventHandler()
    {
      if ( _rackSettingDialog.OKClickedHandler == null ) {
          _rackSettingDialog.OKClickedHandler += ( s, e ) =>
        {
          FixPlacement() ;
        } ;
      }
    }

    protected virtual void AddValueChangedEventHandler() {}

    private void AddCancelEventHandler()
    {
      if ( _rackSettingDialog.CancelClickedHandler == null ) {
        _rackSettingDialog.CancelClickedHandler += ( s, e ) =>
        {
          RemoveDragElement() ;
        } ;
      }
    }

    protected override void CloseDialog()
    {
      _rackSettingDialog.gameObject.SetActive( false ) ;
    }
  }
}