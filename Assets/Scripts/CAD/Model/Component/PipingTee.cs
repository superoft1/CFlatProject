using System ;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.DB ;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.PipingTee )]
  public class PipingTee : Component, IParallelPipe
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

    private readonly Memento<double> _mainLength;
    private readonly Memento<double> _branchLength;

    private readonly Memento<double> _insulationThickness;

    public override bool IsEndOfStream => true;


    public PipingTee( Document document ) : base( document )
    {
      _mainLength = new Memento<double>( this ) ;
      _branchLength = new Memento<double>( this ) ;
      _insulationThickness = new Memento<double>( this, 0 ) ;

      ComponentName = "Tee" ;
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

      var entity = another as PipingTee;
      _mainLength.CopyFrom( entity._mainLength.Value );
      _branchLength.CopyFrom( entity._branchLength.Value );
      _insulationThickness.CopyFrom( entity._insulationThickness.Value );
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      switch ( (ConnectPointType) connectPointNumber ) {
        case ConnectPointType.MainTerm1 :
        case ConnectPointType.MainTerm2 :
        {
          var afterDiameter = DiameterFactory.FromNpsMm( newDiameterNpsMm ) ;
          var branchDiameter = DiameterFactory.FromOutsideMeter( BranchDiameter ) ;
          if ( afterDiameter.NpsMm < branchDiameter.NpsMm ) {
            branchDiameter = DiameterFactory.FromNpsMm( newDiameterNpsMm ) ;
            GetConnectPoint( (int) ConnectPointType.BranchTerm ).Diameter = branchDiameter ;
          }
          MainLength = GetDefaultMainLength( afterDiameter, branchDiameter ) ;
          BranchLength = GetDefaultBranchLength( afterDiameter, branchDiameter ) ;
          GetConnectPoint( (int) ConnectPointType.MainTerm1 ).Diameter = DiameterFactory.FromNpsMm( newDiameterNpsMm ) ;
          GetConnectPoint( (int) ConnectPointType.MainTerm2 ).Diameter = DiameterFactory.FromNpsMm( newDiameterNpsMm ) ;
          // Branch側には伝播しない
          break ;
        }

        case ConnectPointType.BranchTerm :
        {
          var mainDiameter = DiameterFactory.FromOutsideMeter( MainDiameter ) ;
          var afterDiameter = DiameterFactory.FromNpsMm( newDiameterNpsMm ) ;
          if ( afterDiameter.NpsMm > mainDiameter.NpsMm ) {
            throw new InvalidOperationException("Can not change branch diameter as main diameter is smaller than branch diameter.");
          }
          MainLength = GetDefaultMainLength( mainDiameter, afterDiameter ) ;
          BranchLength = GetDefaultBranchLength( mainDiameter, afterDiameter ) ;
          GetConnectPoint( connectPointNumber ).Diameter = DiameterFactory.FromNpsMm( newDiameterNpsMm ) ;
          // 他のConnectPointには伝播しない
          break ;
        }

        default : // ElbOlet
        {
          GetConnectPoint( connectPointNumber ).Diameter = DiameterFactory.FromNpsMm( newDiameterNpsMm ) ;
          // 他のConnectPointには伝播しない
          break ;
        }
      }
    }

    public double MainLength
    {
      get => _mainLength.Value ;
      set
      {
        var mainTerm1 = MainTerm1ConnectPoint ;
        var mainTerm2 = MainTerm2ConnectPoint ;

        _mainLength.Value = value;

        mainTerm1.SetPointVector( Vector3d.right * _mainLength.Value * 0.5);
        mainTerm2.SetPointVector( Vector3d.left * _mainLength.Value * 0.5);
      }
    }

    public double BranchLength
    {
      get => _branchLength.Value ;
      set
      {
        _branchLength.Value = value;

        BranchTermConnectPoint.SetPointVector( Vector3d.up * _branchLength.Value);
      }
    }


    public double MainDiameter
    {
      get => MainTerm1ConnectPoint.Diameter.OutsideMeter ;

      set
      {
        var mainTerm1 = MainTerm1ConnectPoint ;
        var mainTerm2 = MainTerm2ConnectPoint ;

        mainTerm2.Diameter = DiameterFactory.FromOutsideMeter( value ) ;
        mainTerm1.Diameter = mainTerm2.Diameter ;
      }
    }

    public double BranchDiameter
    {
      get => BranchTermConnectPoint.Diameter.OutsideMeter ;

      set
      {
        var branchTerm = BranchTermConnectPoint ;

        branchTerm.Diameter = DiameterFactory.FromOutsideMeter( value ) ;
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

    double IParallelPipe.ParallelDiameter => MainDiameter;
    IEnumerable<(double, double)> IParallelPipe.ParallelRanges => new[] { (-MainLength / 2, MainLength / 2) };
    Vector3d IParallelPipe.LocalParallelDirection => Vector3d.left;

    public override Bounds GetBounds()
    {
      var mainTerm1 = MainTerm1ConnectPoint ;
      var mainTerm2 = MainTerm2ConnectPoint ;
      var branchTerm = BranchTermConnectPoint ;

      var bounds = new Bounds( (Vector3)Origin, Vector3.one * (float)(MainDiameter + InsulationThickness * 2) );
      bounds.Encapsulate( (Vector3)mainTerm1.Point );
      bounds.Encapsulate( (Vector3)mainTerm2.Point );
      bounds.Encapsulate( (Vector3)branchTerm.Point );
      return bounds;
    }

    public static double GetDefaultMainLength( Diameter mainDiameter, Diameter branchDiameter )
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfTeesTable>() ;
      return 2 * table.GetOne( mainDiameter.NpsMm, branchDiameter.NpsMm ).CenterToEnd_Run_C.Millimeters() ;
    }
    
    public static double GetDefaultBranchLength( Diameter mainDiameter, Diameter branchDiameter )
    {
      var table = Chiyoda.DB.DB.Get<DimensionOfTeesTable>() ;
      return table.GetOne( mainDiameter.NpsMm, branchDiameter.NpsMm ).CenterToEnd_Outlet_M.Millimeters() ;
    }
  }
}