using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_A1_S_G_N ;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_A1_S_S_N;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_A2_S_S_C;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_A2_S_S_O;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_B1_S_G_N;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_B1_S_S_N;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_B2_S_S_C;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_B2_S_S_O;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_C1_S_G_N;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_C1_S_S_N;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_C2_S_S_C;
using Importer.BlockPattern.Equipment.TopTopTypePump.TT_C2_S_S_O;

using UnityEngine ;


public class TopTopTypePumpBlockPatternImporter
  {
    [Flags]
    public enum PumpType
    {
      None = 0,
      CheckTT_A1_S_G_N = 1 << 1,
      CheckTT_A1_S_S_N = 1 << 2,
      CheckTT_A2_S_S_C = 1 << 3,
      CheckTT_A2_S_S_O = 1 << 4,
      CheckTT_B1_S_G_N = 1 << 5,
      CheckTT_B1_S_S_N = 1 << 6,
      CheckTT_B2_S_S_C = 1 << 7,
      CheckTT_B2_S_S_O = 1 << 8,
      CheckTT_C1_S_G_N = 1 << 9,
      CheckTT_C1_S_S_N = 1 << 10,
      CheckTT_C2_S_S_C = 1 << 11,
      CheckTT_C2_S_S_O = 1 << 12,
    }

    public static void Import( PumpType type, Action<Edge> onFinish )
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew() ;

    if ( type.HasFlag( PumpType.CheckTT_A1_S_G_N ) )
    {
      new TopTopTypePumpTT_A1_S_G_N( curDoc ).Create( onFinish ) ;
    }
    if (type.HasFlag(PumpType.CheckTT_A1_S_S_N))
    {
      new TopTopTypePumpTT_A1_S_S_N(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckTT_A2_S_S_C))
    {
      new TopTopTypePumpTT_A2_S_S_C(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckTT_A2_S_S_O))
    {
      new TopTopTypePumpTT_A2_S_S_O(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckTT_B1_S_G_N))
    {
      new TopTopTypePumpTT_B1_S_G_N(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckTT_B1_S_S_N))
    {
      new TopTopTypePumpTT_B1_S_S_N(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckTT_B2_S_S_C))
    {
      new TopTopTypePumpTT_B2_S_S_C(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckTT_B2_S_S_O))
    {
      new TopTopTypePumpTT_B2_S_S_O(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckTT_C1_S_G_N))
    {
      new TopTopTypePumpTT_C1_S_G_N(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckTT_C1_S_S_N))
    {
      new TopTopTypePumpTT_C1_S_S_N(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckTT_C2_S_S_C))
    {
      new TopTopTypePumpTT_C2_S_S_C(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckTT_C2_S_S_O))
    {
      new TopTopTypePumpTT_C2_S_S_O(curDoc).Create(onFinish);
    }
  }
}
