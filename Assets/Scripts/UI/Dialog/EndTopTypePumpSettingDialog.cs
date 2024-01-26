using System ;

public class EndTopTypePumpSettingDialog : PumpSettingDialog
{
  protected virtual void InitializeKey()
  {
    _strParamKey = strCheck + "ET" ;
  }

  protected override void SetKey()
  {
    InitializeKey() ;
    SetKeyForValveNum() ;
    SetKeyForMiniFlowToggle() ;
    SetKeyForValveDir() ;
    SetKeyForDischargeSize() ;
  }

  public EndTopTypePumpBlockPatternImporter.PumpType GetPumpType()
  {
    foreach ( var pumpType in Enum.GetValues( typeof(EndTopTypePumpBlockPatternImporter.PumpType) ) ) {
      string name = Enum.GetName( typeof( EndTopTypePumpBlockPatternImporter.PumpType ), pumpType ) ;
      if ( name == _strParamKey ) {
        _bMatchType = true ;
        return (EndTopTypePumpBlockPatternImporter.PumpType)pumpType ;
      }
    }

    _bMatchType = false ;
    return EndTopTypePumpBlockPatternImporter.PumpType.Pump8 ;
  }
}
