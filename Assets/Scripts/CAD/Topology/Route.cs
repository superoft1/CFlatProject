using System;
using System.Linq;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Routing;
using Chiyoda.DB ;
using UnityEngine;

namespace Chiyoda.CAD.Topology
{
  [Serializable]
  [Entity( EntityType.Type.Route )]
  public class Route : CompositeEdge
  {
    private readonly Memento<IEndPoint> _from ;
    private readonly Memento<IEndPoint> _to ;
    private readonly MementoList<IBranch> _branches ;

    private readonly MementoList<Vector3> _throughPoints ;
    private readonly MementoList<LeafEdge> _routingParts ;
    private readonly HashSet<PipingPiece> _errorPieces = new HashSet<PipingPiece>() ;

    private readonly Memento<string> _errorMessage ;

    private readonly Memento<string> _lineId ;
    private readonly Memento<string> _serviceType ;
    private readonly Memento<string> _lineType ;
    private readonly Memento<string> _color ;
    private readonly Memento<string> _fluidPhase ;

    // 表示/演算ロジックのフラグ
    private readonly Memento<bool> _isEndPointDirectionFix ;
    private readonly Memento<bool> _isRoutingPipeRacks ;

    // Insulation
    private readonly Memento<int> _insulationTypeIdx ;
    private readonly Memento<double> _temperature ;
    public IList<string> InsulationTypes
    {
      get { return DB.DB.Get<InsulationTable>().GetCodeList() ; }
    }

    public Route( Document document ) : base( document )
    {
      _from = CreateMementoAndSetupChildrenEvents<IEndPoint>() ;
      _to = CreateMementoAndSetupChildrenEvents<IEndPoint>() ;
      _branches = CreateMementoListAndSetupChildrenEvents<IBranch>() ;
      
      _throughPoints = CreateMementoListAndSetupValueEvents<Vector3>() ;
      _routingParts = CreateMementoListAndSetupChildrenEvents<LeafEdge>() ;
      _errorMessage = CreateMementoAndSetupValueEvents<string>() ;

      _lineId = CreateMementoAndSetupValueEvents<string>() ;
      _serviceType = CreateMementoAndSetupValueEvents<string>() ;
      _lineType = CreateMementoAndSetupValueEvents<string>() ;
      _color = CreateMementoAndSetupValueEvents<string>() ;
      _fluidPhase = CreateMementoAndSetupValueEvents<string>() ;

      _isEndPointDirectionFix = CreateMementoAndSetupValueEvents( false ) ;
      _isRoutingPipeRacks = CreateMementoAndSetupValueEvents( true ) ;
      
      _insulationTypeIdx = CreateMementoAndSetupValueEvents( 0 ) ;
      _temperature = CreateMementoAndSetupValueEvents( 25.0 ) ;
    }

    public override bool HasError => _errorPieces.Any() ;
    
    public IEnumerable<Vector3> ThroughPoints => _throughPoints ;

    public IEndPoint FromPoint => _from.Value ;
    public IEndPoint ToPoint => _to.Value ;
    
    public IEnumerable<IBranch> Branches => _branches ;

    public void PutRoutingResult( LeafEdge e, bool isError )
    {
      _routingParts.Add( e ) ;
      if ( isError ) {
        _errorPieces.Add( e.PipingPiece ) ;
      }
    }
    public void SetThroughPoints( IEnumerable<Vector3> pnts ) => _throughPoints.AddRange( pnts ) ;

    public override IEnumerable<IElement> Children
    {
      get
      {
        foreach ( var child in base.Children ) yield return child ;

        if ( null != FromPoint ) {
          yield return FromPoint ;
        }
        if ( null != ToPoint ) {
          yield return ToPoint;
        }

        foreach ( var b in _branches ) {
          yield return b ;
        }
        
        foreach ( var child in _routingParts ) {
          yield return child ;
        }
      }
    }

    [UI.Property( UI.PropertyCategory.AutoRouting, "LineID", ValueType = UI.ValueType.Text,
      Visibility = UI.PropertyVisibility.Editable )]
    public string LineId
    {
      get => _lineId.Value ;
      set => _lineId.Value = value ;
    }

    [UI.Property( UI.PropertyCategory.AutoRouting, "Service", ValueType = UI.ValueType.Text,
      Visibility = UI.PropertyVisibility.Editable )]
    public string ServiceType
    {
      get => _serviceType.Value ;
      set => _serviceType.Value = value ;
    }

    [UI.Property( UI.PropertyCategory.AutoRouting, "Type", ValueType = UI.ValueType.Text,
      Visibility = UI.PropertyVisibility.Editable )]
    public string LineType
    {
      get => _lineType.Value ;
      set => _lineType.Value = value ;
    }

