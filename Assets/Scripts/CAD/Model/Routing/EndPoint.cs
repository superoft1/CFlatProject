using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Routing
{
  [Entity(EntityType.Type.EndPoint)]
  public class EndPoint : DirectionalPoint, IEndPoint
  {
    private readonly Memento<string> _pipeRackId;
    private readonly Memento<string> _lineId;
    private readonly Memento<HalfVertex> _linkPoint ;
    
    public EndPoint( Document document ) : base( document )
    {
      _lineId = CreateMementoAndSetupValueEvents<string>() ;
      _pipeRackId = CreateMementoAndSetupValueEvents<string>() ;
      _linkPoint = CreateMementoAndSetupValueEvents<HalfVertex>() ;
      
      ShowArrow = ShowArrowMode.Always;
    }

    public override void CopyFrom(ICopyable another, CopyObjectStorage storage)
    {
      base.CopyFrom(another, storage);

      var entity = another as EndPoint;
      _lineId.CopyFrom( entity._lineId.Value );
      _pipeRackId.CopyFrom( entity._pipeRackId.Value );
    }

    public HalfVertex LinkPoint
    {
      get => _linkPoint.Value ;
      set
      {
        if ( value == null || _linkPoint.Value != null ) {
          return ;
        }
        _linkPoint.Value = value ;
        SyncWith( new TermPointConstraints( value ) );
      }
    }

    [UI.Property( UI.PropertyCategory.AutoRouting, "LineID",
      ValueType = UI.ValueType.Text, Visibility = UI.PropertyVisibility.ReadOnly )]
    public string LineId
    {
      get => _lineId.Value ;
      set => _lineId.Value = value ;
    }

    [UI.Property( UI.PropertyCategory.EquipNo, "EquipNo", 
      ValueType = UI.ValueType.Label, Visibility = UI.PropertyVisibility.ReadOnly )]
    private string ParentEquipment
    {
      get
      {
        if ( LinkPoint == null ) {
          return "" ;
        }
        
        var p = LinkPoint.LeafEdges.FirstOrDefault() ;
        if ( p == null ) {
          return string.Empty ;
        }

        if ( p.PipingPiece is Equipment inst ) {
          return inst.EquipNo ;
        }

        var block = p.Parent as BlockPattern ;
        for ( ; block != null ; block = block.Parent as BlockPattern ) {
          var instrument = block.Equipments.FirstOrDefault() ;
          if ( instrument != null ) {
            return instrument.EquipNo ;
          }
        }
        return string.Empty ;
      }
    }

    public void SyncWith( IRoutingConstraint src )
    {
      if ( src == null ) {
        return ;
      }

      if ( src.PositionConstraint.HasValue ) {
        Origin = src.PositionConstraint.Value ;
      }
      if ( src.DirectionConstraint.HasValue ) {
        Direction = src.DirectionConstraint.Value ;
      }
      if( src.DiameterConstraint != null ) {
        NPS = src.DiameterConstraint.NpsMm*0.001 ;
      }
    }

    public Vector3d? PositionConstraint => Origin ;
    public Vector3d? DirectionConstraint => Direction ;
    public Diameter DiameterConstraint => DiameterBody ;

    public IEnumerable<IRoutingEdge> Links => Enumerable.Empty<IRoutingEdge>() ;
  }
}
