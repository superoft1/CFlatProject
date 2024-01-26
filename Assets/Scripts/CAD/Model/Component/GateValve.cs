using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.GateValve )]
  public class GateValve : Component, ILinearComponent
  {
    public enum ConnectPointType
    {
      Weld1Term,
      Weld2Term,
    }

    public ConnectPoint Weld1TermConnectPoint => GetConnectPoint( (int)ConnectPointType.Weld1Term ) ;
    public ConnectPoint Weld2TermConnectPoint => GetConnectPoint( (int)ConnectPointType.Weld2Term ) ;

    private readonly Memento<double> _length;

    private readonly Memento<double> _insulationThickness;

    public GateValve( Document document ) : base( document )
    {
      _length = new Memento<double>(this);
      _insulationThickness = new Memento<double>( this, 0 ) ;

      ComponentName = "GateValve";
    
      RegisterDelayedProperty(
        "Length",
        PropertyType.Length,
        () => Length,
        value => Length = value,
        prop => _length.AfterNewlyValueChanged += ( sender, e ) => prop.ForceTriggerChange() );
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int)ConnectPointType.Weld1Term ) ;
      AddNewConnectPoint( (int)ConnectPointType.Weld2Term ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as GateValve;
      _length.CopyFrom( entity._length.Value );
      _insulationThickness.CopyFrom( entity._insulationThickness.Value ) ;
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      var cp = GetConnectPoint(connectPointNumber);
      var beforeDiameter = cp.Diameter.OutsideMeter;
      var afterDiameter = DiameterFactory.FromNpsMm(newDiameterNpsMm).OutsideMeter;
      Length *= (afterDiameter / beforeDiameter);
      base.ChangeSizeNpsMm(connectPointNumber, newDiameterNpsMm);
    }

    public double Length
    {
      get
      {
        return _length.Value;
      }
      set
      {
        var weld1Term = Weld1TermConnectPoint ;
        var weld2Term = Weld2TermConnectPoint ;

        weld1Term.SetPointVector( 0.5 * value * Axis);
        weld2Term.SetPointVector( -0.5 * value * Axis);

        _length.Value = value;
      }
    }

    public double Diameter
    {
      get
      {
        return Weld1TermConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var weld1Term = Weld1TermConnectPoint ;
        var weld2Term = Weld2TermConnectPoint ;

        weld1Term.Diameter = DiameterFactory.FromOutsideMeter( value );
        weld2Term.Diameter = weld1Term.Diameter ;
      }
    }

    [UI.Property( UI.PropertyCategory.Insulation, "Thickness", ValueType = UI.ValueType.Length, Visibility = UI.PropertyVisibility.ReadOnly )]
    public double InsulationThickness
    {
      get { return _insulationThickness.Value; }
      set { _insulationThickness.Value = value < 0.0 ? 0.0 : value; }
    }

    [UI.Property( UI.PropertyCategory.Insulation, "Code", ValueType = UI.ValueType.Text, Visibility = UI.PropertyVisibility.ReadOnly )]
    public string InsulationCode { get; set; }

    public override Bounds GetBounds()
    {
      var size = new Vector3d( Length, Diameter + InsulationThickness * 2, Diameter + InsulationThickness * 2 );
      return new Bounds( (Vector3)Origin, (Vector3)size );
    }
  }
}