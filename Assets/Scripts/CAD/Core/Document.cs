using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Electricals ;
using Chiyoda.CAD.Plotplan ;
using Chiyoda.CAD.Topology;
using Chiyoda.DB;
using UnityEngine;

namespace Chiyoda.CAD.Core
{
  public partial class Document : MemorableObjectBase, IGroup, ISupportParentElement, IWorkPlane
  {
    private readonly LengthUnitCollection _lengthUnits;
    private readonly DocumentRegion _region;

    private readonly MementoList<Edge> _edges;
    private readonly MementoDictionary<string, Line> _lines;

    private readonly MementoList<Entity> _structures;
    private readonly ElementCollection<Support> _supports;
    private readonly ElementCollection<IWorkPlane> _workPlanes;
    private readonly ElementCollection<Facility> _facilities ;
    private readonly ElementCollection<Electricals> _electrical ;
    private readonly ElementCollection<Unit> _units ;
    
    [NonSerialized]
    private readonly HashSet<IElement> _selections = new HashSet<IElement>();
    [NonSerialized]
    private IElement _belongedElement = null;
    [NonSerialized]
    private readonly MementoList<HalfVertex> _unfilledVertices;
    [NonSerialized]
    private readonly Stack<List<HalfVertex>> _confinedVertices = new Stack<List<HalfVertex>>();
    
    [NonSerialized]
    private IElement _lastSelection = null;
    [NonSerialized]
    private Func<IElement, (IElement, IElement)> _priorPreferredFindScheme = null;
    [NonSerialized]
    private List<DiameterRange> _diameterRanges = new List<DiameterRange>();

    private readonly Memento<bool> _visible;

    public event EventHandler VisibilityChanged ;
    public event EventHandler<ItemChangedEventArgs<IElement>> AfterNewlyChildrenChanged;
    public event EventHandler<ItemChangedEventArgs<IElement>> AfterHistoricallyChildrenChanged;
    public event EventHandler<ItemChangedEventArgs<IElement>> SelectionChanged;
    public event EventHandler Unload;
    public IElement LastSelection => _lastSelection;

    internal bool Close()
    {
      OnUnload( EventArgs.Empty );

      return true;
    }

    protected virtual void OnNewlyChildrenChanged( ItemChangedEventArgs<IElement> e )
    {
      foreach ( var item in e.RemovedItems ) {
        item.OnRemovedFromParent() ;
      }

      foreach ( var item in e.AddedItems ) {
        item.OnAddedIntoNewParent( this ) ;
      }

      AfterNewlyChildrenChanged?.Invoke( this, e ) ;
    }
    protected virtual void OnHistoricallyChildrenChanged( ItemChangedEventArgs<IElement> e )
    {
      AfterHistoricallyChildrenChanged?.Invoke( this, e ) ;
    }

    protected virtual void OnSelectionChanged( ItemChangedEventArgs<IElement> e )
    {
      switch ( _selections.Count ) {
        case 0:
          _belongedElement = null;
          break;
        case 1:
//          _belongedElement = FindPreferredElement( _selections.Single() ).belonged;
          _belongedElement = FindResolvedPreferredElement( _selections.Single() ).belonged;
          break;
        default:
          if ( null != _belongedElement &&
               e.AddedItems.Any( element => _belongedElement != FindPreferredElement( element ).belonged ) ) {
            _belongedElement = null;
          }
          break;
      }

      var lastAddedItem = e.AddedItems.LastOrDefault();
      if ( null != lastAddedItem ) {
        _lastSelection = lastAddedItem;
      }
      else {
        if ( null != _lastSelection && e.RemovedItems.Contains( _lastSelection ) ) {
          _lastSelection = null;
        }
      }

      Draggability.Update( _selections );
      
      SelectionChanged?.Invoke( this, e );
    }

    protected virtual void OnUnload( EventArgs e )
    {
      Unload?.Invoke( this, e );
    }

    public override History History { get; }

    Document IElement.Document => this ;
    IElement IElement.Parent => null ;

