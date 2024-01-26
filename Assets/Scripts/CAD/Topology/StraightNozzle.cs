using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine;


namespace Chiyoda.CAD.Topology
{
  [Entity(EntityType.Type.StraightNozzle)]
  public class StraightNozzle : NozzleGroup
  {
    private readonly  Memento<bool> _hasFlange ;
    private readonly  Memento<double> _length ;
    
    public StraightNozzle( Document document ) : base( document )
    {
      this._hasFlange = CreateMementoAndSetupValueEvents(false ) ;
      this._length = CreateMementoAndSetupValueEvents( 0.0 ) ;
    }

    public override void Create()
    {
      
    }
    
  }

}