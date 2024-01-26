using System ;

public class SideSideTypePumpSettingDialog : PumpSettingDialog
{
  protected virtual void InitializeKey()
  {
    _strParamKey = strCheck + "SS" ;
  }

  protected override void SetKey()
  {
    InitializeKey() ;
    SetKeyForValveNum() ;
    SetKeyForMiniFlowToggle() ;
    SetKeyForValveDir() ;
  }

  private void SetKeyForMiniFlowToggle()
  {
    _strParamKey += "1" ;
  }

  public SideSideTypePumpBlockPatternImporter.PumpType GetPumpType()
  {
    foreach ( var pumpType in Enum.GetValues( typeof(SideSideTypePumpBlockPatternImporter.PumpType) ) ) {
      string name = Enum.GetName( typeof( SideSideTypePumpBlockPatternImporter.PumpType ), pumpType ) ;
      if ( name == _strParamKey ) {
        _bMatchType = true ;
        return (SideSideTypePumpBlockPatternImporter.PumpType)pumpType ;
      }
    }

    _bMatchType = false ;
    return SideSideTypePumpBlockPatternImporter.PumpType.CheckSS_A1_H_H ;
  }
}
