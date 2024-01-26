using System;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using Chiyoda.UI.Dialog;

namespace Chiyoda.UI
{
  public class TopTopTypePumpBlockPatternIcon : PumpBlockPatternIcon
  {
    protected override void CreateInitialElement( Document document, Action<Edge> onFinish )
    {
      TopTopTypePumpBlockPatternImporter.Import( ((TopTopTypePumpSettingDialog)_pumpSettingDialog).GetPumpType(), onFinish ) ;
    }

    protected override void AddValueChangedEventHandler()
    {
      if ( _pumpSettingDialog.ValueChangeHandler == null ) {
        _pumpSettingDialog.ValueChangeHandler += ( s, e ) =>
        {
          TopTopTypePumpBlockPatternImporter.Import( ((TopTopTypePumpSettingDialog)_pumpSettingDialog).GetPumpType(), UpdateElement ) ;
        } ;
      }
    }

    protected override void UIShow()
    {
      _pumpSelectDialogTemp.Show( "Select pump type", "" ) ;
      if ( _pumpSelectDialogTemp.OKClickedHandler == null ) {
        _pumpSelectDialogTemp.OKClickedHandler += ( s, e ) =>
        {
          TopTopTypePumpBlockPatternImporter.Import( ((TopTopTypePumpSelectDialogTemp)_pumpSelectDialogTemp).GetPumpType(), edge =>
          {
            UpdateElement( edge ) ;
            FixPlacement() ;
          } ) ;
        } ;
      }
    }
  }
}