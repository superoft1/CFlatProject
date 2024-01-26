using System.Collections.Generic ;
using Chiyoda.CAD.Model ;

namespace Importer.BlockPattern.Equipment.ControlValve
{
  public class SingleBlockPatternIndexInfo
  {
    /// <summary>
    /// グループインデックス
    /// </summary>
    public int [] MainGroupIndices;
    public int [][] OriginLeafEdgeIndices; //  座標基準となるLeafEdge を指定する（Equipmentがないため）
    /// <summary>
    /// 基本インデックス
    /// </summary>
    public enum BasicIndexType
    {
      InletTube,
      ControlValveA,
      ControlValveB,
      OutletTube,
    }
    public Dictionary<BasicIndexType, int> BasicIndexTypeValue = new Dictionary<BasicIndexType, int>() ;
  }
}
