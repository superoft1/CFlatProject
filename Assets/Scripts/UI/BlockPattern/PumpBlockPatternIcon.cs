// UI切替え
//#define NEW_UI
using UnityEngine ;

namespace Chiyoda.UI
{
  public abstract class PumpBlockPatternIcon : BlockPatternIcon
  {
    [SerializeField]
    protected ModalDialog _pumpSelectDialogTemp ;

    [SerializeField]
    protected PumpSettingDialog _pumpSettingDialog ;

    protected override void ShowDialog()
    {
#if NEW_UI
      _pumpSettingDialog.Show() ;
      AddOkEventHandler() ;
      AddValueChangedEventHandler() ;
      AddCancelEventHandler() ;
      AddRotateEventHandler() ;
      _pumpSettingDialog.InitDialogInfo() ;
#else
      // 派生先で実装
      UIShow() ;
#endif
    }

    protected virtual void UIShow() {}

    private void AddOkEventHandler()
    {
      if ( _pumpSettingDialog.OKClickedHandler == null ) {
        _pumpSettingDialog.OKClickedHandler += ( s, e ) =>
        {
          FixPlacement() ;
        } ;
      }
    }

    /// <summary>
    /// ダイアログの内容が変更する度にImportし直す
    /// </summary>
    protected virtual void AddValueChangedEventHandler() {}

    private void AddCancelEventHandler()
    {
      if ( _pumpSettingDialog.CancelClickedHandler == null ) {
        _pumpSettingDialog.CancelClickedHandler += ( s, e ) =>
        {
          RemoveDragElement() ;
        } ;
      }
    }

    private void AddRotateEventHandler()
    {
      if ( _pumpSettingDialog.PlusRotateButtonHandler == null ) {
        _pumpSettingDialog.PlusRotateButtonHandler += ( s, e ) =>
        {
          bool bPlusRotate = true ;
          RotateDragElement( bPlusRotate ) ;
        } ;
      }

      if ( _pumpSettingDialog.MisunRotateButtonHandler == null ) {
        _pumpSettingDialog.MisunRotateButtonHandler += ( s, e ) =>
        {
          bool bPlusRotate = false ;
          RotateDragElement( bPlusRotate ) ;
        } ;
      }
    }
    
    protected override void CloseDialog()
    {
#if NEW_UI
      _pumpSettingDialog.gameObject.SetActive( false ) ;
#else
      _pumpSelectDialogTemp.gameObject.SetActive( false ) ;
#endif
    }
  }
}