using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Importer.BlockPattern.Equipment.EndTopTypePump.Data8 ;
using Importer.BlockPattern.Equipment.EndTopTypePump.ET_A1_V_H_S ;
using Importer.Equipment ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment.EndTopTypePump
{
  public static class EndTopTypePumpBlockPatternImporter
  {
    [Flags]
    public enum PumpType
    {
      None = 0,
      Pump8 = 1,
      CheckET_A1_V_H_S = 1 << 1,
      CheckET_A1_H_H_L = 1 << 2,
    }

    public static void Import( PumpType type, Action<Edge> onFinish )
    {
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew() ;

      if ( type.HasFlag( PumpType.Pump8 ) ) {
        CreateEndTopTypePumpData8.Create( onFinish ) ;
      }

      if ( type.HasFlag( PumpType.CheckET_A1_V_H_S ) ) {
        new EndTopTypePumpET_A1_V_H_S( curDoc ).Create( onFinish ) ;
      }

      if ( type.HasFlag( PumpType.CheckET_A1_H_H_L ) ) {
        Debug.Log( "PumpType.CheckA2 not implemented." ) ;
      }
    }
  }
}