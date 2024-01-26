using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.EndTopTypePump.Data8 ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_A1_V_H_S;

using Importer.BlockPattern.Equipment.EndTopTypePump.ET_A1_V_V_L;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_A2_H_H_L;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_B1_V_H_S;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_B1_H_H_L;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_B1_V_H_L;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_B2_H_H_L;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_C1_H_H_L;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_C1_V_V_L;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_C2_H_H_L;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_A1_H_H_L;

using UnityEngine ;

public class EndTopTypePumpBlockPatternImporter
{
  [Flags]
  public enum PumpType
  {
    None = 0,
    Pump8 = 1,
    CheckET_A1_V_H_S = 1 << 1,
    CheckET_A1_H_H_L = 1 << 2,
    CheckET_A1_V_V_L = 1 << 3,
    CheckET_A2_H_H_L = 1 << 4,
    CheckET_B1_V_H_S = 1 << 5,
    CheckET_B1_H_H_L = 1 << 6,
    CheckET_B1_V_H_L = 1 << 7,
    CheckET_B2_H_H_L = 1 << 8,
    CheckET_C1_H_H_L = 1 << 9,
    CheckET_C1_V_V_L = 1 << 10,
    CheckET_C2_H_H_L = 1 << 11,
  }


  public static void Import( PumpType type, Action<Edge> onFinish )
  {
    var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

    if ( type.HasFlag( PumpType.Pump8 ) ) {
      CreateEndTopTypePumpData8.Create( onFinish ) ;
    }

    if ( type.HasFlag( PumpType.CheckET_A1_V_H_S ) ) {
      new EndTopTypePumpET_A1_V_H_S(curDoc).Create( onFinish ) ;
    }

    if (type.HasFlag(PumpType.CheckET_A1_H_H_L))
    {
      new EndTopTypePumpET_A1_H_H_L(curDoc).Create(onFinish);
      //      Debug.Log("PumpType.CheckA2 not implemented.");

    }
    if (type.HasFlag(PumpType.CheckET_A1_V_V_L))
    {
      new EndTopTypePumpET_A1_V_V_L(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckET_A2_H_H_L))
    {
      new EndTopTypePumpET_A2_H_H_L(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckET_B1_V_H_S))
    {
      new EndTopTypePumpET_B1_V_H_S(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckET_B1_H_H_L))
    {
      new EndTopTypePumpET_B1_H_H_L(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckET_B1_V_H_L))
    {
      new EndTopTypePumpET_B1_V_H_L(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckET_B2_H_H_L))
    {
      new EndTopTypePumpET_B2_H_H_L(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckET_C1_H_H_L))
    {
      new EndTopTypePumpET_C1_H_H_L(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckET_C1_V_V_L))
    {
      new EndTopTypePumpET_C1_V_V_L(curDoc).Create(onFinish);
    }
    if (type.HasFlag(PumpType.CheckET_C2_H_H_L))
    {
      new EndTopTypePumpET_C2_H_H_L(curDoc).Create(onFinish);
    }

  }
}