    public Draggability Draggability { get; } = new Draggability();

    public bool IsVisible
    {
      get
      {
        return _visible.Value;
      }
      set
      {
        if ( _visible.Value != value ) {
          _visible.Value = value;
        }
      }
    }

    public virtual bool HasError => false ;

    public string Name { get; set; }

    public HydraulicStreamCollection Streams { get; }

    Bounds? IBoundary.GetGlobalBounds()
    {
      var bounds1 = Boundary.GetBounds( EdgeList );
      var bounds2 = Boundary.GetBounds( Lines );
      var bounds3 = Boundary.GetBounds( Structures );

      var boundList = new List<Bounds>();
      boundList.Add( _region.GetGlobalBounds().Value );
      if ( bounds1.HasValue ) boundList.Add( bounds1.Value );
      if ( bounds2.HasValue ) boundList.Add( bounds2.Value );
      if ( bounds3.HasValue ) boundList.Add( bounds3.Value );

      return boundList.UnionBounds();
    }

    public static Document Create()
    {
      return new Document( "Root" );
    }
    public static Document Create( string filepath )
    {
      return new Document( System.IO.Path.GetFileNameWithoutExtension( filepath ) );
    }

    public Document Clone()
    {
      var doc = new Document( this.Name );
      using ( var storage = new CopyObjectStorage() ) {
        doc.CopyFrom( this, storage );
      }
      return doc;
    }

    private Document( string name )
    {
      History = History.Create( this );
      Name = name;

      _lengthUnits = new LengthUnitCollection( this );
      SetupEvents( _lengthUnits ) ;

      _region = CreateEntity<DocumentRegion>() ;
      SetupEvents( _region ) ;

      _edges = new MementoList<Edge>( this );
      _edges.AfterNewlyItemChanged += ( sender, e ) => OnNewlyChildrenChanged( e.As<IElement>() );
      _edges.AfterHistoricallyItemChanged += ( sender, e ) => OnHistoricallyChildrenChanged( e.As<IElement>() ) ;

      _lines = new MementoDictionary<string, Line>( this );
      _lines.AfterItemChanged += ( sender, e ) => SetupLineIdChangedEvents( e ) ;
      _lines.AfterNewlyItemChanged += ( sender, e ) => OnNewlyChildrenChanged( e.Convert<IElement>( pair => pair.Value ) ) ;
      _lines.AfterHistoricallyItemChanged += ( sender, e ) => OnHistoricallyChildrenChanged( e.Convert<IElement>( pair => pair.Value ) ) ;

      _structures = new MementoList<Entity>( this );
      _structures.AfterNewlyItemChanged += ( sender, e ) => OnNewlyChildrenChanged( e.As<IElement>() );
      _structures.AfterHistoricallyItemChanged += ( sender, e ) => OnHistoricallyChildrenChanged( e.As<IElement>() );

      _supports = new ElementCollection<Support>( this );
      _supports.AfterNewlyItemChanged += ( sender, e ) => OnNewlyChildrenChanged( e.As<IElement>() );
      _supports.AfterHistoricallyItemChanged += ( sender, e ) => OnHistoricallyChildrenChanged( e.As<IElement>() );

      _workPlanes = new ElementCollection<IWorkPlane>( this ) ;
      _workPlanes.AfterNewlyItemChanged += ( sender, e ) => OnNewlyChildrenChanged( e.As<IElement>() );
      _workPlanes.AfterHistoricallyItemChanged += ( sender, e ) => OnHistoricallyChildrenChanged( e.As<IElement>() );

      _units = new ElementCollection<Unit>( this ) ;
      _units.AfterNewlyItemChanged += ( sender, e ) => OnNewlyChildrenChanged( e.As<IElement>() );
      _units.AfterHistoricallyItemChanged += ( sender, e ) => OnHistoricallyChildrenChanged( e.As<IElement>() );

      _electrical = new ElementCollection<Electricals>( this ) ;
      _electrical.AfterNewlyItemChanged += ( sender, e ) => OnNewlyChildrenChanged( e.As<IElement>() );
      _electrical.AfterHistoricallyItemChanged += ( sender, e ) => OnHistoricallyChildrenChanged( e.As<IElement>() );

      _unfilledVertices = new MementoList<HalfVertex>( this );

      _visible = new Memento<bool>( this, true );
      _visible.AfterValueChanged += ( sender, e ) => OnVisibilityChanged( EventArgs.Empty );

      Streams = new HydraulicStreamCollection( this );
      Streams.AfterNewlyItemChanged += ( sender, e ) => OnNewlyChildrenChanged( e.As<IElement>() );
      Streams.AfterHistoricallyItemChanged += ( sender, e ) => OnHistoricallyChildrenChanged( e.As<IElement>() );
      
      // DB接続先の設定
      Chiyoda.DB.DB.SetDBConnection( new DBConnection()
        {
          DBType = DBType.SQLite,
          SQLiteDirectory = System.IO.Path.Combine( Application.streamingAssetsPath, "DB" )
        }  
      );
    }

