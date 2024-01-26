using System ;
using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Body;
using UnityEngine ;

namespace Chiyoda.CAD.Topology
{
  [Entity( EntityType.Type.HalfVertex )]
  public class HalfVertex : Topology
  {
    public enum FlowType
    {
      FromThisToAnother,
      FromAnotherToThis,
      Undefined,
    }

    private readonly Memento<int> _connectPointIndex ;
    private readonly Memento<FlowType> _flowType ;
    private readonly Memento<FlowType> _calcFlowType ;
    [IO.LateSerialize]
    private readonly Memento<HalfVertex> _pairPartner ;
    [IO.LateSerialize]
    private readonly Memento<HydraulicStream> _stream ;

    public HalfVertex( Document document ) : base( document )
    {
      _connectPointIndex = new Memento<int>( this, -1 ) ;

      _pairPartner = CreateMementoAndSetupValueEvents<HalfVertex>( null ) ;

      _flowType = new Memento<FlowType>( this, FlowType.Undefined ) ;

      _calcFlowType = new Memento<FlowType>( this, FlowType.Undefined ) ;

      _stream = new Memento<HydraulicStream>( this ) ;
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage ) ;

      var entity = another as HalfVertex ;
      _connectPointIndex.CopyFrom( entity._connectPointIndex.Value ) ;

      _flowType.CopyFrom( entity._flowType.Value ) ;
      _calcFlowType.CopyFrom( entity._calcFlowType.Value ) ;

      // _pairPartnerはCopyObjectStorageの事後処理で実行
      // _streamはコピーしない
    }

    public override Bounds? GetGlobalBounds()
    {
      return LeafEdge?.PipingPiece?.GetGlobalBounds();
    }

    public FlowType Flow
    {
      get => _flowType.Value ;
      set
      {
        _flowType.Value = value ;
        if ( FlowType.Undefined != value ) {
          _calcFlowType.Value = value ;
        }
      }
    }

    public FlowType CalculatedFlow
    {
      get => _calcFlowType.Value ;
      set => _calcFlowType.Value = value ;
    }

    public HydraulicStream HydraulicStream
    {
      get => _stream.Value ;
      set
      {
        _stream.Value = value ;
        if ( null != _pairPartner.Value ) _pairPartner.Value._stream.Value = value ;
      }
    }

    public void ResetHydraulicStream()
    {
      HydraulicStream = null ;
      CalculatedFlow = Flow ;
    }

    public LeafEdge LeafEdge
    {
      get => Parent as LeafEdge;
    }

    public int ConnectPointIndex
    {
      get => _connectPointIndex.Value ;
      set => _connectPointIndex.Value = value ;
    }

    public HalfVertex Partner
    {
      get => _pairPartner.Value ;
      set
      {
        var partner = this._pairPartner.Value ;
        if ( partner == value ) return ;

        if ( null == value ) {
          partner._pairPartner.Value = null ;
          this._pairPartner.Value = null ;
        }
        else {
          if ( null != this._pairPartner.Value ) {
            throw new InvalidOperationException( "Partner can be set for only partnerless HalfVector." ) ;
          }

          if ( null != value._pairPartner.Value ) {
            throw new InvalidOperationException( "Partner can be set for only partnerless HalfVector." ) ;
          }

          value._pairPartner.Value = this ;
          this._pairPartner.Value = value ;
        }
      }
    }

    internal ConnectPoint ConnectPoint => LeafEdge?.PipingPiece?.GetConnectPoint( ConnectPointIndex ) ;

    public Vector3d GetConnectVector()
    {
      var pp = LeafEdge?.PipingPiece ;
      if ( null != pp ) return pp.GetConnectPoint( ConnectPointIndex ).Vector ;
      
      return ConnectPoint.Point ;
    }

    [UI.Property( UI.PropertyCategory.ComponentName, "ID", ValueType = UI.ValueType.Label, Visibility = UI.PropertyVisibility.ReadOnly )]
    private string ComponentName => LeafEdge?.PipingPiece?.Name ?? string.Empty ;

    [UI.Property( UI.PropertyCategory.Position, "Origin", ValueType = UI.ValueType.Position, Visibility = UI.PropertyVisibility.ReadOnly )]
    public Vector3d GlobalPoint => ConnectPoint?.GlobalPoint ?? Vector3d.zero ;
    
    [UI.Property( UI.PropertyCategory.BaseData, "Flow", ValueType = UI.ValueType.Auto, Visibility = UI.PropertyVisibility.ReadOnly )]
    private string FlowTypeString => _flowType.Value.ToString() ;


    public IEnumerable<LeafEdge> LeafEdges
    {
      get
      {
        var le1 = LeafEdge ;
        if ( null != le1 ) yield return le1 ;

        var le2 = _pairPartner.Value?.LeafEdge ;
        if ( null != le2 ) yield return le2 ;
      }
    }

    public override string ToString()
    {
      return $"{base.ToString()} (From={LeafEdge} To={Partner?.LeafEdge})" ;
    }
  }
}