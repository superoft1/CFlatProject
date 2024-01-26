using Importer.BlockPattern.Equipment.SideSideTypePump ;
using MaterialUI ;
using UnityEngine ;

namespace Chiyoda.UI.Dialog
{
  /// <summary>
  /// TODO 選択ダイアログのデザインが出来上がるまでの暫定ダイアログ
  /// </summary>
  public class SideSideTypePumpSelectDialogTemp : ModalDialog
  {
    [SerializeField] GameObject CheckA1 ;
    [SerializeField] GameObject CheckA2 ;
    [SerializeField] GameObject CheckA3;
    [SerializeField] GameObject CheckA4;
    [SerializeField] GameObject CheckA5;
    [SerializeField] GameObject CheckA6;


    void Start()
    {
    }

    void Update()
    {
    }


    public SideSideTypePumpBlockPatternImporter.PumpType GetPumpType()
    {
      var type = SideSideTypePumpBlockPatternImporter.PumpType.None ;
      if (CheckA1.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= SideSideTypePumpBlockPatternImporter.PumpType.CheckSS_A1_H_H ;
      }
      if ( CheckA2.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= SideSideTypePumpBlockPatternImporter.PumpType.CheckSS_A1_V_V ;
      }
      if (CheckA3.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= SideSideTypePumpBlockPatternImporter.PumpType.CheckSS_B1_H_H;
      }
      if (CheckA4.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= SideSideTypePumpBlockPatternImporter.PumpType.CheckSS_B1_V_V;
      }
      if (CheckA5.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= SideSideTypePumpBlockPatternImporter.PumpType.CheckSS_C1_H_H;
      }
      if (CheckA6.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= SideSideTypePumpBlockPatternImporter.PumpType.CheckSS_C1_V_V;
      }

      return type ;
    }
  }
}