    public void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      var entity = another as Document;
      _lengthUnits.CopyFrom( entity._lengthUnits, storage );
      _region.CopyFrom( entity._region, storage );
      _edges.CloneFrom( entity._edges, storage );
      _lines.Clear();
      foreach ( var pair in entity._lines ) {
        _lines.Add( pair.Key, pair.Value.Clone( storage ) );
      }
      _structures.CloneFrom( entity._structures, storage );

      entity.Streams.CopyFrom( Streams, storage );
      _supports.CopyFrom( entity._supports, storage );
      _workPlanes.CopyFrom( entity._workPlanes, storage );
      _electrical.CopyFrom( entity._electrical, storage );
      _units.CopyFrom( entity._units, storage );
    }

    [UI.Property( UI.PropertyCategory.Dimension, "Display Units" )]
    public LengthUnitCollection LengthUnits { get { return _lengthUnits; } }

    [UI.Property( UI.PropertyCategory.Dimension, "Region" )]
    public DocumentRegion Area => _region ;

    public Region Region => _region ;

    private void SetupLineIdChangedEvents( ItemChangedEventArgs<KeyValuePair<string, Line>> e )
    {
      foreach ( var pair in e.RemovedItems ) {
        var line = pair.Value ;
        line.LineIdChanging -= Line_LineIdChanging ;
        line.LineIdChanged -= Line_LineIdChanged ;
      }

      foreach ( var pair in e.AddedItems ) {
        var line = pair.Value ;
        line.LineIdChanging += Line_LineIdChanging ;
        line.LineIdChanged += Line_LineIdChanged ;
      }
    }

    private void Line_LineIdChanging( object sender, EventArgs e )
    {
      var line = sender as Line;
      _lines.Remove( line.LineId );
    }

    private void Line_LineIdChanged( object sender, EventArgs e )
    {
      var line = sender as Line;
      _lines.Add( line.LineId, line );
    }
    
    protected virtual void OnVisibilityChanged( EventArgs e )
    {
      VisibilityChanged?.Invoke( this, e ) ;
    }

    public IEnumerable<IElement> Children
    {
      get
      {
        yield return _region ;
        foreach ( var pattern in _edges ) yield return pattern;
        foreach ( var line in _lines.Values ) yield return line;
        foreach ( var stream in Streams ) yield return stream;
        foreach ( var structure in _structures ) yield return structure;
        foreach ( var support in _supports ) yield return support;
        foreach ( var wp in _workPlanes ) yield return wp;
        foreach ( var electrical in _electrical ) yield return electrical;
        foreach ( var facility in _units ) yield return facility;
      }
    }

    public IEnumerable<IRegion> Regions
    {
      get { yield return _region ; }
    }


    public IEnumerable<BlockPattern> BlockPatterns
    {
      get { return _edges.OfType<BlockPattern>(); }
    }

    public LocalCodSys3d GlobalCod { get { return LocalCodSys3d.Identity; } }

