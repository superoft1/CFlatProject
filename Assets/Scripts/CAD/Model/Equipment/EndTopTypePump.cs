using System;
using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.EndTopTypePump )]
  public class EndTopTypePump : HorizontalPump
  {
    public EndTopTypePump( Document document ) : base( document )
    {
      EquipmentName = "EndTopTypePump";
    }
  }
}
