using System ;
using Chiyoda.UI.Dialog;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment ;
using UnityEngine;

namespace Chiyoda.UI
{
  public class ControlValveBlockPatternIcon : BlockPatternIcon
  {
    [SerializeField]
    ControlValveSelectDialogTemp _controlValveSelectDialogTemp;

    protected override void CreateInitialElement( Document document, Action<Edge> onFinish )
    {
      ControlValveBlockPatternImporter.Import( onFinish ) ;
    }

    protected override void ShowDialog()
    {
      _controlValveSelectDialogTemp.Show("Select Control Valve", "");
      ControlValveBlockPatternImporter.Prepare();
      if (_controlValveSelectDialogTemp.OKClickedHandler == null)
      {
        _controlValveSelectDialogTemp.OKClickedHandler += (s, e) =>
        {
          ControlValveBlockPatternImporter.Import(_controlValveSelectDialogTemp.GetCVType(), edge =>
          {
            UpdateElement(edge);
            FixPlacement();
          });
        };
      }
    }
    
    protected override void CloseDialog()
    {
      _controlValveSelectDialogTemp.gameObject.SetActive(false);
    }
  }
}