    public IEnumerable<Edge> EdgeList
    {
      get { return _edges; }
    }

    public int EdgeCount { get { return _edges.Count; } }

    public bool AddEdge( Edge edge )
    {
      _edges.Add( edge );
      return true;
    }

    public bool RemoveEdge( Edge edge )
    {
      return _edges.Remove( edge );
    }

    public bool ReplaceEdge( int index, Edge newEdge )
    {
      if ( index < 0 || _edges.Count <= index ) return false ;

      _edges[ index ] = newEdge ;

      return true ;
    }

    public bool ReplaceEdge( Edge oldEdge, Edge newEdge )
    {
      int index = _edges.IndexOf( oldEdge ) ;
      if ( index < 0 ) return false ;

      return ReplaceEdge( index, newEdge ) ;
    }

    public IEnumerable<Line> Lines => _lines.Values ;
    
    public IList<Entity> Structures => _structures ;
    
    public IEnumerable<Route> Routes => this.EdgeList.OfType<Route>();

    public ICollection<Support> Supports => _supports;

    public ICollection<IWorkPlane> WorkPlanes => _workPlanes ;

    public ICollection<Unit> Units => _units ;

    public ICollection<Electricals> Electrical => _electrical ;

    public Entity CreateEntity( EntityType.Type type )
    {
      return EntityManager.CreateEntity( type, this );
    }

    public Entity CreateEntity( Type type )
    {
      return EntityManager.CreateEntity( type, this );
    }

    public T CreateEntity<T>() where T : Entity
    {
      return EntityManager.CreateEntity<T>( this );
    }

    internal Entity CopyEntity( Entity entity, CopyObjectStorage storage )
    {
      return EntityManager.CopyEntity( entity, storage, this );
    }

    public Line FindLine( string lineId )
    {
      Line line;
      if ( !_lines.TryGetValue( lineId, out line ) ) return null;
      return line;
    }

    public Line FindOrCreateLine( string lineId )
    {
      return FindLine( lineId ) ?? CreateLine( lineId );
    }

    private Line CreateLine( string lineId )
    {
      var line = CreateEntity<Line>();
      line.LineId = lineId;
      _lines.Add( lineId, line );

      return line;
    }

    public bool RemoveLine( string lineId, bool removeAllEdges )
    {
      var line = FindLine( lineId );
      if ( null == line ) return false;

      if ( removeAllEdges ) {
        foreach ( var edge in line.LeafEdges.ToArray() ) {
          if ( edge.Parent is IGroup @group ) {
            group.RemoveEdge( edge );
          }
        }
      }

      _lines.Remove( lineId );

      return true;
    }

    public LeafEdge CreateLeafEdge( PipingPiece pp )
    {
      var leafEdge = CreateEntity<LeafEdge>() ;
      leafEdge.PipingPiece = pp ;
      leafEdge.CreateAllHalfVertices() ;
      return leafEdge ;
    }

    public void CreateHalfVerticesAndMakePairs( LeafEdge le )
    {
      IList<HalfVertex> unfilledVertices = _unfilledVertices;
      if ( _confinedVertices.Any() ) unfilledVertices = _confinedVertices.Peek();

      // 外部でvertexのリンクを行っているとunfilledVerticesの中にペアのものが存在する事の対応
      // FIXME: 外部でフリーバーテックスを発生させている場合はここでは検知出来ていないはず。フリーバーテックスの収集が必要になるかもしれない
      var removeTarget = unfilledVertices.Where( vertex => vertex.Partner != null ).ToList() ;
      foreach ( var vertex in removeTarget ) {
        unfilledVertices.Remove( vertex ) ;
      }
      le.CreateAllHalfVertices() ;
      foreach ( var vertex in le.Vertices ) {
        PairToUnfilledVertex( vertex ) ;
      }
    }

