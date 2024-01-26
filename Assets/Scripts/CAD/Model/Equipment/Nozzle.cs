using System;
using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Manager ;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.Nozzle )]
  public class Nozzle : Entity, IDiameterRange
  {
    public enum Type
    {
      Suction,
      Discharge
    }

    private readonly Memento<Type> _nozzleType;
    private readonly Memento<Diameter> _diameter;
    private readonly Memento<double> _length;
    private readonly Memento<int> _nozzleNumber ;
    private readonly Memento<DiameterRange> _diameterRange;

    DiameterRange IDiameterRange.DiameterRange => _diameterRange.Value;

    void IDiameterRange.ChangeRange(int minDiameterNpsMm, int maxDiameterNpsMm)
    {
      _diameterRange.Value.ChangeRange(minDiameterNpsMm, maxDiameterNpsMm);
    }

    public Nozzle( Document document ) : base( document )
    {
      _nozzleType = CreateMementoAndSetupValueEvents( Type.Suction ) ;
      _diameter = CreateMementoAndSetupValueEvents(DiameterFactory.FromOutsideMeter(0.0) ) ;
      _length = CreateMementoAndSetupValueEvents( 0.0 ) ;
      _nozzleNumber = new Memento<int>( this ) ;
      _diameterRange = CreateMementoAndSetupValueEvents(new DiameterRange(40, 500));
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as Nozzle;
      _nozzleType.CopyFrom( entity._nozzleType.Value );
      _diameter.CopyFrom( entity._diameter.Value );
      _length.CopyFrom( entity._length.Value );
      _nozzleNumber.CopyFrom( entity._nozzleNumber.Value ) ;
      _diameterRange.CopyFrom(entity._diameterRange.Value);
    }
    public Equipment Instrument => Parent as Equipment ;

    public int NozzleNumber
    {
      get => _nozzleNumber.Value ;
      internal set => _nozzleNumber.Value = value ;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "NozzleType", ValueType = UI.ValueType.Select, Visibility = UI.PropertyVisibility.Editable)]
    public Type NozzleType
    {
      get
      {
        return _nozzleType.Value;
      }

      set
      {
        _nozzleType.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "Diameter", ValueType = UI.ValueType.DiameterRange, Visibility = UI.PropertyVisibility.Editable)]
    public int DiameterNpsMm
    {
      get
      {
        return Diameter.NpsMm;
      }

      set
      {
        Diameter = DiameterFactory.FromNpsMm(value);
      }
    }

    public Diameter Diameter
    {
      get
      {
        return _diameter.Value;
      }

      set
      {
        _diameter.Value = value;
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "Length", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double Length
    {
      get
      {
        return _length.Value;
      }

      set
      {
        _length.Value = value;
      }
    }

    public override Bounds? GetGlobalBounds()
    {
      BodyMap.Instance.TryGetBody( this, out var body ) ;

      return body?.GetGlobalBounds() ;
    }
  }

}