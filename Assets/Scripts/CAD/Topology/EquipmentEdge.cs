using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine;

[Entity(EntityType.Type.EquipmentEdge)]
public class EquipmentEdge : BlockEdge
{
  
    public EquipmentEdge( Document document ) : base( document )
    {
    }

    protected override void ReleaseAllEdgesForDisassemble()
    {
    }

    public List<LeafEdge> EquipmentLeafEdges
    {
      get
      {
        return null ;
      }
    }

    public List<Equipment> Equipments
    {
      get
      {
        return null;  
      }
    }

    public List<Nozzle> Nozzles
    {
      get
      {
        return null ;
      }
    }
    
}