    private void PairToUnfilledVertex( HalfVertex vertex )
    {
      IList<HalfVertex> unfilledVertices = _unfilledVertices;
      if ( _confinedVertices.Any() ) unfilledVertices = _confinedVertices.Peek();

      var isVertexMergeModeOnDifferentLine = Manager.GlobalSettings.Instance == null || Manager.GlobalSettings.Instance.IsVertexMergeModeOnDifferentLine ;// Test RunnerのEditorModeから呼び出した時はGlobalSettingsはnull
      var point = vertex.GlobalPoint ;
      foreach ( var v in unfilledVertices ) {
        if ( IsSameVertex( point, vertex.LeafEdge, v, isVertexMergeModeOnDifferentLine ) ) {
          vertex.Partner = v ;
          unfilledVertices.Remove( v ) ;
          return ;
        }
      }

      unfilledVertices.Add( vertex ) ;
    }
    
    private bool IsSameVertex( in Vector3d globalPoint, LeafEdge edge, HalfVertex anotherVertex, bool isVertexMergeModeOnDifferentLine )
    {
      if ( !isVertexMergeModeOnDifferentLine ) {
        if ( edge.Line.LineId != anotherVertex.LeafEdge.Line.LineId ) return false;
      }

      if ( edge == anotherVertex.LeafEdge ) return false ;

      return Vector3d.Distance( globalPoint, anotherVertex.GlobalPoint ) < Tolerance.MergeToleranceForImporter;
    }

    internal IDisposable ConfineVerticesMakePairs( bool keepUnfilled = true )
    {
      return new VerticesMakePairsConfiner( this, keepUnfilled );
    }

    private class VerticesMakePairsConfiner : IDisposable
    {
      private readonly Document _document ;
      private readonly bool _keepUnfilled ;

      public VerticesMakePairsConfiner( Document document, bool keepUnfilled )
      {
        _document = document;
        _document._confinedVertices.Push( new List<HalfVertex>() );

        _keepUnfilled = keepUnfilled;
      }

      ~VerticesMakePairsConfiner()
      {
        throw new InvalidOperationException( "VerticesMakePairsConfiner is disposed without Dispose()." );
      }

      public void Dispose()
      {
        var confinedVertices = _document._confinedVertices.Pop();

        if ( _keepUnfilled ) {
          if ( _document._confinedVertices.Any() ) {
            _document._confinedVertices.Peek().AddRange( confinedVertices );
          }
          else {
            _document._unfilledVertices.AddRange( confinedVertices );
          }
        }

        GC.SuppressFinalize( this );
      }
    }

    public IPreferredSelector PreferredSelector { get; set; } = null;

    internal void SelectPreferredElement( IElement element, bool append = false )
    {
      if ( append ) {
        if ( _selections.Any( selection => selection != FindPrimePreferredElement( selection ).belonged ) ) {
          _priorPreferredFindScheme = ( element_ ) => ( element_, null );
        }
        else {
          _priorPreferredFindScheme = FindPrimePreferredElement;
          if ( _selections.Count == 1 ) {
            _belongedElement = FindPrimePreferredElement( _selections.Single() ).belonged;
          }
        }
      }

      SelectElement( FindPreferredElement( element ).preferred, append );

      if ( append ) {
        _priorPreferredFindScheme = null;
      }
    }

    private ( IElement preferred, IElement belonged ) FindPreferredElement( IElement element )
    {
      Func<IElement, (IElement, IElement)> findScheme = _priorPreferredFindScheme ?? FindResolvedPreferredElement;
      return findScheme.Invoke( element );
    }

    private ( IElement preferred, IElement belonged ) FindPrimePreferredElement( IElement element )
    {
      var preferredElements = PreferredSelector?.FindPreferredElements( element );
      if ( null == preferredElements || !preferredElements.Any() ) {
        return ( element, null );
      }

      var belongedElement = preferredElements.First();
      if ( null == _belongedElement || belongedElement != _belongedElement ) {
        return ( belongedElement, belongedElement );
      }
      return ( element, belongedElement );
    }

