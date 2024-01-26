using Importer.BlockPattern.Equipment;
using MaterialUI ;
using UnityEngine ;

namespace Chiyoda.UI.Dialog
{
  /// <summary>
  /// TODO 選択ダイアログのデザインが出来上がるまでの暫定ダイアログ
  /// </summary>
  public class PressureReliefValveSelectDialogTemp : ModalDialog
  {
    [SerializeField] GameObject CheckA1 ;
    [SerializeField] GameObject CheckA2 ;
    [SerializeField] GameObject CheckA3 ;

    void Start()
    {
    }

    void Update()
    {
    }

    public PressureReliefValveBlockPatternImporter.PressureReliefValveType GetPRVType()
    {
      var type = PressureReliefValveBlockPatternImporter.PressureReliefValveType.None ;

      if ( CheckA1.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= PressureReliefValveBlockPatternImporter.PressureReliefValveType.PRV_C1_V;
      }
      if ( CheckA2.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= PressureReliefValveBlockPatternImporter.PressureReliefValveType.PRV_C1_H;
      }
      if ( CheckA3.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= PressureReliefValveBlockPatternImporter.PressureReliefValveType.PRV_C1_U;
      }
      return type ;
    }
  }
}
