using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.BP;
using System;
using Chiyoda.CAD.Maintainer;
using System.Linq;

namespace Chiyoda.CAD.Topology
{
  [System.Serializable]
  [Entity( EntityType.Type.BlockPattern )]
  public class BlockPattern : BlockEdge, ISupportParentElement
  {
    private readonly Memento<bool> _propertyEditable;
    private readonly Memento<BlockPatternType.Type> _type;
    private readonly ElementCollection<Support> _supports;

    private readonly Util.LineIdObserver _lineIdObserver;

    public BlockPattern( Document document ) : base( document )
    {
      _type = CreateMemento( BlockPatternType.Type.Unknown ) ;
      _propertyEditable = CreateMemento( true ) ;

      _supports = new ElementCollection<Support>( this );
      _supports.AfterNewlyItemChanged += ( sender, e ) => OnNewlyChildrenChanged( e.As<IElement>() );
      _supports.AfterHistoricallyItemChanged += ( sender, e ) => OnHistoricallyChildrenChanged( e.As<IElement>() );

      // LineIdObserver のコンストラクタは内部でChildrenを呼ぶため、一番最後に
      _lineIdObserver = new Util.LineIdObserver( this );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as BlockPattern;
      _type.CopyFrom( entity._type.Value );
      _propertyEditable.CopyFrom( entity._propertyEditable.Value );
      _supports.CopyFrom( entity._supports, storage );

      ClearDescendantNameCache();
    }

    public override bool UserDefinedPropertyEditable => _propertyEditable.Value;

    public void SetUserDefinedPropertyEditable( bool value )
    {
      _propertyEditable.Value = value;
    }

    [UI.Property( UI.PropertyCategory.Position, "WorldPosition", ValueType = UI.ValueType.Position, Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false )]
    public Vector3d BlockPatternPosition
    {
      get
      {
        return LocalCod.Origin;
      }

      set
      {
        LocalCod = new LocalCodSys3d( value, LocalCod );
      }
    }

    [UI.Property( UI.PropertyCategory.Position, "Rotation", ValueType = UI.ValueType.Rotation, Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false )]
    public double Rotation
    {
      get
      {
        return (double)LocalCod.Rotation.eulerAngles.z;
      }
      set
      {
        var rot = new LocalCodSys3d( LocalCod.Origin, Quaternion.AngleAxis( (float)value, Vector3.forward ), LocalCod.IsMirrorType );
        LocalCod = rot;
      }
    }

    public BlockPatternType.Type Type
    {
      get => _type.Value ;
      set => _type.Value = value ;
    }

    public IEnumerable<LeafEdge> EquipmentEdges
    {
      get { return EdgeList.OfType<LeafEdge>().Where( e => e.PipingPiece is Equipment ); }
    }
    public IEnumerable<Edge> NonEquipmentEdges
    {
      get { return EdgeList.Where( e => !(e is LeafEdge le) || !(le.PipingPiece is Equipment) ); }
    }

    public ICollection<Support> Supports => _supports;

    public IEnumerable<Equipment> Equipments
    {
      get
      {
        foreach ( var i in EquipmentEdges ) {
          yield return i.PipingPiece as Equipment;
        }
      }
    }

    public IEnumerable<Line> Lines { get { return _lineIdObserver.Lines; } }

    public override IEnumerable<IElement> Children => base.Children.Concat( _supports );

    public LeafEdge CreateEquipmentEdgeVertex( PipingPiece pipingPiece, LocalCodSys3d localCod, bool createVertex = true )
    {
      var leafEdge = Document.CreateEntity<LeafEdge>();
      leafEdge.PipingPiece = pipingPiece;
      leafEdge.LocalCod = localCod;

      AddEdge( leafEdge );

      if ( createVertex ) {
        Document.CreateHalfVerticesAndMakePairs( leafEdge );
      }

      return leafEdge;
    }

    public void CreateVertices( LeafEdge leafEdge )
    {
      Document.CreateHalfVerticesAndMakePairs( leafEdge );
    }


    private Dictionary<string, IPropertiedElement> _namedDescendants = null;

    public void ClearDescendantNameCache()
    {
      _namedDescendants = null;
    }

    private void CollectAllNamedDescendants()
    {
      if ( null != _namedDescendants ) return;

      _namedDescendants = new Dictionary<string, IPropertiedElement>();

      foreach ( var elm in GetAllDescendants() ) {
        var entity = elm as Entity;
        if ( null != entity && null != entity.ObjectName ) {
          if ( false == _namedDescendants.ContainsKey( entity.ObjectName ) ) {
            _namedDescendants.Add( entity.ObjectName, entity );
          }
        }
      }
    }

    public override IPropertiedElement GetElementByName( string objectName )
    {
      if ( string.IsNullOrEmpty( objectName ) ) return null;

      if ( this.GetSpecialElement( objectName ) is IPropertiedElement specialEntity ) return specialEntity;

      CollectAllNamedDescendants();

      if ( _namedDescendants.TryGetValue( objectName, out var elm ) ) return elm;

      return null;
    }

    public override void Mirror( in Vector3d origin, in Vector3d normalDirection )
    {
      var planeCodSys = new LocalCodSys3d( origin, Vector3d.zero, Vector3d.zero, normalDirection ) ;

      base.Mirror( origin, normalDirection ) ;

      var newCodSys = LocalCod.LocalizeCodSys( planeCodSys ) ;
      var o = newCodSys.Origin ;
      var dir = newCodSys.DirectionZ ;

      foreach ( var support in _supports ) {
        support.Mirror( o, dir ) ;
      }
    }

    protected override void ReleaseAllEdgesForDisassemble()
    {
      _edgeList.Clear() ;
    }
  }
}