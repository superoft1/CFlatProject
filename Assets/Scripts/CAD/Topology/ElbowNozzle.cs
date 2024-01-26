using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine;

namespace Chiyoda.CAD.Topology
{
  [Entity(EntityType.Type.ElbowNozzle)]
  public class ElbowNozzle : NozzleGroup
  {
    private readonly  Memento<bool> _hasFlange ;
    private readonly  Memento<double> _firstStraightLength ;
    private readonly  Memento<double> _elbowAngle ;
    private readonly  Memento<double> _secondStraightLength ;
    
    public ElbowNozzle( Document document ) : base( document )
    {
      this._hasFlange = CreateMementoAndSetupValueEvents( false ) ;
      this._firstStraightLength = CreateMementoAndSetupValueEvents( 0.0 ) ;
      this._elbowAngle = CreateMementoAndSetupValueEvents( 0.0 ) ;
      this._secondStraightLength = CreateMementoAndSetupValueEvents( 0.0 ) ;
    }
    
    public override void Create()
    {
      
    }
    
  }

}