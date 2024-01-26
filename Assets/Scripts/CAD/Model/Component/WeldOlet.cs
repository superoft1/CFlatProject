using System ;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.WeldOlet )]
  public class WeldOlet : Component
  {
    public enum ConnectPointType
    {
      MainTerm1,
      MainTerm2,
      BranchTerm,
    }

    public ConnectPoint MainTerm1ConnectPoint => GetConnectPoint( (int)ConnectPointType.MainTerm1 ) ;
    public ConnectPoint MainTerm2ConnectPoint => GetConnectPoint( (int)ConnectPointType.MainTerm2 ) ;
    public ConnectPoint BranchTermConnectPoint => GetConnectPoint( (int)ConnectPointType.BranchTerm ) ;
    private readonly Memento<double> _length;

    public WeldOlet( Document document ) : base( document )
    {
      _length = new Memento<double>( this ) ;

      ComponentName = "WeldOlet" ;

      RegisterDelayedProperty(
        "Diameter",
        PropertyType.DiameterRange,
        () => Diameter,
        value => Diameter = value,
        prop => BranchTermConnectPoint.AfterNewlyDiameterChanged += ( sender, e ) => prop.ForceTriggerChange() ) ;
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.MainTerm1 );
      AddNewConnectPoint( (int) ConnectPointType.MainTerm2 );
      AddNewConnectPoint( (int) ConnectPointType.BranchTerm );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as WeldOlet;
      _length.CopyFrom( entity._length.Value );
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      switch ((ConnectPointType)connectPointNumber)
      {
        case ConnectPointType.MainTerm1:
        case ConnectPointType.MainTerm2:
          {
            var afterDiameter = DiameterFactory.FromNpsMm( newDiameterNpsMm ) ;
            var diffDiameter = afterDiameter.OutsideMeter - MainTerm1ConnectPoint.Diameter.OutsideMeter;
            var branchDiameter = GetConnectPoint( (int) ConnectPointType.BranchTerm ).Diameter ;
            if ( afterDiameter.NpsMm < branchDiameter.NpsMm ) {
              branchDiameter = DiameterFactory.FromNpsMm( newDiameterNpsMm ) ;
              GetConnectPoint( (int) ConnectPointType.BranchTerm ).Diameter = branchDiameter ;
            }
            LengthFromPipeCenter += diffDiameter * 0.5;
            GetConnectPoint((int)ConnectPointType.MainTerm1).Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            GetConnectPoint((int)ConnectPointType.MainTerm2).Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            // Branch側には伝播しない
            break;
          }

        case ConnectPointType.BranchTerm:
          {
            var mainDiameter = GetConnectPoint((int)ConnectPointType.MainTerm1).Diameter ;
            var afterDiameter = DiameterFactory.FromNpsMm( newDiameterNpsMm ) ;
            if ( afterDiameter.NpsMm > mainDiameter.NpsMm ) {
              throw new InvalidOperationException("Can not change branch diameter as main diameter is smaller than branch diameter.");
            }
            
            LengthFromPipeOuter *= DiameterFactory.FromNpsMm(newDiameterNpsMm).OutsideMeter / Diameter;
            BranchTermConnectPoint.Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            // 他のConnectPointには伝播しない
            break;
          }

        default:
          throw new InvalidOperationException();
      }
    }

    public double LengthFromPipeOuter
    {
      get => LengthFromPipeCenter - MainTerm1ConnectPoint.Diameter.OutsideMeter * 0.5 ;
      set => LengthFromPipeCenter = value + MainTerm1ConnectPoint.Diameter.OutsideMeter * 0.5 ;
    }
    
    public double LengthFromPipeCenter
    {
      get => BranchTermConnectPoint.Point.x ;
      set => BranchTermConnectPoint.SetPointVector( new Vector3d( value, 0, 0 ) );
    }


    public double Diameter
    {
      get => BranchTermConnectPoint.Diameter.OutsideMeter ;
      set => BranchTermConnectPoint.Diameter = DiameterFactory.FromOutsideMeter(value);
    }

    public override Bounds GetBounds()
    {
      var length = (float)LengthFromPipeCenter ;
      var o = (Vector3) Origin ;
      o.x += length * 0.5f ;
      return new Bounds( o, new Vector3( length, (float) Diameter, (float) Diameter ) ) ;
    }

    public override ConnectPoint GetAntiPoleConnectPoint( int connectPointIndex )
    {
      switch ( (ConnectPointType) connectPointIndex ) {
        case ConnectPointType.MainTerm1 : return MainTerm2ConnectPoint ;
        case ConnectPointType.MainTerm2 : return MainTerm1ConnectPoint ;
        case ConnectPointType.BranchTerm : return null ;
        default : throw new ArgumentOutOfRangeException( nameof( connectPointIndex ) ) ;
      }
    }
  }
}