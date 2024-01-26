using Importer.BlockPattern.Equipment;
using MaterialUI;
using UnityEngine;

namespace Chiyoda.UI.Dialog
{
  /// <summary>
  /// TODO 選択ダイアログのデザインが出来上がるまでの暫定ダイアログ
  /// </summary>
  public class HeatExchangerSelectDialogTemp : ModalDialog
  {

    [SerializeField] GameObject CheckA1;
    //[SerializeField] GameObject CheckA2;
    //[SerializeField] GameObject CheckA3;
    //[SerializeField] GameObject CheckA4;
    //[SerializeField] GameObject CheckA5;
    //[SerializeField] GameObject CheckA6;
    //[SerializeField] GameObject CheckA7;
    //[SerializeField] GameObject CheckA8;
    //[SerializeField] GameObject CheckA9;
    //[SerializeField] GameObject CheckA10;
    //[SerializeField] GameObject CheckA11;
    //[SerializeField] GameObject CheckA12;
    //[SerializeField] GameObject CheckA13;

    void Start()
    {
    }

    void Update()
    {
    }

    public HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType GetHeatExchangerType()
    {
      var type = HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.None;

      if (CheckA1.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_A1_1_G;
      }
      //if (CheckA2.GetComponent<MaterialCheckbox>().toggle.isOn)
      //{
      //  type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_A1_1_S;
      //}
      //if (CheckA3.GetComponent<MaterialCheckbox>().toggle.isOn)
      //{
      //  type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_B1_1_G;
      //}
      //if (CheckA4.GetComponent<MaterialCheckbox>().toggle.isOn)
      //{
      //  type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_B1_1_S;
      //}
      //if (CheckA5.GetComponent<MaterialCheckbox>().toggle.isOn)
      //{
      //  type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_C1_1_G;
      //}
      //if (CheckA6.GetComponent<MaterialCheckbox>().toggle.isOn)
      //{
      //  type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_C1_1_S;
      //}
      //if (CheckA7.GetComponent<MaterialCheckbox>().toggle.isOn)
      //{
      //  type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_C2_1_G;
      //}
      //if (CheckA8.GetComponent<MaterialCheckbox>().toggle.isOn)
      //{
      //  type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_C2_1_S;
      //}
      //if (CheckA9.GetComponent<MaterialCheckbox>().toggle.isOn)
      //{
      //  type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_D1_1_G;
      //}
      //if (CheckA10.GetComponent<MaterialCheckbox>().toggle.isOn)
      //{
      //  type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_D2_1_G;
      //}
      //if (CheckA11.GetComponent<MaterialCheckbox>().toggle.isOn)
      //{
      //  type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_D2_2_G;
      //}
      //if (CheckA12.GetComponent<MaterialCheckbox>().toggle.isOn)
      //{
      //  type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_E1_1_G;
      //}
      //if (CheckA13.GetComponent<MaterialCheckbox>().toggle.isOn)
      //{
      //  type |= HorizontalHeatExchangerBlockPatternImporter.HeatExchangerType.HE_E1_2_G;
      //}

      return type;
    }
  }
}
