using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.OrificePlate)]
  public class OrificePlate : Component, ILinearComponent
  {
    public enum ConnectPointType
    {
      Term1,
      Term2,
    }

    public ConnectPoint Term1ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term1 ) ;
    public ConnectPoint Term2ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term2 ) ;
    private readonly Memento<double> _length;


    public OrificePlate( Document document ) : base( document )
    {
      _length = new Memento<double>( this ) ;
      ComponentName = "OrificePlate" ;
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.Term1 );
      AddNewConnectPoint( (int) ConnectPointType.Term2 );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as OrificePlate;
      _length.CopyFrom( entity._length.Value );
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      var cp = GetConnectPoint(connectPointNumber);
      var beforeDiameter = cp.Diameter.OutsideMeter;
      var afterDiameter = DiameterFactory.FromNpsMm(newDiameterNpsMm).OutsideMeter;
      Length *= (afterDiameter / beforeDiameter);
      base.ChangeSizeNpsMm(connectPointNumber, newDiameterNpsMm);
    }

    public Vector3d Direction => (Term2 - Term1).normalized ;

    [UI.Property( UI.PropertyCategory.Dimension, "Length", ValueType = UI.ValueType.Length, Visibility = UI.PropertyVisibility.ReadOnly )]
    public double Length
    {
      get => _length.Value ;
      set
      {
        var term1 = Term1ConnectPoint ;
        var term2 = Term2ConnectPoint ;

        term1.SetPointVector( 0.5 * value * Axis);
        term2.SetPointVector( -0.5 * value * Axis);

        _length.Value = value ;
      }
    }

    public Vector3d Term1 => Term1ConnectPoint.Point ;

    public Vector3d Term2 => Term2ConnectPoint.Point ;

    public double Diameter
    {
      get => Term1ConnectPoint.Diameter.OutsideMeter ;

      set
      {
        var term1 = Term1ConnectPoint ;
        var term2 = Term2ConnectPoint ;

        term1.Diameter = DiameterFactory.FromOutsideMeter( value ) ;
        term2.Diameter = term1.Diameter ;
      }
    }

    public override Bounds GetBounds()
    {
      return new Bounds((Vector3)Origin, new Vector3((float)Length, (float)Diameter, (float)Diameter));
    }
  }
}
