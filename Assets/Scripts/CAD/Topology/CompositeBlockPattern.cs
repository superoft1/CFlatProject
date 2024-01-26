using System ;
using System.Collections.Generic ;
using System.ComponentModel ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Chiyoda.CAD.Topology
{
  public abstract partial class CompositeBlockPattern : BlockEdge
  {
    public enum JoinType
    {
      /// <summary>
      /// 結合しない
      /// </summary>
      Independent,

      /// <summary>
      /// BasePattern方向に伸ばす
      /// </summary>
      [Description( "BaseSide" )]
      StraightInnerDir,

      /// <summary>
      /// BasePatternとは逆方向に伸ばす
      /// </summary>
      [Description( "Outside" )]
      StraightOuterDir,

      /// <summary>
      /// 中央にTを用意する。中央に用意できない場合、BasePattern方向にずらすか伸ばす
      /// </summary>
      [Description( "Middle(Base)" )]
      MiddleTeeInnerDir,

      /// <summary>
      /// 中央にTを用意する。中央に用意できない場合、BasePatternとは逆方向にずらすか伸ばす
      /// </summary>
      [Description( "Middle(Out)" )]
      MiddleTeeOuterDir,

      /// <summary>
      /// 中央にTを用意する。中央に用意できない場合、BasePattern方向にずらすか伸ばす(＋90°回転)
      /// </summary>
      [Description( "Middle(Base)+90°" )]
      MiddleTeeInnerDir90,

      /// <summary>
      /// 中央にTを用意する。中央に用意できない場合、BasePatternとは逆方向にずらすか伸ばす(＋90°回転)
      /// </summary>
      [Description( "Middle(Out)+90°" )]
      MiddleTeeOuterDir90,

      /// <summary>
      /// 中央にTを用意する。中央に用意できない場合、BasePattern方向にずらすか伸ばす(＋180°回転)
      /// </summary>
      [Description( "Middle(Base)+180°" )]
      MiddleTeeInnerDir180,

      /// <summary>
      /// 中央にTを用意する。中央に用意できない場合、BasePatternとは逆方向にずらすか伸ばす(＋180°回転)
      /// </summary>
      [Description( "Middle(Out)+180°" )]
      MiddleTeeOuterDir180,

      /// <summary>
      /// 中央にTを用意する。中央に用意できない場合、BasePattern方向にずらすか伸ばす(－90°回転)
      /// </summary>
      [Description( "Middle(Base)-90°" )]
      MiddleTeeInnerDir270,

      /// <summary>
      /// 中央にTを用意する。中央に用意できない場合、BasePatternとは逆方向にずらすか伸ばす(－90°回転)
      /// </summary>
      [Description( "Middle(Out)-90°" )]
      MiddleTeeOuterDir270,
    }


    private readonly Memento<BlockPattern> _basePattern ;
    private readonly MementoDictionary<string, JoinType> _joinTypes ;
    private readonly MementoDictionary<string, HalfVertex[]> _jointEndVertices ;
    private readonly MementoDictionary<string, string> _firstPipeNames ;
    private readonly MementoDictionary<string, (string name, HalfVertex.FlowType flow)> _vertexNames ;
    private readonly MementoDictionary<string, Group> _jointGroups ;

    private bool _cloning = false ; // Clone中フラグ

    protected CompositeBlockPattern( Document document ) : base( document )
    {
      _basePattern = new Memento<BlockPattern>( this ) ;
      _basePattern.AfterHistoricallyValueChanged += ( sender, e ) =>
      {
        if ( null != e.OldValue ) {
          RemoveChangingEvents( e.OldValue, false ) ;
        }

        if ( null != e.NewValue ) {
          AddChangingEvents( e.NewValue, false ) ;
        }
      } ;
      _basePattern.AfterNewlyValueChanged += ( sender, e ) =>
      {
        if ( null != e.OldValue ) {
          RemoveChangingEvents( e.OldValue, ! _cloning ) ;
        }

        if ( null != e.NewValue ) {
          AddChangingEvents( e.NewValue, ! _cloning ) ;
        }

        if ( _cloning ) return ;

        using ( new VertexRejoiner( this ) ) {
          if ( null != e.OldValue ) {
            SplitHalfVertices( this ) ;
            _edgeList.Clear() ;
          }

          if ( null != e.NewValue ) {
            _edgeList.Add( e.NewValue ) ;
            UpdateClonedEdges() ;
          }
          else {
            EraseAllJointData() ;
          }
        }
      } ;
      _joinTypes = new MementoDictionary<string, JoinType>( this ) ;
      _jointEndVertices = new MementoDictionary<string, HalfVertex[]>( this ) ;
      _firstPipeNames = new MementoDictionary<string, string>( this ) ;
      _vertexNames = new MementoDictionary<string, (string, HalfVertex.FlowType)>( this ) ;
      _jointGroups = new MementoDictionary<string, Group>( this ) ;
    }

    public sealed override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      var entity = another as CompositeBlockPattern ;
      _cloning = true ; // JointEdge作成等の処理を無効化
      try {
        base.CopyFrom( another, storage ) ;

        _basePattern.Value = _edgeList[ 0 ] as BlockPattern ;

        _joinTypes.SetCopyValueFrom( entity._joinTypes ) ;
        _jointEndVertices.CopyFrom( entity._jointEndVertices.Select( pair => new KeyValuePair<string, HalfVertex[]>( pair.Key, pair.Value.GetCopyObjectArray( storage ) ) ) ) ;
        _firstPipeNames.CopyFrom( entity._firstPipeNames ) ;
        _vertexNames.CopyFrom( entity._vertexNames ) ;
        _jointGroups.SetCopyObjectFrom( entity._jointGroups, storage ) ;

        CopyFromImpl( entity, storage ) ;
      }
      finally {
        _cloning = false ;
      }
    }

    protected abstract void CopyFromImpl( CompositeBlockPattern compositeBlockPattern, CopyObjectStorage storage ) ;
    
    public abstract int CopyCount { get ; }


    [UI.Property( UI.PropertyCategory.Position, "WorldPosition", ValueType = UI.ValueType.Position, Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false )]
    public Vector3d Position
    {
      get => LocalCod.Origin ;
      set => LocalCod = new LocalCodSys3d( value, LocalCod ) ;
    }

    [UI.Property( UI.PropertyCategory.Position, "Rotation", ValueType = UI.ValueType.Rotation, Visibility = UI.PropertyVisibility.Editable, IsEditableForBlockPatternChildren = false )]
    public double Rotation
    {
      get => LocalCod.Rotation.eulerAngles.z ;
      set => LocalCod = new LocalCodSys3d( LocalCod.Origin, Quaternion.AngleAxis( (float) value, Vector3.forward ), LocalCod.IsMirrorType ) ;
    }

    public BlockPattern BaseBlockPattern
    {
      get => _basePattern.Value ;
      set => _basePattern.Value = value ;
    }


    public BlockPattern GetBlockPattern( int index )
    {
      if ( index < 0 || CopyCount <= index ) throw new ArgumentOutOfRangeException( nameof( index ) ) ;

      return _edgeList[ index ] as BlockPattern ;;
    }
    
    public override bool AddEdge( Edge edge )
    {
      throw new InvalidOperationException( "Cannot add edges by AddEdge() method." ) ;
    }

    protected void RecreateJointEdges()
    {
      using ( new VertexRejoiner( this ) ) {
        UpdateClonedEdges() ;
      }
    }
    
    

    private void AddChangingEvents( BlockPattern bp, bool newlyValueChanged )
    {
      bp.PropertyAdded += BlockPattern_PropertyAdded ;
      bp.PropertyRemoved += BlockPattern_PropertyRemoved ;

      if ( newlyValueChanged ) {
        bp.SetUserDefinedPropertyEditable( false ) ;

        foreach ( var prop in bp.GetProperties().OfType<UserDefinedNamedPropertyBase>() ) {
          RegisterSameNameUserDefinedProperty( prop ).AddUserDefinedRule( UserDefinedRules.ApplyToChildBlockPatternRule ) ;
          RuleList.AddClonedRangeRules( bp.RuleList, prop.PropertyName ) ;
        }
      }
    }

    private void RemoveChangingEvents( BlockPattern bp, bool newlyValueChanged )
    {
      bp.PropertyAdded -= BlockPattern_PropertyAdded ;
      bp.PropertyRemoved -= BlockPattern_PropertyRemoved ;

      if ( newlyValueChanged ) {
        bp.SetUserDefinedPropertyEditable( true ) ;
        
        foreach ( var prop in bp.GetProperties().OfType<UserDefinedNamedPropertyBase>() ) {
          UnregisterProperty( prop.PropertyName ) ;
          RuleList.RemoveClonedRangeRules( bp.RuleList, prop.PropertyName ) ;
        }
      }
    }

    private void BlockPattern_PropertyAdded( object sender, PropertyEventArgs e )
    {
      if ( e.Property is UserDefinedNamedPropertyBase prop ) {
        RegisterSameNameUserDefinedProperty( prop ).AddUserDefinedRule( UserDefinedRules.ApplyToChildBlockPatternRule ) ;
        RuleList.AddClonedRangeRules( ( (Entity) sender ).RuleList, prop.PropertyName ) ;
      }
    }

    private void BlockPattern_PropertyRemoved( object sender, PropertyEventArgs e )
    {
      if ( e.Property is UserDefinedNamedPropertyBase prop ) {
        UnregisterProperty( prop.PropertyName ) ;
        RuleList.RemoveClonedRangeRules( ( (Entity) sender ).RuleList, prop.PropertyName ) ;
      }
    }

    private static void OnPropertyChanged( IPropertiedElement elm, IUserDefinedNamedProperty prop )
    {
      var cbp = elm as CompositeBlockPattern ;
      var name = prop.PropertyName ;

      for ( int i = 0, n = cbp.CopyCount ; i < n ; ++i ) {
        (cbp._edgeList[ i ].GetProperty( name ) as IUserDefinedNamedProperty)?.ModifyFrom( prop ) ;
      }
    }

    private void SetupClonedBlockPattern( BlockPattern bp )
    {
      bp.Name = $"{_basePattern.Value.Name} (copy)" ;

      bp.SetUserDefinedPropertyEditable( false ) ;
    }

    public void JoinByName( string endEdgeName, JoinType joinType )
    {
      if ( ! _joinTypes.TryGetValue( endEdgeName, out var orgType ) ) {
        orgType = JoinType.Independent ;
      }

      if ( joinType == orgType ) return ;

      _joinTypes.Remove( endEdgeName ) ;
      if ( JoinType.Independent != joinType ) {
        _joinTypes.Add( endEdgeName, joinType ) ;
      }

      using ( new VertexRejoiner( this ) ) {
        SetupJointEdges( endEdgeName ) ;
      }
    }

    public void SetFirstJointPipeName( string endEdgeName, string pipeName )
    {
      if ( string.IsNullOrEmpty( pipeName ) ) {
        _firstPipeNames.Remove( endEdgeName ) ;
        pipeName = null ;
      }
      else {
        _firstPipeNames[ endEdgeName ] = pipeName ;
      }

      // 作成済みエッジの名前を変更
      if ( _jointGroups.TryGetValue( endEdgeName, out var group ) ) {
        var pipeEdge = group.EdgeList?.ElementAtOrDefault( 1 ) ;
        if ( null != pipeEdge ) {
          pipeEdge.ObjectName = pipeName ;
        }
      }
    }

    public string GetFirstJointPipeName( string endEdgeName )
    {
      if ( _firstPipeNames.TryGetValue( endEdgeName, out var pipeName ) ) {
        if ( ! string.IsNullOrEmpty( pipeName ) ) return pipeName ;
      }

      return null ;
    }

    public void SetVertexName( string endEdgeName, string vertexName, HalfVertex.FlowType flow )
    {
      if ( string.IsNullOrEmpty( vertexName ) ) {
        _vertexNames.Remove( endEdgeName ) ;
        vertexName = null ;
      }
      else {
        _vertexNames[ endEdgeName ] = (vertexName, flow) ;
      }

      // 作成済みVertexの名前を変更
      if ( _jointEndVertices.TryGetValue( endEdgeName, out var vertices ) ) {
        foreach ( var v in vertices ) {
          v.Name = vertexName ;
          v.Flow = flow ;
        }
      }
      else if ( ! _joinTypes.ContainsKey( endEdgeName ) ) {
        // Independent
        foreach ( var bp in _edgeList.Take( CopyCount ) ) {
          var elm = bp.GetElementByName( endEdgeName ) as LeafEdge ;
          if ( null == elm ) continue ;

          foreach ( var v in GetFreeVertex( elm ) ) {
            v.Name = vertexName ;
            v.Flow = flow ;
          }
        }
      }
    }

    public (string name, HalfVertex.FlowType flow)? GetVertexName( string endEdgeName )
    {
      if ( _vertexNames.TryGetValue( endEdgeName, out var vertexName ) ) {
        return vertexName ;
      }
      return null ;
    }

    public void ModifyJointEdges()
    {
      using ( new VertexRejoiner( this ) ) {
        // 再作成が必要なエッジのみ再作成
        foreach ( var endEdgeName in _joinTypes.Keys ) {
          if ( ! _jointGroups.TryGetValue( endEdgeName, out var group ) ) {
            // 存在しないので再作成不要
            continue ;
          }

          // JointGroupがあるなら対応LeafEdgeは必ず見つかるはず (le != null)
          var le = BaseBlockPattern.GetElementByName( endEdgeName ) as LeafEdge ;
          var endVertex = le.Vertices.FirstOrDefault( v => v.Partner == null || v.Partner.Closest<BlockEdge>() == this ) ;
          if ( null == endVertex ) {
            throw new InvalidOperationException() ;
          }

          if ( null == endVertex.Partner || Tolerance.DistanceTolerance < Math.Abs( endVertex.ConnectPoint.Diameter.OutsideMeter - endVertex.Partner.ConnectPoint.Diameter.OutsideMeter ) ) {
            // JointEdgeが未作成か、径が変わったため再作成
            SetupJointEdges( endEdgeName ) ;
          }
          else {
            // 移動
            var moveVec = endVertex.GlobalPoint - endVertex.Partner.GlobalPoint ;
            if ( Tolerance.DistanceTolerance * Tolerance.DistanceTolerance < moveVec.sqrMagnitude ) {
              // 移動が必要
              var translate = group.GlobalCod.LocalizeVector( moveVec ) ;
              foreach ( var e in group.EdgeList ) {
                e.LocalCod = e.LocalCod.Translate( translate ) ;
              }
            }
          }
        }
      }
    }

    private void SetupJointEdgesAll()
    {
      foreach ( var endEdgeName in _joinTypes.Keys ) {
        SetupJointEdges( endEdgeName ) ;
      }
    }

    private void SetupJointEdges( string endEdgeName )
    {
      if ( _jointGroups.TryGetValue( endEdgeName, out var group ) ) {
        SplitHalfVertices( group ) ;
        _edgeList.Remove( group ) ;
      }

      int newCount = CopyCount ;
      if ( newCount == 1 ) {
        if ( BaseBlockPattern.GetElementByName( endEdgeName ) is LeafEdge le ) {
          SetJointEndVertices( endEdgeName, GetFreeVertex( le ).ToArray() ) ;
        }
        else {
          SetJointEndVertices( endEdgeName, Array.Empty<HalfVertex>() ) ;
        }

        _jointGroups.Remove( endEdgeName ) ;
        return ;
      }

      if ( ! _joinTypes.TryGetValue( endEdgeName, out var joinType ) ) {
        joinType = JoinType.Independent ;
      }

      AddJointEdges( endEdgeName, joinType ) ;
    }

    private static void SplitHalfVertices( Edge edge )
    {
      foreach ( var v in edge.Vertices ) {
        v.Partner = null ;
      }
    }

    private void AddJointEdges( string endEdgeName, JoinType joinType )
    {
      int n = CopyCount ;

      var bp = _basePattern.Value ;
      var elm = bp.GetElementByName( endEdgeName ) as LeafEdge ;
      if ( null == elm ) {
        SetJointEndVertices( endEdgeName, Array.Empty<HalfVertex>() ) ;
        return ;
      }

      var vertices = new List<HalfVertex>() ;
      vertices.AddRange( GetFreeVertex( elm ) ) ;

      for ( int i = 1 ; i < n ; ++i ) {
        var elm2 = _edgeList[ i ].GetElementByName( endEdgeName ) as LeafEdge ;
        vertices.AddRange( GetFreeVertex( elm2 ) ) ;
      }

      if ( JoinType.Independent == joinType ) {
        _jointGroups.Remove( endEdgeName ) ;
        SetJointEndVertices( endEdgeName, vertices.ToArray() ) ;
        return ;
      }

      if ( vertices[ 0 ].Name == GetVertexName( endEdgeName )?.name ) {
        // HalfVertexのNameを新しいVertexに付け替えるため、元のVertexからは削除
        foreach ( var v in vertices ) {
          v.Name = null ;
        }
      }

      var firstPipeName = GetFirstJointPipeName( endEdgeName ) ;

      var group = Document.CreateEntity<Group>() ;
      using ( CAD.Topology.Group.ContinuityIgnorer( group ) ) {
        foreach ( var jointEdge in CreateJointEdges( elm, vertices, joinType, firstPipeName ) ) {
          group.AddEdge( jointEdge ) ;
        }
      }

      group.Name = $"Joint Edge ({endEdgeName})" ;
      _jointGroups[ endEdgeName ] = group ;
      _edgeList.Add( group ) ;

      SetJointEndVertices( endEdgeName, GetFreeVertex( group ).ToArray() ) ;
    }

    private LeafEdge[] CreateJointEdges( LeafEdge firstEdge, IList<HalfVertex> vertices, JoinType joinType, string firstPipeName )
    {
      return new CompositeBlockPatternJointEdgeCreator( this, joinType, firstPipeName ).CreateEdges( firstEdge, vertices ) ;
    }

    private void EraseAllJointData()
    {
      _joinTypes.Clear() ;
      _vertexNames.Clear() ;
      _firstPipeNames.Clear() ;
      _jointEndVertices.Clear() ;
      _jointGroups.Clear() ;
    }

    private void SetJointEndVertices( string endEdgeName, HalfVertex[] vertices )
    {
      _jointEndVertices[ endEdgeName ] = vertices ;

      if ( _vertexNames.TryGetValue( endEdgeName, out var vertexName ) ) {
        foreach ( var v in vertices ) {
          v.Name = vertexName.name;
          v.Flow = vertexName.flow ;
        }
      }
    }

    private static IEnumerable<HalfVertex> GetFreeVertex( Edge edge )
    {
      return edge.Vertices.Where( v => null == v.Partner ) ;
    }

    private void UpdateClonedEdges()
    {
      if ( null == _basePattern.Value ) {
        SplitHalfVertices( this ) ;
        _edgeList.Clear() ;
        EraseAllJointData() ;
      }
      else {
        int newCount = CopyCount, orgCount = _edgeList.Count ;
        for ( int i = 0 ; i < orgCount ; ++i ) {
          if ( _edgeList[ i ] is BlockPattern ) continue ;

          for ( var j = i ; j < orgCount ; ++j ) {
            SplitHalfVertices( _edgeList[ j ] ) ;
          }

          _edgeList.RemoveRange( i, orgCount - i ) ;
          orgCount = i ;
          break ;
        }

        if ( newCount < orgCount ) {
          for ( int i = newCount ; i < orgCount ; ++i ) {
            SplitHalfVertices( _edgeList[ i ] ) ;
          }

          _edgeList.RemoveRange( newCount, orgCount - newCount ) ;
        }

        while ( _edgeList.Count < newCount ) {
          using ( var storage = new CopyObjectStorage() ) {
            var bp = _basePattern.Value.Clone( storage ) ;
            SetupClonedBlockPattern( bp ) ;
            _edgeList.Add( bp ) ;
          }
        }

        var baseCod = _basePattern.Value.LocalCod ;
        for ( int i = 1 ; i < newCount ; ++i ) {
          _edgeList[ i ].LocalCod = CreateCopyLocalCod( baseCod, i ) ;
        }

        SetupJointEdgesAll() ;
      }
    }

    protected abstract LocalCodSys3d CreateCopyLocalCod( LocalCodSys3d baseCod, int index ) ;

    protected override void ReleaseAllEdgesForDisassemble()
    {
      // BlockPatternを削除するとJointEdgeの再作成が走ってしまうので、それ以外のみ削除
      _edgeList.RemoveRange( CopyCount, _edgeList.Count - CopyCount ) ;
    }

    private class VertexRejoiner : IDisposable
    {
      private readonly CompositeBlockPattern _cbp ;
      private readonly Dictionary<string, (HalfVertex, HalfVertex)[]> _oldVertexInfo ;

      public VertexRejoiner( CompositeBlockPattern cbp )
      {
        _cbp = cbp ;

        _oldVertexInfo = new Dictionary<string, (HalfVertex, HalfVertex)[]>( cbp._jointEndVertices.Count ) ;
        foreach ( var pair in cbp._jointEndVertices ) {
          _oldVertexInfo.Add( pair.Key, GetVertexInfo( pair.Value ) ) ;
        }
      }

      ~VertexRejoiner()
      {
        throw new InvalidProgramException( "VertexRejoiner must be used with a `using` statement." ) ;
      }

      public void Dispose()
      {
        GC.SuppressFinalize( this ) ;

        foreach ( var pair in _cbp._jointEndVertices ) {
          if ( ! _oldVertexInfo.TryGetValue( pair.Key, out var oldVertexInfo ) ) continue ;

          JoinVertices( pair.Value, oldVertexInfo ) ;
        }
      }

      private void JoinVertices( HalfVertex[] vertices, (HalfVertex, HalfVertex)[] oldVertexInfo )
      {
        var minCount = Math.Min( vertices.Length, oldVertexInfo.Length ) ;
        for ( int i = 0 ; i < minCount ; ++i ) {
          var oldVertex = oldVertexInfo[ i ].Item1 ;
          vertices[ i ].LeafEdge.SwapVertex( vertices[ i ], oldVertex ) ;
          vertices[ i ] = oldVertex ;
          oldVertex.Partner = oldVertexInfo[ i ].Item2 ;
        }
      }

      private static (HalfVertex, HalfVertex)[] GetVertexInfo( HalfVertex[] vertices )
      {
        return Array.ConvertAll( vertices, v => ( v, v.Partner ) ) ;
      }
    }
  }
}