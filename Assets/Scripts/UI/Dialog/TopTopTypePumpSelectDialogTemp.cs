using Importer.BlockPattern.Equipment.TopTopTypePump ;
using MaterialUI ;
using UnityEngine ;

namespace Chiyoda.UI.Dialog
{
  /// <summary>
  /// TODO 選択ダイアログのデザインが出来上がるまでの暫定ダイアログ
  /// </summary>
  public class TopTopTypePumpSelectDialogTemp : ModalDialog
  {
  
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
    [SerializeField] GameObject CheckA12;
   
    void Start()
    {
    }

    void Update()
    {
    }
    
    public TopTopTypePumpBlockPatternImporter.PumpType GetPumpType()
    {
      var type = TopTopTypePumpBlockPatternImporter.PumpType.None;
     
      if (CheckA1.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_A1_S_G_N;
      }
      if (CheckA2.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_A1_S_S_N;
      }
      if (CheckA3.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_A2_S_S_C;
      }
      if (CheckA4.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_A2_S_S_O;
      }
      if (CheckA5.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_B1_S_G_N;
      }
      if (CheckA6.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_B1_S_S_N;
      }
      if (CheckA7.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_B2_S_S_C;
      }
      if (CheckA8.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_B2_S_S_O;
      }
      if (CheckA9.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_C1_S_G_N;
      }
      if (CheckA10.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_C1_S_S_N;
      }
      if (CheckA11.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_C2_S_S_C;
      }
      if (CheckA12.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= TopTopTypePumpBlockPatternImporter.PumpType.CheckTT_C2_S_S_O;
      }
      return type;
    }
  }
}