    private ( IElement preferred, IElement belonged ) FindResolvedPreferredElement( IElement element )
    {
      var preferredElements = PreferredSelector?.FindPreferredElements( element );
      if ( null == preferredElements || !preferredElements.Any() ) {
        return ( element, null );
      }

      IElement belongedElement = null;

      if ( null == _belongedElement ) {
        belongedElement = preferredElements.First();
        return ( belongedElement, belongedElement );
      }

      belongedElement = preferredElements.SkipWhile( preferredElement => preferredElement != _belongedElement )
                                         .Take( 2 )
                                         .LastOrDefault();
      if ( null != belongedElement ) {
        return ( belongedElement != _belongedElement ? belongedElement : element , belongedElement );
      }

      foreach ( var commonElement in PreferredSelector.FindPreferredElements( _belongedElement ) ) {
        if ( !preferredElements.Contains( commonElement ) ) {
          continue;
        }
        belongedElement = preferredElements.SkipWhile( preferredElement => preferredElement != commonElement )
                                           .Take( 2 )
                                           .Last();
        return ( belongedElement != commonElement ? belongedElement : element, belongedElement );
      }

      belongedElement = preferredElements.First();
      return ( belongedElement, belongedElement );
    }

    public void SelectElement( IElement element, bool append = false )
    {
      if ( !append ) {
        SelectElements( new [] { element } );
      }
      else
      {
        List<IElement> deselectedElements = new List<IElement>();
        List<IElement> selectedElements = new List<IElement>();
      
        if ( _selections.Remove( element ) ) {
          deselectedElements.Add( element );
        }
        else
        {
          DeselectAllParent( element, deselectedElements );
          DeselectAllDescendants( element, deselectedElements );

          _selections.Add( element );
          selectedElements.Add( element );
        }

        OnSelectionChanged( new ItemChangedEventArgs<IElement>( deselectedElements.ToArray(),
                                                                selectedElements.ToArray() ) );
      }
    }

    public void SelectElements( IEnumerable<IElement> elements )
    {
      List<IElement> deselectedElements = new List<IElement>();
      deselectedElements.AddRange( _selections );

      _selections.Clear();
      _selections.UnionWith( elements );

      List<IElement> selectedElements = new List<IElement>();
      selectedElements.AddRange( elements );

      OnSelectionChanged( new ItemChangedEventArgs<IElement>( deselectedElements.ToArray(),
                                                              selectedElements.ToArray() ) );
    }

    public void DeselectElement( IElement element )
    {
      if ( !_selections.Remove( element ) ) {
        return;
      }

      List<IElement> deselectedElements = new List<IElement>();
      deselectedElements.Add( element );

      OnSelectionChanged( new ItemChangedEventArgs<IElement>( deselectedElements.ToArray(),
                                                              Array.Empty<IElement>() ) );
    }

    public void DeselectAllElements()
    {
      SelectElements( new List<IElement>() );
    }

    private void DeselectAllParent( IElement element, IList<IElement> deselections )
    {
      IElement parent = element.Parent;
      if ( null != parent ) {
        if ( _selections.Remove( parent ) ) {
          deselections.Add( parent );
        }
        else {
          DeselectAllParent( parent, deselections );
        }
      }  
    }

    private void DeselectAllDescendants( IElement element, IList<IElement> deselections )
    {
      foreach ( var child in element.Children ) {
        if ( _selections.Remove( child ) ) {
          deselections.Add( child );
        }
        else {
          DeselectAllDescendants( child, deselections );
        }
      }
    }

    public bool IsSelected( IElement element )
    {
      return _selections.Contains( element );
    }

    public IEnumerable<IElement> SelectedElements
    {
      get { return _selections; }
    }

    public int SelectedElementCount
    {
      get { return _selections.Count; }
    }

    void IElement.OnRemovedFromParent()
    {
      throw new InvalidOperationException( "Document cannot be a child of an element." ) ;
    }

    void IElement. OnAddedIntoNewParent( IElement element )
    {
      throw new InvalidOperationException( "Document cannot be a child of an element." ) ;
    }
  }
}
