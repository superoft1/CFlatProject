using System.ComponentModel;
namespace  Importer.BlockPattern.Equipment.ControlValve
{
  public class ValveSelectItems 
  {
    /// <summary>
    /// Valve 選択セレクタ用定数（Descriptionの文字列は、BlockPattern名(default:idfファイル名)の一部である必要がある）
    /// </summary>
    public enum Items
    {
      None,
      [Description( "SV-N1" )]
      SV_N1,
      [Description( "DV-N2" )]
      DV_N2
    }
  }
}
