using System;
using System.Collections.Generic;
using System.ComponentModel ;
using System.Data ;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.AirFinCooler)]
  public class AirFinCooler : Equipment
  {
    public enum PlacementPlane
    {
      UpForward = 0,
      UpBack,
      DownForward,
      DownBack
    }
      
    // AirFinCoolerのノズル種別
    public enum NozzleKind
    {
      Inlet = 0,
      Outlet,
    }

    private readonly Memento<double> _widthOfAirCooler;
    private readonly Memento<double> _lengthOfAirCooler;
    private readonly Memento<double> _heightOfAirCooler;
    
    private readonly Memento<double> _upperWidthOfAirCooler;
    private readonly Memento<double> _upperLengthOfAirCooler;
    private readonly Memento<double> _upperHeightOfAirCooler;
    
    private readonly Memento<double> _lowerWidthOfAirCooler;
    private readonly Memento<double> _lowerLengthOfAirCooler;
    private readonly Memento<double> _lowerHeightOfAirCooler;
    
    private readonly MementoList<AFCNozzleArray> _nozzleArrays ;
    private readonly MementoDictionary<NozzleKind,PlacementPlane> _placementMap;
    private readonly MementoList<NozzleKind> _nozzleKindList;

    private const double WidthRate = 0.9;
    private const double LengthRate = 0.8;
    private const double HeightRate = 1;
    

    public AirFinCooler( Document document ) : base( document )
    {
      EquipmentName = "AirFinCooler" ;

      _widthOfAirCooler = CreateMementoAndSetupValueEvents( 0.0 ) ;
      _lengthOfAirCooler = CreateMementoAndSetupValueEvents( 0.0 ) ;
      _heightOfAirCooler = CreateMementoAndSetupValueEvents( 0.0 ) ;
      
      _upperWidthOfAirCooler = CreateMementoAndSetupValueEvents( 0.0 ) ;
      _upperLengthOfAirCooler = CreateMementoAndSetupValueEvents( 0.0 ) ;
      _upperHeightOfAirCooler = CreateMementoAndSetupValueEvents( 0.0 ) ;
      
      _lowerWidthOfAirCooler = CreateMementoAndSetupValueEvents( 0.0 ) ;
      _lowerLengthOfAirCooler = CreateMementoAndSetupValueEvents( 0.0 ) ;
      _lowerHeightOfAirCooler = CreateMementoAndSetupValueEvents( 0.0 ) ;
      
      _nozzleArrays = new MementoList<AFCNozzleArray>( this ) ;
      _nozzleArrays.AfterNewlyItemChanged += ( sender, e ) => OnNewlyChildrenChanged( e.As<IElement>() );
      _nozzleArrays.AfterHistoricallyItemChanged += ( sender, e ) => OnHistoricallyChildrenChanged( e.As<IElement>() ) ;
      
      _placementMap = new MementoDictionary<NozzleKind, PlacementPlane>(this);
      _nozzleKindList = new MementoList<NozzleKind>( this ) ;
    }

    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects() ;
      
      //! NozzleArrayは2つ
      var first = Document.CreateEntity( EntityType.Type.AFCNozzleArray ) as AFCNozzleArray ;
      first.InitialConnectPointIndex = 1000 ;
      _nozzleArrays.Add(first);
      
      var second = Document.CreateEntity( EntityType.Type.AFCNozzleArray ) as AFCNozzleArray ;
      second.InitialConnectPointIndex = 2000 ;
      _nozzleArrays.Add(second);
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as AirFinCooler;
      _widthOfAirCooler.CopyFrom( entity._widthOfAirCooler.Value );
      _lengthOfAirCooler.CopyFrom( entity._lengthOfAirCooler.Value );
      _heightOfAirCooler.CopyFrom( entity._heightOfAirCooler.Value );

      _upperWidthOfAirCooler.CopyFrom( entity._upperWidthOfAirCooler.Value );
      _upperLengthOfAirCooler.CopyFrom( entity._upperLengthOfAirCooler.Value );
      _upperHeightOfAirCooler.CopyFrom( entity._upperHeightOfAirCooler.Value );
      
      _lowerWidthOfAirCooler.CopyFrom( entity._lowerWidthOfAirCooler.Value );
      _lowerLengthOfAirCooler.CopyFrom( entity._lowerLengthOfAirCooler.Value );
      _lowerHeightOfAirCooler.CopyFrom( entity._lowerHeightOfAirCooler.Value );

      _nozzleArrays.CopyFrom( entity._nozzleArrays );
      _placementMap.CopyFrom(entity._placementMap);
      _nozzleKindList.CopyFrom(entity._nozzleKindList);
    }

    [UI.Property(UI.PropertyCategory.BaseData, "WidthOfAirCooler", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double WidthOfAirCooler
    {
      get
      {
        return _widthOfAirCooler.Value;
      }

      set
      {
        _widthOfAirCooler.Value = value;
        _upperWidthOfAirCooler.Value = value * WidthRate ;
        _lowerWidthOfAirCooler.Value = value * WidthRate ;
        AirFinCoolerNozzlePositionChanged();
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "LengthOfAirCooler", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double LengthOfAirCooler
    {
      get
      {
        return _lengthOfAirCooler.Value;
      }

      set
      {
        _lengthOfAirCooler.Value = value;
        _upperLengthOfAirCooler.Value = value * LengthRate ;
        _lowerLengthOfAirCooler.Value = value * LengthRate ;
        AirFinCoolerNozzlePositionChanged();
      }
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfAirCooler", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double HeightOfAirCooler
    {
      get
      {
        return _heightOfAirCooler.Value;
      }

      set
      {
        _heightOfAirCooler.Value = value;
        _upperHeightOfAirCooler.Value = value * HeightRate;
        _lowerHeightOfAirCooler.Value = value * HeightRate;
        AirFinCoolerNozzlePositionChanged();
      }
    }
    
    void AirFinCoolerNozzlePositionChanged()
    {
      foreach ( var array in _nozzleArrays ) {
        array.OnNozzlePositionChanged(EventArgs.Empty) ;
      }
    }

    public double UpperWidthOfAirCooler
    {
      get { return _upperWidthOfAirCooler.Value ; }
    }

    public double UpperLengthOfAirCooler
    {
      get { return _upperLengthOfAirCooler.Value ; }
    }
    
    public double UpperHeightOfAirCooler
    {
      get { return _upperHeightOfAirCooler.Value ; }
    }
    
    public double LowerWidthOfAirCooler
    {
      get { return _lowerWidthOfAirCooler.Value ; }
    }
    
    public double LowerLengthOfAirCooler
    {
      get { return _lowerLengthOfAirCooler.Value ; }
    }
    
    public double LowerHeightOfAirCooler
    {
      get { return _lowerHeightOfAirCooler.Value ; }
    }

    public override Bounds GetBounds()
    {
      var centerOfBounds = new Vector3(0.0f, 0.0f, (float)(0.5 * HeightOfAirCooler + (float)HeightRate * HeightOfAirCooler));
      var sizeOfBounds = new Vector3((float)LengthOfAirCooler, (float)WidthOfAirCooler, (float)HeightOfAirCooler * (1f+(float)HeightRate*2f));
      return new Bounds(centerOfBounds, sizeOfBounds);
    }

    public class AFCNozzleCod
    {
      public AFCNozzleCod( Vector3d origin, Vector3d dirMargin, Vector3d dirNozzleArray, Vector3d dirNozzzle )
      {
        Origin = origin ;
        DirMargin = dirMargin ;
        DirNozzleArray = dirNozzleArray ;
        DirNozzle = dirNozzzle ;
      }
      
      public Vector3d Origin { get ; set ; } 
      public Vector3d DirMargin { get ; set ; }
      public Vector3d DirNozzleArray { get ; set ; }
      public Vector3d DirNozzle { get ; set ; }
    }
    
    public AFCNozzleCod PlacementLocalCodeSys( PlacementPlane placement )
    {
      var lengthOfTerm = LengthOfAirCooler * 0.5 ;
      var heightOfDown = LowerHeightOfAirCooler ;
      var heightOfUp = LowerHeightOfAirCooler + HeightOfAirCooler ;
      if ( placement == PlacementPlane.UpForward ) {
        return new AFCNozzleCod(lengthOfTerm* Vector3d.right + heightOfUp * Vector3d.forward, Vector3d.left, Vector3d.up, Vector3d.forward );
      } else if ( placement == PlacementPlane.UpBack ) {
        return new AFCNozzleCod(lengthOfTerm * Vector3d.left + heightOfUp * Vector3d.forward, Vector3d.right, Vector3d.up, Vector3d.forward );        
      } else if ( placement == PlacementPlane.DownForward ) {
        return new AFCNozzleCod(lengthOfTerm* Vector3d.right + heightOfDown * Vector3d.forward, Vector3d.left, Vector3d.up, Vector3d.back );
      }
      else if (placement == PlacementPlane.DownBack) {
        return new AFCNozzleCod(lengthOfTerm * Vector3d.left + heightOfDown * Vector3d.forward, Vector3d.right, Vector3d.up, Vector3d.back );
      }
      
      throw new InvalidEnumArgumentException();
    }

    public override Nozzle GetNozzle( int nozzleNumber )
    {
      foreach ( var array in _nozzleArrays ) {
        if ( array.ExistsNozzle( nozzleNumber ) ) {
          return array.GetNozzle( nozzleNumber ) ;
        }
      }
      return null ;
    }
    
    public override bool ExistsNozzleNumber( int nozzleNumber )
    {
      foreach ( var array in _nozzleArrays ) {
        if ( array.ExistsNozzle( nozzleNumber ) ) {
          return true ;
        }
      }
      return false ;
    }

    public override Vector3d GetNozzleOriginPosition( Nozzle nozzle )
    {
      var nozzleArray = nozzle.Parent as AFCNozzleArray ;
      return nozzleArray.GetNozzleOriginPosition( nozzle ) ;
    }

    public override Vector3d GetNozzleDirection( Nozzle nozzle )
    {
      var nozzleArray = nozzle.Parent as AFCNozzleArray ;
      return nozzleArray.GetNozzleDirection( nozzle ) ;
    }

    public override IEnumerable<Nozzle> Nozzles
    {
      get
      {
        foreach ( var nozzle in base.Nozzles ) {
          yield return nozzle ;
        }
      }
    }
    
    public override IEnumerable<IElement> Children
    {
      get
      {
        foreach ( var nozzleArray in _nozzleArrays ) yield return nozzleArray ;

        foreach ( var child in base.Children ) {
          yield return child ;
        }
      }
    }
  }
}
