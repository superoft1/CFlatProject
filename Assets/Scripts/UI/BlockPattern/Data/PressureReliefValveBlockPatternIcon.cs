using System ;
using Chiyoda.UI.Dialog;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment ;
using UnityEngine;

namespace Chiyoda.UI
{
  public class PressureReliefValveBlockPatternIcon : BlockPatternIcon
  {
    [SerializeField]
    PressureReliefValveSelectDialogTemp _pressureReliefSelectDialogTemp;

    protected override void CreateInitialElement(Document document, Action<Edge> onFinish)
    {
      Debug.Log("PressureReliefValveBlockPatternIcon.CreateInitialElement");
      // PressureReliefValveBlockPatternImporter.Import(PressureReliefValveBlockPatternImporter.PressureReliefValveType.PRV_C1_H, onFinish);
      PressureReliefValveBlockPatternImporter.Import(onFinish);
    }

    protected override void ShowDialog()
    {
      _pressureReliefSelectDialogTemp.Show("Select Pressure Relief Valve", "");
      if (_pressureReliefSelectDialogTemp.OKClickedHandler == null)
      {
        _pressureReliefSelectDialogTemp.OKClickedHandler += (s, e) =>
        {
          PressureReliefValveBlockPatternImporter.Import(_pressureReliefSelectDialogTemp.GetPRVType(), edge =>
          {
            UpdateElement(edge);
            FixPlacement();
          });
        };
      }
    }
    
    protected override void CloseDialog()
    {
      _pressureReliefSelectDialogTemp.gameObject.SetActive(false);
    }
  }
}
