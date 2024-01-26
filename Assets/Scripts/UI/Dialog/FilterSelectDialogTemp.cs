using Importer.BlockPattern.Equipment;
using MaterialUI ;
using UnityEngine ;

namespace Chiyoda.UI.Dialog
{
  /// <summary>
  /// TODO 選択ダイアログのデザインが出来上がるまでの暫定ダイアログ
  /// </summary>
  public class FilterSelectDialogTemp : ModalDialog
  {
  
    [SerializeField] GameObject CheckA1;
    [SerializeField] GameObject CheckA2;
    [SerializeField] GameObject CheckA3;
    [SerializeField] GameObject CheckA4;
    [SerializeField] GameObject CheckA5;
    [SerializeField] GameObject CheckA6;
    [SerializeField] GameObject CheckA7;
    [SerializeField] GameObject CheckA8;
   
    void Start()
    {
    }

    void Update()
    {
    }
    
    public FilterBlockPatternImporter.FilterType GetFilterType()
    {
      var type = FilterBlockPatternImporter.FilterType.None;
     
      if (CheckA1.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= FilterBlockPatternImporter.FilterType.FS_A1_1_S;
      }
      if (CheckA2.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= FilterBlockPatternImporter.FilterType.FS_A1_1_G;
      }
      if (CheckA3.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= FilterBlockPatternImporter.FilterType.FS_A2_2_S;
      }
      if (CheckA4.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= FilterBlockPatternImporter.FilterType.FS_A2_2_G;
      }
      if (CheckA5.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= FilterBlockPatternImporter.FilterType.FD_B1_1_S;
      }
      if (CheckA6.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= FilterBlockPatternImporter.FilterType.FD_B1_1_G;
      }
      if (CheckA7.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= FilterBlockPatternImporter.FilterType.FD_B2_2_S;
      }
      if (CheckA8.GetComponent<MaterialCheckbox>().toggle.isOn)
      {
        type |= FilterBlockPatternImporter.FilterType.FD_B2_2_G;
      }
      return type;
    }
  }
}
