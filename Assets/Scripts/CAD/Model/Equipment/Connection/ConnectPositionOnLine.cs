using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core ;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.ConnectPositionOnLine)]
  public class ConnectPositionOnLine : ConnectPosition
  {
    private readonly Memento<double> _distanceFromBase;
    
    public ConnectPositionOnLine( Document document ) : base( document )
    {
      _distanceFromBase = CreateMementoAndSetupValueEvents( 0.0 ) ;
    }
    
    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as ConnectPositionOnLine;
      _distanceFromBase.CopyFrom( entity._distanceFromBase.Value );
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DistanceFromBase", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DistanceFromBase
    {
      get
      {
        return _distanceFromBase.Value;
      }
      set
      {
        _distanceFromBase.Value = value;
      }
    }

  }

}