    [UI.Property( UI.PropertyCategory.AutoRouting, "Color", ValueType = UI.ValueType.Text,
      Visibility = UI.PropertyVisibility.Editable )]
    public string Color
    {
      get => _color.Value ;
      set => _color.Value = value ;
    }

    [UI.Property( UI.PropertyCategory.AutoRouting, "Phase", ValueType = UI.ValueType.Text,
      Visibility = UI.PropertyVisibility.Editable )]
    public string FluidPhase
    {
      get => _fluidPhase.Value ;
      set => _fluidPhase.Value = value ;
    }

    [UI.Property( UI.PropertyCategory.AutoRouting, "RouteID", ValueType = UI.ValueType.Text,
      Visibility = UI.PropertyVisibility.ReadOnly )]
    public string AutoRoutingId { get ; set ; }

    [UI.Property( UI.PropertyCategory.AutoRouting, "IsDirectionFix", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable )]
    public bool IsEndPointDirectionFix
    {
      get => _isEndPointDirectionFix.Value ;
      set => _isEndPointDirectionFix.Value = value ;
    }

    [UI.Property( UI.PropertyCategory.AutoRouting, "IsRoutingPipeRacks", ValueType = UI.ValueType.CheckBox,
      Visibility = UI.PropertyVisibility.Editable )]
    public bool IsRoutingPipeRacks
    {
      get => _isRoutingPipeRacks.Value ;
      set => _isRoutingPipeRacks.Value = value ;
    }

    [UI.Property( UI.PropertyCategory.AutoRouting, "Error Info.", ValueType = UI.ValueType.Auto,
      Visibility = UI.PropertyVisibility.ReadOnly, Order = 10 )]
    public string ErrorMessage
    {
      get => _errorMessage.Value ?? "NA" ;
      set => _errorMessage.Value = value ;
    }

    [UI.Property( UI.PropertyCategory.Insulation, "Type",
      ValueType = UI.ValueType.Select, ListDataMethodName = "InsulationTypes", Order = 11 )]
    public int InsulationTypeIndex
    {
      get => _insulationTypeIdx.Value ;
      set => _insulationTypeIdx.Value = value ;
    }

    public string InsulationType
    {
      get {  return InsulationTypes[_insulationTypeIdx.Value]; }
    }

    [UI.Property( UI.PropertyCategory.Insulation, "Temperature", ValueType = UI.ValueType.Auto,
      Visibility = UI.PropertyVisibility.Editable, Order = 12 )]
    public double Temperature
    {
      get => _temperature.Value ;
      set => _temperature.Value = value ;
    }

    internal void SetMainRoute( IEndPoint from, IEndPoint to )
    {
      _from.Value = from ;
      _to.Value = to ;
      from.Name = "From" ;
      to.Name = "To" ;
    }
    
    internal void AddBranch( IBranch b )
    {
      _branches.Add( b );
      b.TermPoint.Name =  b.IsStart ? "From" : "To" ;
      if ( b is Entity e ) {
        e.Name = $"Branch{_branches.Count}" ;
      }
    }

    internal void DeleteResult()
    {
      _routingParts.Clear() ;
      _throughPoints.Clear() ;
      _errorPieces.Clear() ;
      _errorMessage.Value = null ;
    }

    public void SyncEndPointsToLinkPoint()
    {
      void SyncToLinkPoint( IEndPoint e )
      {
        if ( e.LinkPoint != null ) {
          e.SyncWith( new TermPointConstraints( e.LinkPoint ) ) ;
        }
      }

      void SyncBranch( IBranch b )
      {
        SyncToLinkPoint( b.TermPoint );
        b.Branches.ForEach( SyncBranch );
      }

      SyncToLinkPoint( _from.Value );
      SyncToLinkPoint( _to.Value );
      _branches.ForEach( SyncBranch ) ;
    }

    public override Bounds? GetGlobalBounds()
    {
      return Boundary.GetBounds( Children ) ;
    }

    public static bool HasColor( Entity entity, out Color color )
    {
      if ( entity.Parent?.Parent is Route route ) {
        color = route._errorPieces.Contains( entity )
          ? UnityEngine.Color.red
          : VtpAutoRouting.ColorUtil.GetUnity( route.Color ) ;
        return true ;
      }

      color = UnityEngine.Color.white ;
      return false ;
    }

    public static bool GetInsulationThickness( Entity entity, double outsideDiameter, out double thickness )
    {
      try {
        if ( entity.Parent?.Parent is Route route ) {
          // TODO NPSに変換（NPS対応後に実施する）
          var NPS = (int)(outsideDiameter*1000.0) ;
          thickness = DB.DB.Get<InsulationTable>().
                        GetThickness( route.InsulationType, NPS, (float)route.Temperature ) / 1000.0 ;
          return true ;
        }
        thickness = 0.0 ;
        return false ;
      }
      catch ( NoRecordFoundException e ) {
        thickness = 0.0 ;
        return false ;
      }
    }
  }
}