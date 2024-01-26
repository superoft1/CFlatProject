using Importer.BlockPattern.Equipment.EndTopTypePump ;
using MaterialUI ;
using UnityEngine ;

namespace Chiyoda.UI.Dialog
{
  /// <summary>
  /// TODO 選択ダイアログのデザインが出来上がるまでの暫定ダイアログ
  /// </summary>
  public class EndTopTypePumpSelectDialogTemp : ModalDialog
  {
    [SerializeField] GameObject CheckPump8 ;
    [SerializeField] GameObject CheckA1 ;
    [SerializeField] GameObject CheckA2 ;
    [SerializeField] GameObject CheckA3;
    [SerializeField] GameObject CheckA4;
    [SerializeField] GameObject CheckA5;
    [SerializeField] GameObject CheckA6;
    [SerializeField] GameObject CheckA7;
    [SerializeField] GameObject CheckA8;
    [SerializeField] GameObject CheckA9;
    [SerializeField] GameObject CheckA10;
    [SerializeField] GameObject CheckA11;
       
        void Start()
    {
    }

    void Update()
    {
    }


    public EndTopTypePumpBlockPatternImporter.PumpType GetPumpType()
    {
      var type = EndTopTypePumpBlockPatternImporter.PumpType.None ;

      if ( CheckPump8.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= EndTopTypePumpBlockPatternImporter.PumpType.Pump8 ;
      }
      if ( CheckA1.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= EndTopTypePumpBlockPatternImporter.PumpType.CheckET_A1_V_H_S;
      }
      if ( CheckA2.GetComponent<MaterialCheckbox>().toggle.isOn ) {
        type |= EndTopTypePumpBlockPatternImporter.PumpType.CheckET_A1_H_H_L;
      }
      if ( CheckA3.GetComponent<MaterialCheckbox>().toggle.isOn)  {
        type |= EndTopTypePumpBlockPatternImporter.PumpType.CheckET_A1_V_V_L;
      }
      if (CheckA4.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= EndTopTypePumpBlockPatternImporter.PumpType.CheckET_A2_H_H_L;
      }
      if (CheckA5.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= EndTopTypePumpBlockPatternImporter.PumpType.CheckET_B1_V_H_S;
      }
      if (CheckA6.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= EndTopTypePumpBlockPatternImporter.PumpType.CheckET_B1_H_H_L;
      }
      if (CheckA7.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= EndTopTypePumpBlockPatternImporter.PumpType.CheckET_B1_V_H_L;
      }
      if (CheckA8.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= EndTopTypePumpBlockPatternImporter.PumpType.CheckET_B2_H_H_L;
      }
      if (CheckA9.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= EndTopTypePumpBlockPatternImporter.PumpType.CheckET_C1_H_H_L;
      }
      if (CheckA10.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= EndTopTypePumpBlockPatternImporter.PumpType.CheckET_C1_V_V_L;
      }
      if (CheckA11.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= EndTopTypePumpBlockPatternImporter.PumpType.CheckET_C2_H_H_L;
      }
      return type ;
    }
  }
}
