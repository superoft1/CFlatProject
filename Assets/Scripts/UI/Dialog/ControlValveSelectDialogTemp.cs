using Importer.BlockPattern.Equipment;
using MaterialUI ;
using UnityEngine ;

namespace Chiyoda.UI.Dialog
{
  /// <summary>
  /// TODO 選択ダイアログのデザインが出来上がるまでの暫定ダイアログ
  /// </summary>
  public class ControlValveSelectDialogTemp : ModalDialog
  {
    [SerializeField] GameObject CheckA1 ;
    [SerializeField] GameObject CheckA2 ;
    [SerializeField] GameObject CheckSelectableValve ;
    [SerializeField] GameObject CheckSV_N1 ;
    [SerializeField] GameObject CheckDV_N2 ;

    void Start()
    {
    }

    void Update()
    {
    }

    public ControlValveBlockPatternImporter.ControlValveType GetCVType()
    {
      var type = ControlValveBlockPatternImporter.ControlValveType.None ;

      if ( CheckA1.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= ControlValveBlockPatternImporter.ControlValveType.CV_A1_TEST;
      }
      if ( CheckA2.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= ControlValveBlockPatternImporter.ControlValveType.CV_A1_TEST;
      }
      if ( CheckSelectableValve.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= ControlValveBlockPatternImporter.ControlValveType.CV_SELECTABLE;
      }
      if ( CheckSV_N1.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= ControlValveBlockPatternImporter.ControlValveType.SV_N1;
      }
      if ( CheckDV_N2.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= ControlValveBlockPatternImporter.ControlValveType.DV_N2;
      }
      return type ;
    }
  }
}
