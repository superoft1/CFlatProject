using System ;
using System.Globalization ;
using Chiyoda.CAD.Model.Structure;
using UnityEngine ;
using UnityEngine.UI ;

public class RackSettingDialog : ModelessDialog
{
  public EventHandler ValueChangeHandler ;

  [SerializeField] protected InputField _id ;
  [SerializeField] protected InputField _interval ;
  [SerializeField] protected InputField _width ;
  [SerializeField] protected InputField _floorCount ;
  [SerializeField] protected InputField _beamInterval ;
  [SerializeField] protected Toggle _halfDownSideBeam ;
  [SerializeField] protected Text _messageText ;
  [SerializeField] protected Button _OKButton ;
  [SerializeField] protected Button _CancelButton ;

  private bool _valueError = false ;

  private int _intervalVal ;
  private double _widthVal ;
  private int _floorCountVal ;
  private double _beamIntervalVal ;

  public void SetInitialValue( int interval, int floorCount, double width, double beamInterval )
  {
    _interval.text = interval.ToString() ;
    _width.text = width.ToString( "F2" ) ;
    _floorCount.text = floorCount.ToString() ;
    _beamInterval.text = beamInterval.ToString( "F2" ) ;
  }

  public void OnValueChange()
  {
    GetAndCheckValue() ;
    
    if ( !_valueError ) ValueChangeHandler?.Invoke(this,EventArgs.Empty) ;
    
    UpdateMessage() ;
    UpdateButton() ;
  }

  private void UpdateMessage()
  {
    _messageText.text = "" ;
    if ( _valueError ) {
      _messageText.text = "Invalid value." ;
      _messageText.color = Color.red ;
    }
  }

  private void UpdateButton()
  {
    if ( _valueError ) {
      _OKButton.interactable = false ;
    }
    else {
      _OKButton.interactable = true ;
    }
  }

  public void InitDialogInfo()
  {
    OnValueChange() ;
  }

  private void GetAndCheckValue()
  {
    if ( int.TryParse( _interval.text, out _intervalVal ) &&
         double.TryParse( _width.text, out _widthVal ) &&
         int.TryParse( _floorCount.text, out _floorCountVal ) &&
         double.TryParse( _beamInterval.text, out _beamIntervalVal ) &&
         !( _intervalVal > StructureConstant.MaxInterval ||
            _widthVal > StructureConstant.MaxWidth ||
            _floorCountVal > StructureConstant.MaxFloorCount ||
            _beamIntervalVal > StructureConstant.MaxBeamInterval ) ) {
      _valueError = false ;
    }
    else {
      _valueError = true ;
    }
  }

  public string StructureId => _id.text ;
  public int Interval => _intervalVal ;
  
  public double Width => _widthVal ;
  public int FloorCount => _floorCountVal ;
  public double BeamInterval => _beamIntervalVal ; 
  public bool HalfDownSideBeam => _halfDownSideBeam.isOn ; 
  public bool HasError => _valueError ;
}
