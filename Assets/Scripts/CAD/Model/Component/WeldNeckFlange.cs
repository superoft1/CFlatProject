using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.WeldNeckFlange )]
  public class WeldNeckFlange : Component, ILinearComponent
  {
    public enum ConnectPointType
    {
      WeldTerm,
      OutsideTerm,
    }

    public ConnectPoint WeldTermConnectPoint => GetConnectPoint( (int)ConnectPointType.WeldTerm ) ;
    public ConnectPoint OutsideTermConnectPoint => GetConnectPoint( (int)ConnectPointType.OutsideTerm ) ;

    private readonly Memento<double> _length;

    private readonly Memento<double> _insulationThickness;

    public WeldNeckFlange( Document document ) : base( document )
    {
      _length = new Memento<double>(this);
      _insulationThickness = new Memento<double>( this, 0 ) ;

      ComponentName = "WeldNeckFlange";
    
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
      AddNewConnectPoint( (int) ConnectPointType.WeldTerm );
      AddNewConnectPoint( (int) ConnectPointType.OutsideTerm );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as WeldNeckFlange;
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

        var weldTerm = WeldTermConnectPoint ;
        var outsideTerm = OutsideTermConnectPoint ;

        weldTerm.SetPointVector( 0.5 * value * Axis);
        outsideTerm.SetPointVector( -0.5 * value * Axis);

        _length.Value = value;
      }
    }


    public double WeldDiameter
    {
      get
      {
        return WeldTermConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var weldTerm = WeldTermConnectPoint ;

        weldTerm.Diameter = DiameterFactory.FromOutsideMeter( value ) ;
      }
    }

    public double OutsideDiameter
    {
      get
      {
        return OutsideTermConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var outsideTerm = OutsideTermConnectPoint ;

        outsideTerm.Diameter = DiameterFactory.FromOutsideMeter( value ) ;
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
      var size = new Vector3d( Length, WeldDiameter + InsulationThickness * 2, WeldDiameter + InsulationThickness * 2 );
      return new Bounds( (Vector3)Origin, (Vector3)size );
    }
  }
}