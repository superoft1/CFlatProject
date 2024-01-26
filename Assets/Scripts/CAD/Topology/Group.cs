using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Topology
{
  [System.Serializable]
  [Entity( EntityType.Type.Group )]
  public class Group : CompositeEdge
  {
    public static bool CanCreateContinuousGroup( params Edge[] edges )
    {
      if ( null == edges || 0 == edges.Length ) return false;

      IGroup commonParent = null;
      foreach ( var edge in edges ) {
        if ( null == edge ) return false;

        if ( null == commonParent ) {
          commonParent = edge.Group;
        }
        else if ( commonParent != edge.Group ) {
          return false;
        }
      }

      return Edge.IsContinuous( edges );
    }

    public void MergeGroup(Group merge)
    {
      if (this.Parent != merge.Parent) return;
      //TODO 座標変換

      using (ContinuityIgnorer(this))
      {
        foreach (var edge in merge.Children)
        {
          this.AddEdge(edge as Edge);
        }
        var group = this.Parent as IGroup;
        group.RemoveEdge(merge);
      }
    }

    public static Group MergeGroup(List<Group> groups)
    {
      if (groups.Count == 0) return null;
      var main = groups[0];
      for (int i = 1; i < groups.Count; ++i)
      {
        main.MergeGroup(groups[i]);
      }
      return main;
    }

    public static Group CreateContinuousGroup<T>( params T[] edges ) where T : Edge
    {
      if ( null == edges || 0 == edges.Length ) {
        throw new ArgumentException( "A group must have at least one edge." ) ;
      }

      IGroup commonParent = null ;
      foreach ( var edge in edges ) {
        if ( null == edge ) {
          throw new ArgumentException( "Edges cannot be null." ) ;
        }

        if ( null == commonParent ) {
          commonParent = edge.Group ;
        }
        else if ( commonParent != edge.Group ) {
          throw new ArgumentException( "Edges don't have a common group." ) ;
        }
      }

      if ( ! Edge.IsContinuous( edges ) ) {
        throw new ArgumentException( "Edges are not continuous." ) ;
      }

      var group = commonParent.Document.CreateEntity<Group>() ;
      using ( ContinuityIgnorer( commonParent ) ) {
        foreach ( var edge in edges ) {
          commonParent.RemoveEdge( edge ) ;
          group.AddEdgeInternal( edge ) ;
        }

        commonParent.AddEdge( group ) ;
      }

      return group ;
    }

    public Group( Document document ) : base( document )
    {
    }

    public override bool AddEdge( Edge edge )
    {
      if ( 0 == EdgeCount || CanIgnoreContinuity ) {
        AddEdgeInternal( edge );
        return true;
      }

      var myVerts = GetVertices();
      foreach ( var v in edge.Vertices ) {
        if ( myVerts.Contains( v.Partner ) ) {
          AddEdgeInternal( edge );
          return true;
        }
      }

      return false;
    }

    public override bool RemoveEdge( Edge edge )
    {
      if ( !CanIgnoreContinuity && !Edge.IsContinuous( EdgeList.Where( e => e != edge ) ) ) return false;

      return base.RemoveEdge( edge );
    }

    public override bool ReplaceEdge( int index, Edge newEdge )
    {
      if ( index < 0 || _edgeList.Count <= index ) return false ;

      var oldEdge = _edgeList[ index ] ;
      if ( oldEdge == newEdge ) return true ;

      if ( ! CanIgnoreContinuity && ! Edge.IsContinuous( EdgeList.Where( e => e != oldEdge ).Concat( new[] { newEdge } ) ) ) return false ;

      return base.ReplaceEdge( index, newEdge );
    }

    private void AddEdgeInternal( Edge edge )
    {
      base.AddEdge( edge );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var group = another as Group;

      // CanIgnoreContinuityは無視
    }


    #region 一時的に不連続性を許容する処理

    // インポート時にのみ必要
    private bool CanIgnoreContinuity { get; set; }

    public static IDisposable ContinuityIgnorer( IGroup group )
    {
      return new ContinuityIgnorerObject( group );
    }

    private class ContinuityIgnorerObject : IDisposable
    {
      private readonly Group _group;
      public ContinuityIgnorerObject( IGroup group )
      {
        _group = group as Group;
        if ( null != _group ) {
          if ( false == _group.CanIgnoreContinuity ) {
            _group.CanIgnoreContinuity = true;
          }
          else {
            // 多重の場合は片方は無視
            _group = null;
          }
        }
      }

      ~ContinuityIgnorerObject()
      {
        throw new InvalidProgramException( "Group.ContinuityIgnorer( IGroup group ) is not disposed." );
      }

      public void Dispose()
      {
        if ( null != _group ) {
          _group.CanIgnoreContinuity = false;
        }

        GC.SuppressFinalize( this );
      }
    }

    #endregion
  }
}
