using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine;

namespace Chiyoda.CAD.Topology
{
  [Entity(EntityType.Type.NozzleGroup)]
  public abstract class NozzleGroup : Group
  {
    private readonly  Memento<double> _diameter ;

    private Memento<LeafEdge> _equipmentEdge ;
    
    /// <summary>
    /// 機器側のConnectPoint
    /// </summary>
    private Memento<ConnectPoint> _connectPoint ;

    /// <summary>
    /// 機器側のConnectPosition
    /// </summary>
    private readonly Memento<ConnectPosition> _connectPosition ;
    
    public NozzleGroup( Document document ) : base( document )
    {
    }

    public abstract void Create() ;
    
  }

}
