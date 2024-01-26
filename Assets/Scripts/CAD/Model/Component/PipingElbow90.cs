using System ;
using Chiyoda.CAD.Core;
using Chiyoda.DB ;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.PipingElbow90 )]
  public class PipingElbow90 : Component
  {
    public enum ConnectPointType
    {
      Term1,
      Term2,
    }

    public enum Elbow90Type
    {
      ShortElbow,
      LongElbow,
    }

    public ConnectPoint Term1ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term1 ) ;
    public ConnectPoint Term2ConnectPoint => GetConnectPoint( (int)ConnectPointType.Term2 ) ;

    private readonly Memento<double> _insulationThickness ;

    private readonly Memento<Elbow90Type> _elbowType ;

    public PipingElbow90( Document document ) : base( document )
    {
      _elbowType = new Memento<Elbow90Type>( this, Elbow90Type.LongElbow ) ;
      _insulationThickness = new Memento<double>( this, 0.0 ) ;

      ComponentName = "Elbow90" ;
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.Term1 );
      AddNewConnectPoint( (int) ConnectPointType.Term2 );
    }

    protected internal override void RegisterNonMementoMembersFromDefaultObjects()
    {
      base.RegisterNonMementoMembersFromDefaultObjects() ;

      _elbowType.AfterNewlyValueChanged += ( sender, e ) => SetupBendLength( Diameter ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as PipingElbow90;
      _elbowType.CopyFrom( entity._elbowType.Value ) ;
      _insulationThickness.CopyFrom( entity._insulationThickness.Value ) ;
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      switch ((ConnectPointType)connectPointNumber)
      {
        case ConnectPointType.Term1:
        case ConnectPointType.Term2:
          {
            var afterDiameter = DiameterFactory.FromNpsMm(newDiameterNpsMm).OutsideMeter;
            SetupBendLength(afterDiameter);
            GetConnectPoint((int)ConnectPointType.Term1).Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            GetConnectPoint((int)ConnectPointType.Term2).Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            // ElbOletには伝播しない
            break;
          }

        default:  // ElbOlet
          {
            GetConnectPoint(connectPointNumber).Diameter = DiameterFactory.FromNpsMm(newDiameterNpsMm);
            // 他のConnectPointには伝播しない
            break;
          }
      }
    }

    public double BendLength
    {
      get => Term1ConnectPoint.Point.y ;
    }

    private void SetupBendLength( double diameterOutsideMeter)
    {
      var diameterObj = DiameterFactory.FromOutsideMeter(diameterOutsideMeter) ;
      double length ;
      switch ( ElbowType ) {
        case Elbow90Type.ShortElbow : length = GetDefaultShortBendLength( diameterObj ) ; break ;
        case Elbow90Type.LongElbow : length = GetDefaultLongBendLength( diameterObj ) ; break ;
        default : throw new ArgumentOutOfRangeException() ;
      }
      
      Term1ConnectPoint.SetPointVector( new Vector3d( 0, length, 0 ) );
      Term2ConnectPoint.SetPointVector( new Vector3d( -length, 0, 0 ) );
    }


    public Vector3d Term1
    {
      get
      {
        return Term1ConnectPoint.Point;
      }
    }

    public Vector3d Term2
    {
      get
      {
        return Term2ConnectPoint.Point;
      }
    }

    public double Diameter
    {
      get
      {
        return Term1ConnectPoint.Diameter.OutsideMeter;
      }
    }

    public Elbow90Type ElbowType
    {
      get => _elbowType.Value ;
      set => _elbowType.Value = value ;
    }

    [UI.Property( UI.PropertyCategory.Insulation, "Thickness", ValueType = UI.ValueType.Length, Visibility = UI.PropertyVisibility.ReadOnly )]
    public double InsulationThickness
    {
      get { return _insulationThickness.Value ; }
      set { _insulationThickness.Value = Math.Max( 0.0, value ) ; }
    }

    [UI.Property( UI.PropertyCategory.Insulation, "Code", ValueType = UI.ValueType.Text, Visibility = UI.PropertyVisibility.ReadOnly )]
    public string InsulationCode { get; set; }

    public override Bounds GetBounds()
    {
      var bounds = new Bounds( (Vector3)Origin, Vector3.one * (float)(Diameter + InsulationThickness * 2) );
      bounds.Encapsulate( (Vector3)Term1 );
      bounds.Encapsulate( (Vector3)Term2 );
      return bounds;
    }

    public static double GetDefaultLongBendLength( Diameter diameter )
    {
      try {
        var table = Chiyoda.DB.DB.Get<DimensionOfElbowsTable>() ;
        return ((double)table.GetOne( diameter.NpsMm, "90deg" ).CenterToEnd).Millimeters() ;
      }
      catch ( Chiyoda.DB.NoRecordFoundException e ) {
        // TODO 1000本ノックの例外回避のための一時処理（1/4インチの径を取得するケース）
        //      -> SWタイプのデータを挿入次第、削除する
        return diameter.NpsMm * 0.001 * 1.5 ;
      }
    }

    public static double GetDefaultShortBendLength( Diameter diameter )
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfElbowsTable>() ;
      return ( (double) table.GetOne( diameter.NpsMm, "S90deg" ).CenterToEnd ).Millimeters() ;
    }

    public void SetElbowTypeFromBendLength( Diameter diameter, double bendLength )
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfElbowsTable>() ;
      var rec = table.GetOne90( diameter.NpsMm, (float)bendLength.ToMillimeters() ) ;
      ElbowType = rec.ElbowType == "90deg" ? Elbow90Type.LongElbow : Elbow90Type.ShortElbow ;
    }
  }
}