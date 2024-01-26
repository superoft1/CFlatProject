using System;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_A1_H_H;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_A1_V_V;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_B1_H_H;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_B1_V_V;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_C1_H_H;
using Importer.BlockPattern.Equipment.SideSideTypePump.SS_C1_V_V;


using UnityEngine ;

public class SideSideTypePumpBlockPatternImporter
  {
    [Flags]
    public enum PumpType
    {
      None = 0,
      CheckSS_A1_H_H = 1 << 1,
      CheckSS_A1_V_V = 1 << 2,
      CheckSS_B1_H_H = 1 << 3,
      CheckSS_B1_V_V = 1 << 4,
      CheckSS_C1_H_H = 1 << 5,
      CheckSS_C1_V_V = 1 << 6,
    }

    public static void Import(PumpType type, Action<Edge> onFinish)
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

    if (type.HasFlag(PumpType.CheckSS_A1_H_H))
    {
      new SideSideTypePumpSS_A1_H_H(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckSS_A1_V_V))
    {
      new SideSideTypePumpSS_A1_V_V(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckSS_B1_H_H))
    {
      new SideSideTypePumpSS_B1_H_H(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckSS_B1_V_V))
    {
      new SideSideTypePumpSS_B1_V_V(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckSS_C1_H_H))
    {
      new SideSideTypePumpSS_C1_H_H(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckSS_C1_V_V))
    {
      new SideSideTypePumpSS_C1_V_V(curDoc).Create(onFinish);
    }

  }
}
