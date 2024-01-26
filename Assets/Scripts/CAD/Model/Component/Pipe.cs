using System;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Manager ;
using Chiyoda.CAD.Topology;
using MaterialUI ;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.Pipe )]
  public class Pipe : Component, IParallelPipe, ILinearComponent
  {
    public enum ConnectPointType
    {
      Term1,
      Term2,
    }

    public ConnectPoint Term1ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term1 ) ;
    public ConnectPoint Term2ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term2 ) ;

    private readonly Memento<double> _preferredLength;
    private readonly Memento<double> _flexRatio;
    private readonly Memento<double> _minimumLengthWithoutOletRadius;
    private readonly Memento<double> _minimumLengthRatioByDiameter;

    private readonly Memento<double> _insulationThickness;

    public Pipe( Document document ) : base( document )
    {
      _preferredLength = CreateMementoAndSetupValueEvents( 0.0 );
      _flexRatio = new Memento<double>( this, 0.0 );
      _minimumLengthWithoutOletRadius = CreateMementoAndSetupValueEvents( 0.0 );
      _insulationThickness = new Memento<double>( this, 0.0 ) ;
      _minimumLengthRatioByDiameter = new Memento<double>( this, 0.0 ) ;

      ComponentName = "Pipe";

      RegisterDelayedProperty(
        "Diameter",
        PropertyType.DiameterRange,
        () => Diameter,
        value => Diameter = DiameterFactory.FromNpsMm(value).OutsideMeter,
        prop => Term1ConnectPoint.AfterNewlyDiameterChanged += ( sender, e ) => prop.ForceTriggerChange()  );

      RegisterDelayedProperty(
        "MinLength",
        PropertyType.Length,
        this.GetMinimumLength,
        this.SetMinimumLength,
        prop => _minimumLengthWithoutOletRadius.AfterNewlyValueChanged += ( sender, e ) => prop.ForceTriggerChange() );

      RegisterDelayedProperty(
        "Length",
        PropertyType.Length,
        () => Length,
        value => Length = value,
        prop => _preferredLength.AfterNewlyValueChanged += ( sender, e ) =>
        {
          if ( e.OldValue <= _minimumLengthWithoutOletRadius.Value && e.NewValue <= _minimumLengthWithoutOletRadius.Value ) return ;
          prop.ForceTriggerChange() ;
        } );
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects() ;

      AddNewConnectPoint( (int) ConnectPointType.Term1 );
      AddNewConnectPoint( (int) ConnectPointType.Term2 );
    }

    protected internal override void RegisterNonMementoMembersFromDefaultObjects()
    {
      base.RegisterNonMementoMembersFromDefaultObjects() ;

      _minimumLengthRatioByDiameter.AfterNewlyValueChanged += ( sender, e ) => UpdateMinimumLength() ;
      Term1ConnectPoint.AfterNewlyDiameterChanged += ( sender, e ) =>
      {
        if ( 0 < _minimumLengthRatioByDiameter.Value ) UpdateMinimumLength() ;
      } ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as Pipe;
      _minimumLengthRatioByDiameter.CopyFrom( entity._minimumLengthRatioByDiameter.Value );  // UpdateMinimumLengthが発生するので最初に設定
      _preferredLength.CopyFrom( entity._preferredLength.Value );
      _flexRatio.CopyFrom( entity._flexRatio.Value );
      _minimumLengthWithoutOletRadius.CopyFrom( entity._minimumLengthWithoutOletRadius.Value );
      _insulationThickness.CopyFrom( entity._insulationThickness.Value ) ;
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      var cp = GetConnectPoint(connectPointNumber);
      var beforeDiameter = cp.Diameter.OutsideMeter;
      var afterDiameter = DiameterFactory.FromNpsMm(newDiameterNpsMm).OutsideMeter;
      PreferredLength *= (afterDiameter / beforeDiameter);
      base.ChangeSizeNpsMm(connectPointNumber, newDiameterNpsMm);
    }

    private void UpdateMinimumLength()
    {
      var ratio = _minimumLengthRatioByDiameter.Value ;
      if ( ratio <= 0 ) return ;

      MinimumLengthWithoutOletRadius = this.WeldMinDistance().Millimeters() * _minimumLengthRatioByDiameter.Value ;
    }

    public double FlexRatio // FIXME: Pipeのメンバーではなくす
    {
      get => _flexRatio.Value ;
      set => _flexRatio.Value = value ;
    }

    public double MinimumLengthWithoutOletRadius // FIXME: Pipeのメンバーではなくす
    {
      get => _minimumLengthWithoutOletRadius.Value ;
      set
      {
        _minimumLengthWithoutOletRadius.Value = value ;
        Length = PreferredLength ;
      }
    }

    public double MinimumLengthRatioByDiameter
    {
      get => _minimumLengthRatioByDiameter.Value ;
      set => _minimumLengthRatioByDiameter.Value = value ;
    }

    public double PreferredLength
    {
      get => _preferredLength.Value ;
      set => Length = value ;
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Length", ValueType = UI.ValueType.Length, Visibility = UI.PropertyVisibility.ReadOnly )]
    public double Length
    {
      get => Math.Max( _preferredLength.Value, this.GetMinimumLength() ) ;
      set
      {
        var term1 = Term1ConnectPoint ;
        var term2 = Term2ConnectPoint ;

        var minLength = this.GetMinimumLength() ;
        
        var trueLength = Math.Max( value, minLength );
        term1.SetPointVector( -Axis * trueLength * 0.5);
        term2.SetPointVector( Axis * trueLength * 0.5);

        _preferredLength.Value = value;
      }
    }
    [UI.Property(UI.PropertyCategory.Dimension, "Diameter", ValueType = UI.ValueType.DiameterRange, Visibility = UI.PropertyVisibility.ReadOnly)]
    public int DiameterNpsMm
    {
      get
      {
        return DiameterFactory.FromOutsideMeter(Diameter).NpsMm;
      }

      set
      {
        Diameter = DiameterFactory.FromNpsMm(value).OutsideMeter;
      }
    }

    public double Diameter
    {
      get => Term1ConnectPoint.Diameter.OutsideMeter ;

      set
      {
        var term1 = Term1ConnectPoint ;
        var term2 = Term2ConnectPoint ;

        term1.Diameter = DiameterFactory.FromOutsideMeter(value);
        term2.Diameter = term1.Diameter ;
      }
    }
    
    public Diameter DiameterObj => Term1ConnectPoint.Diameter ;

    [UI.Property( UI.PropertyCategory.Insulation, "Thickness", ValueType = UI.ValueType.Length, Visibility = UI.PropertyVisibility.ReadOnly )]
    public double InsulationThickness
    {
      get => _insulationThickness.Value ;
      set => _insulationThickness.Value = Math.Max( 0.0, value ) ;
    }

    [UI.Property( UI.PropertyCategory.Insulation, "Code", ValueType = UI.ValueType.Text, Visibility = UI.PropertyVisibility.ReadOnly )]
    public string InsulationCode { get; set; }

    double IParallelPipe.ParallelDiameter => Diameter;
    IEnumerable<(double, double)> IParallelPipe.ParallelRanges => new[] { (-Length / 2, Length / 2) };
    Vector3d IParallelPipe.LocalParallelDirection => Vector3d.right;

    public override Bounds GetBounds()
    {
      var size = new Vector3d( Length, Diameter + InsulationThickness * 2, Diameter + InsulationThickness * 2 );
      return new Bounds( (Vector3)Origin, (Vector3)size );
    }

    public static double GetDefaultPipeMinLength( double diameter )
    {
      var dia = DiameterFactory.FromOutsideMeter( diameter ) ;
      return dia.WeldMinDistance() ;
    }
  }
}
