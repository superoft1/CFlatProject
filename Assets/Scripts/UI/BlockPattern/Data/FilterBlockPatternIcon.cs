using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment ;
using Chiyoda.UI.Dialog;
using UnityEngine;


namespace Chiyoda.UI
{
  public class FilterBlockPatternIcon : BlockPatternIcon
  {
    [SerializeField]
    FilterSelectDialogTemp _filterSelectDialogTemp;

    protected override void CreateInitialElement( Document document, Action<Edge> onFinish )
    {
      FilterBlockPatternImporter.Import(FilterBlockPatternImporter.FilterType.FS_A1_1_S, onFinish ) ;
    }

    protected override void ShowDialog()
    {
      _filterSelectDialogTemp.Show("Select Filter", "");
      if (_filterSelectDialogTemp.OKClickedHandler == null)
      {
        _filterSelectDialogTemp.OKClickedHandler += (s, e) =>
        {
          FilterBlockPatternImporter.Import(_filterSelectDialogTemp.GetFilterType(), edge =>
          {
            UpdateElement(edge);
            FixPlacement();
          });
        };
      }
    }

    protected override void CloseDialog()
    {
      _filterSelectDialogTemp.gameObject.SetActive(false);
    }
  }
}