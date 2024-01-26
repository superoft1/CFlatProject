using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment ;
using Chiyoda.UI.Dialog;
using UnityEngine;

namespace Chiyoda.UI
{
  public class HorizontalHeatExchangerBlockPatternIcon : BlockPatternIcon
  {
    [SerializeField]
    HeatExchangerSelectDialogTemp _heatExchangerSelectDialogTemp;

    protected override void CreateInitialElement( Document document, Action<Edge> onFinish )
    {
      HorizontalHeatExchangerBlockPatternImporter.Import(HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_A1_1_G, onFinish ) ;
    }
    protected override void ShowDialog()
    {
      _heatExchangerSelectDialogTemp.Show("Select Heat Exchanger", "");
      if (_heatExchangerSelectDialogTemp.OKClickedHandler == null)
      {
        _heatExchangerSelectDialogTemp.OKClickedHandler += (s, e) =>
        {
          HorizontalHeatExchangerBlockPatternImporter.Import(_heatExchangerSelectDialogTemp.GetHeatExchangerType(), edge =>
          {
            UpdateElement(edge);
            FixPlacement();
          });
        };
      }
    }

    protected override void CloseDialog()
    {
      _heatExchangerSelectDialogTemp.gameObject.SetActive(false);
    }
  }
}