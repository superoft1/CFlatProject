using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.CheckValve )]
  public class CheckValve : Component, ILinearComponent
  {
    public enum ConnectPointType
    {
      Term1,
      Term2,
    }
    
    public ConnectPoint Term1ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term1 ) ;
    public ConnectPoint Term2ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term2 ) ;
    private readonly Memento<double> _length;

    public override bool IsEndOfStream => true;

    public CheckValve( Document document ) : base( document )
    {
      _length = new Memento<double>(this);

      ComponentName = "CheckValve";
      
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
      AddNewConnectPoint( (int) ConnectPointType.Term1 );
      AddNewConnectPoint( (int) ConnectPointType.Term2 );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as CheckValve;
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

    public double Diameter
    {
      get { return Term1ConnectPoint.Diameter.OutsideMeter ; }

      set
      {
        var term1 = Term1ConnectPoint ;
        var term2 = Term2ConnectPoint ;

        term1.Diameter = DiameterFactory.FromOutsideMeter( value ) ;
        term2.Diameter = term1.Diameter ;
      }
    }

    public double Length
    {
      get { return _length.Value ; }
      set
      {
        var term1 = Term1ConnectPoint ;
        var term2 = Term2ConnectPoint ;

        term1.SetPointVector( 0.5 * value * Axis) ;
        term2.SetPointVector( -0.5 * value * Axis) ;

        _length.Value = value ;
      }
    }

    public override Bounds GetBounds()
    {
      return new Bounds((Vector3)Origin, new Vector3((float)Length, (float)Diameter, (float)Diameter));
    }
  }
}