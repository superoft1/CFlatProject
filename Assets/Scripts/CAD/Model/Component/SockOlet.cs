using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.SockOlet )]
  public class SockOlet : Component
  {
    public enum ConnectPointType
    {
      MainTerm,
      BranchTerm,
    }

    public ConnectPoint MainTermConnectPoint => GetConnectPoint( (int)ConnectPointType.MainTerm ) ;
    public ConnectPoint BranchTermConnectPoint => GetConnectPoint( (int)ConnectPointType.BranchTerm ) ;
    private readonly Memento<double> _length;

    public SockOlet( Document document ) : base( document )
    {
      _length = new Memento<double>( this ) ;

      ComponentName = "SockOlet" ;
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.MainTerm );
      AddNewConnectPoint( (int) ConnectPointType.BranchTerm );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as SockOlet;
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

    public double Length
    {
      get
      {
        return _length.Value;
      }

      set
      {
        var mainTerm = MainTermConnectPoint ;
        var branchTerm = BranchTermConnectPoint ;

        mainTerm.SetPointVector( -0.5 * value * Axis);
        branchTerm.SetPointVector( 0.5 * value * Axis);

        _length.Value = value;
      }
    }

    public double Diameter
    {
      get
      {
        return MainTermConnectPoint.Diameter.OutsideMeter;
      }

      set
      {
        var mainTerm = MainTermConnectPoint ;
        var branchTerm = BranchTermConnectPoint ;

        mainTerm.Diameter = DiameterFactory.FromOutsideMeter( value ) ;
        branchTerm.Diameter = mainTerm.Diameter ;
      }
    }

    public override Bounds GetBounds()
    {
      return new Bounds((Vector3)Origin, new Vector3((float)Length, (float)Diameter, (float)Diameter));
    }
  }
}