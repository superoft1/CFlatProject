using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.BlindFlange )]
  public class BlindFlange : Component
  {
    public enum ConnectPointType
    {
      WeldTerm,
    }

    public ConnectPoint WeldTermConnectPoint => GetConnectPoint( (int)ConnectPointType.WeldTerm ) ;

    private readonly Memento<double> _length;

    private readonly Memento<double> _insulationThickness;

    public BlindFlange( Document document ) : base( document )
    {
      _length = new Memento<double>(this);
      _insulationThickness = new Memento<double>( this, 0 ) ;

      ComponentName = "BlindFlange";
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.WeldTerm );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as BlindFlange;
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
        _length.Value = value;

        WeldTermConnectPoint.SetPointVector( 0.5 * value * Axis ) ;
      }
    }

    public double Diameter
    {
      get
      {
        return WeldTermConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var weldTerm = WeldTermConnectPoint ;
        weldTerm.Diameter = DiameterFactory.FromOutsideMeter( value ); ;
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