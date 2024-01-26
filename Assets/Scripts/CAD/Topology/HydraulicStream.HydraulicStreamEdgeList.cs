using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics ;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Topology
{
  partial class HydraulicStream
  {
    private class HydraulicStreamEdgeList
    {
      private readonly HashSet<LeafEdge> _allEdges;
      private readonly HashSet<LeafEdge> _endEdges;
      private readonly HashSet<HalfVertex> _nextVertices;

      /// <summary>ストリームが含む <see cref="LeafEdge"/> を全て取得します（末端エッジを含む）。</summary>
      public IEnumerable<LeafEdge> AllLeafEdges => _allEdges;

      /// <summary>ストリームの末端 <see cref="LeafEdge"/> を全て取得します。</summary>
      public IEnumerable<LeafEdge> EndLeafEdges => _endEdges;

      /// <summary>末端の <see cref="LeafEdge"/> の <see cref="HalfVertex"/> のうち、別のストリームに繋がるものを全て取得します。</summary>
      public IEnumerable<HalfVertex> NextVertices => _nextVertices;

      public static void Create( HydraulicStream stream )
      {
        if ( null != stream.PromoterVertex ) {
          stream._edgeList.Value = new HydraulicStreamEdgeList( stream );
        }
        else {
          StreamGraph.Solve( stream );
        }
      }

      private HydraulicStreamEdgeList( HydraulicStream stream )
      {
        if ( null == stream.PromoterVertex ) throw new ArgumentException() ;

        CollectEdgesFromPromoterVertex( stream, stream.PromoterVertex, out _allEdges, out _endEdges ) ;

        // 探索終了エッジのVertexのうち、自身に含まれないものを NextVertices とする
        _nextVertices = new HashSet<HalfVertex>( _endEdges.SelectMany( le => le.Vertices ).Select( v => v.Partner )
          .Where( v => v != null ).Where( v => stream != v.HydraulicStream ) ) ;

        // 本当にFrom/Toに接続してるかどうかチェック
        if ( false == HasValidStreamConnections( stream, _nextVertices ) ) {
//          throw new InvalidOperationException( "Invalid hydraulic stream connections." ) ;
          UnityEngine.Debug.LogError("Invalid hydraulic stream connections.");
        }
      }

      private HydraulicStreamEdgeList( HashSet<LeafEdge> allEdges, HashSet<LeafEdge> endEdges, HashSet<HalfVertex> nextVertices )
      {
        _allEdges = allEdges;
        _endEdges = endEdges;
        _nextVertices = nextVertices;
      }

      private static bool HasValidStreamConnections( HydraulicStream stream, ICollection<HalfVertex> nextVertices )
      {
        int allNeighborStreamCount = stream.FromSideNeighborStreams.Count + stream.ToSideNeighborStreams.Count;
        if ( nextVertices.Count != allNeighborStreamCount ) return false;

        var connections = new List<HydraulicStream>( stream.NeighborStreams.Where( IsKnownStream ) );
        foreach ( var v in nextVertices ) {
          if ( null == v.HydraulicStream ) continue;
          if ( !connections.Remove( v.HydraulicStream ) ) return false;
        }
        if ( 0 < connections.Count ) return false;

        return true;
      }

      private static void CollectEdgesFromPromoterVertex( HydraulicStream stream, HalfVertex promoter, out HashSet<LeafEdge> allEdges, out HashSet<LeafEdge> endEdges )
      {
        allEdges = new HashSet<LeafEdge>();
        endEdges = new HashSet<LeafEdge>();

        var queue = new Queue<HalfVertex>();
        queue.Enqueue( promoter );

        while ( 0 < queue.Count ) {
          var v = queue.Dequeue();
          if ( null != v.HydraulicStream && stream != v.HydraulicStream ) {
            //throw new InvalidOperationException( "One vertex has at most one hydraulic streams." );
            UnityEngine.Debug.LogError( "One vertex has at most one hydraulic streams." );
          }
          v.HydraulicStream = stream;

          foreach ( var le in v.LeafEdges ) {
            if ( !allEdges.Add( le ) ) continue;  // 探索済み

            if ( le.PipingPiece.IsEndOfStream ) {
              // このエッジは探索終了
              endEdges.Add( le );
            }
            else {
              // 次のエッジを探索
              foreach ( var v2 in le.Vertices ) {
                if ( null == v2.HydraulicStream ) {
                  queue.Enqueue( v2 );
                }
              }
            }
          }
        }
      }

      private static bool IsKnownStream( HydraulicStream stream ) => (null != stream._promoterVertex.Value || null != stream._edgeList);



      private class StreamGraph
      {
        public static void Solve( HydraulicStream promoterStream )
        {
          var graph = new StreamGraph( promoterStream );
          graph.SolveFromNeighborStreams();
          graph.SolveAll();
        }



        private readonly HashSet<HydraulicStream> _innerStreams = new HashSet<HydraulicStream>();
        private readonly HashSet<HydraulicStream> _edgeStreams = new HashSet<HydraulicStream>();

        private StreamGraph( HydraulicStream stream )
        {
          var queue = new Queue<HydraulicStream>();
          queue.Enqueue( stream );

          while ( 0 < queue.Count ) {
            var s = queue.Dequeue();
            if ( !_innerStreams.Add( s ) ) continue;

            if ( IsKnownStream( s ) ) {
              // 終端ストリーム
              _edgeStreams.Add( s );
            }
            else {
              // 非終端ストリーム
              foreach ( var s2 in s.NeighborStreams ) {
                queue.Enqueue( s2 );
              }
            }
          }

          _innerStreams.ExceptWith( _edgeStreams );
        }

        private void SolveFromNeighborStreams()
        {
          IEnumerable<HydraulicStream> cancidateStreams = _innerStreams.ToArray();
          while ( true ) {
            var newNeighbors = new HashSet<HydraulicStream>();
            foreach ( var stream in cancidateStreams ) {
              if ( false == _innerStreams.Contains( stream ) ) continue;

              if ( TrySolveFromNeighborStreams( stream, out var fixedStreams ) ) {
                newNeighbors.UnionWith( fixedStreams.SelectMany( s => s.NeighborStreams ) );
                _edgeStreams.UnionWith( fixedStreams );
                _innerStreams.ExceptWith( fixedStreams );
              }
            }
            if ( 0 == newNeighbors.Count ) break;

            // 今回発見したStreamに隣接するStreamを再チェック
            newNeighbors.ExceptWith( _innerStreams );
            cancidateStreams = newNeighbors;
          }
          // 端点からのみ構築可能なものは全て構築済み
        }

        private bool TrySolveFromNeighborStreams( HydraulicStream stream, out IEnumerable<HydraulicStream> fixedStreams )
        {
          fixedStreams = null;

          if ( false == GetCommonNodeEdges( stream.FromSideNeighborStreams, out var commonFromNodeEdges ) ) return false; // 条件を満たすEdgeが見つからない
          if ( false == GetCommonNodeEdges( stream.ToSideNeighborStreams, out var commonToNodeEdges ) ) return false;    // 条件を満たすEdgeが見つからない

          var candVertices = new List<(HalfVertex Vertex, HashSet<LeafEdge> AllEdges, HashSet<LeafEdge> EndEdges, HashSet<HalfVertex> NextVertices)>();
          HashSet<HalfVertex> doneVertex = new HashSet<HalfVertex>();
          foreach ( var leafEdge in commonFromNodeEdges.Union( commonToNodeEdges ) ) {
            foreach ( var vertex in leafEdge.Vertices.Where( v => null == v.HydraulicStream ) ) {
              if ( doneVertex.Contains( vertex ) ) continue;  // 他のvertexを元に計算完了

              CollectEdgesFromPromoterVertex( stream, vertex, out var allEdges, out var endEdges );
              // 探索終了エッジのVertexのうち、streamに含まれないものを nextVertices とする
              var nextVertices = new HashSet<HalfVertex>( endEdges.SelectMany( le => le.Vertices ).Where( v => stream != v.HydraulicStream ) );
              // 本当にFrom/Toに接続してるかどうかチェック
              if ( true == HasValidStreamConnections( stream, nextVertices ) ) {
                candVertices.Add( (Vertex: vertex, AllEdges: allEdges, EndEdges: endEdges, NextVertices: nextVertices) );
                doneVertex.UnionWith( nextVertices.Select( v => v.Partner ).Where( v => null != v ) ) ;
                doneVertex.UnionWith( nextVertices );
              }
            }
          }

          if ( 0 == candVertices.Count && (0 < commonFromNodeEdges.Length + commonToNodeEdges.Length) ) {
            // 接続に矛盾アリ
            throw new InvalidOperationException( "Invalid hydraulic stream connections." );
          }
          if ( 1 == candVertices.Count ) {
            // 候補が1つだけなので確定可能
            stream._edgeList.Value = new HydraulicStreamEdgeList( candVertices[0].AllEdges, candVertices[0].EndEdges, candVertices[0].NextVertices );
            fixedStreams = new[] { stream };
            return true;
          }

          // 複数候補がある場合は、この段階では何もしない
          return false;
        }

        private void SolveAll()
        {
          // TODO
        }



        private static bool GetCommonNodeEdges( ICollection<HydraulicStream> streams, out LeafEdge[] commonNodeEdges )
        {
          commonNodeEdges = null;

          HashSet<LeafEdge> commonEdges = null;
          foreach ( var stream in streams ) {
            if ( !IsKnownStream( stream ) ) continue;

            var edges = stream._edgeList.Value.EndLeafEdges;
            if ( null == commonEdges ) {
              // 最初の候補
              commonEdges = new HashSet<LeafEdge>( edges );
            }
            else {
              // ストリーム間で候補が一致するもののみを選択
              commonEdges.IntersectWith( edges );
              if ( 0 == commonEdges.Count ) return false;
            }
          }

          if ( 0 != commonEdges.Count ) {
            int streamCount = streams.Count + 1, knownStreamCount = streams.Count( IsKnownStream );
            commonNodeEdges = commonEdges.Where( le => le.VertexCount == streamCount && GetKnownStreamVertexCount( le ) == knownStreamCount ).ToArray();
          }
          else {
            commonNodeEdges = Array.Empty<LeafEdge>();
          }

          return true;
        }

        private static int GetKnownStreamVertexCount( LeafEdge le )
        {
          return le.Vertices.Count( v => null != v.HydraulicStream );
        }
      }
    }
  }
}