using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Chiyoda.UI.Dialog ;

namespace Chiyoda.UI
{
  public class EndTopTypePumpBlockPatternIcon : PumpBlockPatternIcon
  {
    protected override void CreateInitialElement( Document document, Action<Edge> onFinish )
    {
      EndTopTypePumpBlockPatternImporter.Import( ((EndTopTypePumpSettingDialog)_pumpSettingDialog).GetPumpType(), onFinish ) ;
    }

    protected override void AddValueChangedEventHandler()
    {
      if ( _pumpSettingDialog.ValueChangeHandler == null ) {
        _pumpSettingDialog.ValueChangeHandler += ( s, e ) =>
        {
          EndTopTypePumpBlockPatternImporter.Import( ((EndTopTypePumpSettingDialog)_pumpSettingDialog).GetPumpType(), UpdateElement ) ;
        } ;
      }
    }

    protected override void UIShow()
    {
      _pumpSelectDialogTemp.Show( "Select pump type", "" ) ;
      if ( _pumpSelectDialogTemp.OKClickedHandler == null ) {
        _pumpSelectDialogTemp.OKClickedHandler += ( s, e ) =>
        {
          EndTopTypePumpBlockPatternImporter.Import( ((EndTopTypePumpSelectDialogTemp)_pumpSelectDialogTemp).GetPumpType(), edge =>
          {
            UpdateElement( edge ) ;
            FixPlacement() ;
          } ) ;
        } ;
      }
    }
  }
}