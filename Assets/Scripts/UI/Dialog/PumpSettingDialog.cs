using System ;
using UnityEngine ;
using UnityEngine.UI ;

public abstract class PumpSettingDialog : ModelessDialog
{
  public EventHandler ValueChangeHandler ;
  
  [SerializeField] protected Dropdown _suctionValveNum ;
  [SerializeField] protected Dropdown _suctionValveDir ;
  [SerializeField] protected Dropdown _suctionValveLine ;
  [SerializeField] protected Dropdown _dischargeValveNum ;
  [SerializeField] protected Dropdown _dischargeValveDir ;
  [SerializeField] protected Dropdown _dischargeValveLine ;
  [SerializeField] protected Dropdown _flowMeter ;
  [SerializeField] protected Toggle _bFlowMeter ;
  [SerializeField] protected Dropdown _dischargeDiameter ;
  [SerializeField] protected Text _messageText ;
  [SerializeField] protected Button _OKButton ;
  [SerializeField] protected Button _CancelButton ;

  public EventHandler PlusRotateButtonHandler ;
  public EventHandler MisunRotateButtonHandler ;

  protected string _strParamKey ;
  protected bool _bMatchType = false ;
  protected const string strCheck = "Check" ;

  private void OnValueChange()
  {
    // 派生先で実装
    SetKey() ;

    Debug.Log( _strParamKey ) ;
    ValueChangeHandler?.Invoke(this,EventArgs.Empty) ;

    UpdateMessage() ;
    UpdateButton() ;
  }

  protected abstract void SetKey() ;

  protected void SetKeyForValveNum()
  {
    var suctionValveNum = int.Parse( _suctionValveNum.captionText.text ) ;
    var dischargeValveNum = int.Parse( _dischargeValveNum.captionText.text ) ;

    if ( suctionValveNum == 1 && dischargeValveNum == 1 ) {
      _strParamKey += "_A" ;
    }
    else if ( suctionValveNum == 1 && dischargeValveNum == 2 ) {
      _strParamKey += "_B" ;
    }
    else if ( suctionValveNum == 2 && dischargeValveNum == 2 ) {
      _strParamKey += "_C" ;
    }
    else {
      _strParamKey += "_X" ;// 仮にXとしておく
      Debug.Log( "not supported." ) ;
    }
  }

  protected void SetKeyForMiniFlowToggle()
  {
    if ( !( _bFlowMeter.isOn ) ) {
      _strParamKey += "1" ;
    }
    else {
      _strParamKey += "2" ;
    }
  }

  protected void SetKeyForMiniFlowDropdown()
  {
    if ( _flowMeter.captionText.text == "No" ) {
      _strParamKey += "1" ;
    }
    else {
      _strParamKey += "2" ;
    }
  }

  protected void SetKeyForValveDir()
  {
    SetKeyForValveDir( _suctionValveDir.captionText.text ) ;
    SetKeyForValveDir( _dischargeValveDir.captionText.text ) ;
  }

  private void SetKeyForValveDir( string strValveDir )
  {
    if ( strValveDir == "Vertical" ) {
      _strParamKey += "_V" ;
    }
    else if ( strValveDir == "Horizontal" ) {
      _strParamKey += "_H" ;
    }
    else {
      Debug.Log( "not supported." ) ;
    }
  }

  protected void SetKeyForValveLine()
  {
    SetKeyForValveLine( _suctionValveLine.captionText.text ) ;
    SetKeyForValveLine( _dischargeValveLine.captionText.text ) ;
  }

  private void SetKeyForValveLine( string strValveLine )
  {
    if ( strValveLine == "Stage" ) {
      _strParamKey += "_S" ;
    }
    else if ( strValveLine == "Ground" ) {
      _strParamKey += "_G" ;
    }
    else {
      Debug.Log( "not supported." ) ;
    }
  }

  protected void SetKeyForFlowMeter()
  {
    var strFlowMeter = _flowMeter.captionText.text ;
    if ( strFlowMeter == "Condition Orifice" ) {
      _strParamKey += "_C" ;
    }
    else if ( strFlowMeter == "Orifice" ) {
      _strParamKey += "_O" ;
    }
    else if ( strFlowMeter == "No" ) {
      _strParamKey += "_N" ;
    }
    else {
      Debug.Log( "not supported." ) ;
    }
  }

  protected void SetKeyForDischargeSize()
  {
    var size = _dischargeDiameter.captionText.text.Substring( 0, 1 ) ;
    if ( size == "S" ) {// 6インチ以下はS、8インチ以上はL
      _strParamKey += "_S" ;
    }
    else if ( size == "L" ) {
      _strParamKey += "_L" ;
    }
    else {
      Debug.Log( "not supported." ) ;
    }
  }

  private void UpdateMessage()
  {
    var strOnlyTypeName = _strParamKey.Substring( strCheck.Length ) ;
    _messageText.text = "Pump type : " + strOnlyTypeName + "\r\n" ;
    if ( _bMatchType ) {
      _messageText.color = Color.black ;
    }
    else {
      _messageText.text += "is not supported. \r\nDefault pump type is shown." ;
      _messageText.color = Color.red ;
    }
  }

  private void UpdateButton()
  {
    if ( _bMatchType ) {
      _OKButton.interactable = true ;
    }
    else {
      _OKButton.interactable = false ;
    }
  }

  public void InitDialogInfo()
  {
    OnValueChange() ;
  }
  
  public void PlusButton_Clicked()
  {
    PlusRotateButtonHandler?.Invoke(this,EventArgs.Empty) ;
  }
  
  public void MinusButton_Clicked()
  {
    MisunRotateButtonHandler?.Invoke(this,EventArgs.Empty) ;
  }

}
