using System ;

public class TopTopTypePumpSettingDialog : PumpSettingDialog
{
  protected virtual void InitializeKey()
  {
    _strParamKey = strCheck + "TT" ;
  }

  protected override void SetKey()
  {
    InitializeKey() ;
    SetKeyForValveNum() ;
    SetKeyForMiniFlowDropdown() ; 
    SetKeyForValveLine() ;
    SetKeyForFlowMeter() ;
  }

  public TopTopTypePumpBlockPatternImporter.PumpType GetPumpType()
  {
    foreach ( var pumpType in Enum.GetValues( typeof(TopTopTypePumpBlockPatternImporter.PumpType) ) ) {
      string name = Enum.GetName( typeof( TopTopTypePumpBlockPatternImporter.PumpType ), pumpType ) ;
      if ( name == _strParamKey ) {
        _bMatchType = true ;
        return (TopTopTypePumpBlockPatternImporter.PumpType)pumpType ;
      }
    }

    _bMatchType = false ;
    return TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_A1_S_G_N ;
  }
}
