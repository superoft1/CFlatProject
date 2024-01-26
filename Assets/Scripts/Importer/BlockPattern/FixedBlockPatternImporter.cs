using System ;
using System.Collections.Generic ;
using System.Data ;
using System.IO ;
using System.Linq ;
using System.Text ;
using Chiyoda.CAD ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Chiyoda.CAD.Util ;
using Chiyoda.Importer ;
using IDF ;
using Importer.Equipment ;
using UnityEngine ;

namespace Importer.BlockPattern
{
  static class FixedBlockPatternImporter
  {
    /// <summary>
    /// IDFのファイル名と行番号の一致を調べるキー
    /// </summary>
    private struct IdfFileIndexKey: IEquatable<IdfFileIndexKey>
    {
      private string File { get; }
      private int LineIndex { get; }

      public IdfFileIndexKey(string file, int lineIndex)
      {
        File = file;
        LineIndex = lineIndex ;
      }

      public bool Equals( IdfFileIndexKey other )
      {
        return File == other.File && LineIndex == other.LineIndex ;
      }

      public override bool Equals( object obj )
      {
        return obj is IdfFileIndexKey other && Equals( other ) ;
      }

      public override int GetHashCode()
      {
        unchecked {
          return ( ( File != null ? File.GetHashCode() : 0 ) * 397 ) ^ LineIndex ;
        }
      }
    }
    
    /// <summary>
    /// LeafEdgeの名前と座標(Originに一致)
    /// </summary>
    private class LeafEdgeInfo
    {
      /// <summary>
      /// vertexが設定される場合のみNozzleIDが設定される
      /// </summary>
      public string NozzleId { get; set; }

      public string LeafName { get; set; }
      public Vector3d Position { get; set; }
    }

    public static Chiyoda.CAD.Topology.BlockPattern Import(DataSet dataSet, string csvPath, bool createVertex = true, bool createNozzle = true)
    {
      List<List<string>> idfDirAndTranses = new List<List<string>>();
      List<string> deleteIDFs = new List<string>();
      List<string> boundaryTargetFiles = new List<string>();
      List<string> instruments = new List<string>();
      List<List<string>> vertices = new List<List<string>>();
      List<List<string>> specialMove = new List<List<string>>();// AirFinCoolerのためだけの特別処理（自動ルーティングを通すため）
      List<List<string>> specialDelete = new List<List<string>>();// AirFinCoolerのためだけの特別処理（自動ルーティングを通すため）
      var blockType = BlockPatternType.Type.Unknown;
      foreach ( var valueTuple in ImportUtil.ParseCsv( Path.Combine( AGRUnitView.FixedBlockPatternPath(), csvPath ) ) ) {
        if (valueTuple.type.Contains( "IdfDirAndTrans" )) {
          idfDirAndTranses.Add( valueTuple.values );
        }
        else if (valueTuple.type.Contains( "BoundaryTargetIDFs" )) {
          boundaryTargetFiles.Add( Path.Combine( AGRUnitView.FixedBlockPatternPath(), valueTuple.values[0] ) );
        }
        else if (valueTuple.type.Contains( "DeleteIDFs" )) {
          deleteIDFs.Add( Path.Combine( AGRUnitView.FixedBlockPatternPath(), valueTuple.values[0] ) );
        }
        else if (valueTuple.type.Contains( "Vertex" )) {
          vertices.Add( valueTuple.values );
        }
        else if ( valueTuple.type.Contains( "SpecialMove" ) ) {
          specialMove.Add(valueTuple.values);
        }
        else if ( valueTuple.type.Contains( "SpecialDelete" ) ) {
          specialDelete.Add(valueTuple.values);
        }
        else {
          var parsedType = BlockPatternType.Parse( valueTuple.type );
          if (parsedType != BlockPatternType.Type.Unknown) {
            blockType = parsedType;
            instruments.Add( valueTuple.values[ 0 ] ) ;
          }
        }
      }
      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();
      var bp = BlockPatternFactory.CreateBlockPattern( blockType );

      // IDFより先に機器の座標を設定
      bool updateBlockPatternLocalCod = true ;
      foreach ( var inst in instruments ) {
        var instrumentTable = PipingPieceTableFactory.Create( blockType, dataSet ) ;
        var (instrument, origin, rot) = instrumentTable.Generate( curDoc, inst, createNozzle: createNozzle ) ;
        if ( updateBlockPatternLocalCod ) {
          bp.LocalCod = new LocalCodSys3d( origin, rot, false ) ;
        }
        updateBlockPatternLocalCod = false ;
        BlockPatternFactory.CreateInstrumentEdgeVertex( bp, instrument as Chiyoda.CAD.Model.Equipment, origin, rot ) ;
      }

      var createdEdgesAll = new Dictionary<string, List<LeafEdge>>();
      foreach ( var idfDirAndTrans in idfDirAndTranses ) {
        var createdEdges = new Dictionary<string, List<(LeafEdge le, string file, int lineIndex)>>();
        var idfBp = curDoc.CreateEntity<Chiyoda.CAD.Topology.BlockPattern>();
        idfBp.Type = BlockPatternType.Type.Idf;
        idfBp.Name = idfBp.Type + "Block";
        bp.AddEdge( idfBp );

        var idfDirCod = ImportUtil.ParseIdfDir( AGRUnitView.FixedBlockPatternPath(), idfDirAndTrans );
        foreach ( var idf in idfDirCod.idfPath ) {
          var file = Path.GetFileNameWithoutExtension( idf ) ;
          var idfDeserializer = new IDFDeserializer();
          var grpInfo = new GroupInfo(curDoc, idfBp, idf, appendDirectlyToGroup:true) ;
          var edges = idfDeserializer.ImportData( grpInfo, idf, false );
          if (edges == null) continue;
          foreach ( var (leafEdge, lineIndex) in edges ) {
            if (createdEdges.ContainsKey( leafEdge.PipingPiece.Name )) {
              createdEdges[leafEdge.PipingPiece.Name].Add( (leafEdge, file, lineIndex) );
            }
            else {
              createdEdges.Add( leafEdge.PipingPiece.Name, new List<(LeafEdge le, string file, int lineIndex)> {(leafEdge, file, lineIndex)} );
            }
          }
        }


#if false
        // 毎回結果の調整が大変なので、8月時点でのIDFで使われていたインデックスを保存するように変更。そのための取得コード
        var boundaryTargetPipes = createdEdges.Values.SelectMany( lf => lf )
          .Where( e => e.le.PipingPiece is Pipe )
          .Where( e => ( (Pipe) e.le.PipingPiece ).Length > Tolerance.MergeToleranceForImporter ).ToList() ;

        using ( var writer =
          new StreamWriter(
            $"D:/Debug/BoundaryTargetIndex_{idfBp.Parent.Name}_{string.Join( "_", ( (Chiyoda.CAD.Topology.BlockPattern) idfBp.Parent ).Equipments.Select( e => e.EquipNo ) )}.txt",
            false, new UTF8Encoding( true ) ) ) {
          foreach ( var targetPipe in boundaryTargetPipes ) {
            writer.WriteLine($"{targetPipe.le.PipingPiece.Name}, {targetPipe.file}, {targetPipe.lineIndex}") ;
          }
        }
                
        var bnd =  Boundary.GetBounds(boundaryTargetPipes.Select(e=>e.le));
#else
        var bndTarget = ParseBoundaryTarget(boundaryTargetFiles) ;

        var bnd = Boundary.GetBounds( createdEdges.Values.SelectMany( lf => lf )
          .Where( b => bndTarget.Contains( new IdfFileIndexKey( b.file, b.lineIndex ) ) )
          .Select( e => e.le ) ) ;
#endif

        if (bnd != null) UpdateLeafEdgeCodSys( idfBp, createdEdges.Values.SelectMany( e => e ), bnd.Value.center );
        UpdateIdfBlockCodSys( bp, idfBp, idfDirCod.codSys );

        DeleteIdfs( idfBp, createdEdges, deleteIDFs );

        foreach ( var createdEdge in createdEdges) {
          var edges = createdEdge.Value.Where( e => e.le.PipingPiece != null ).Select(e=>e.le).ToList() ;
          if ( !edges.Any() ) {
            continue ;
          }
          if (createdEdgesAll.ContainsKey( createdEdge.Key )) {
            createdEdgesAll[createdEdge.Key].AddRange( edges );
          }
          else {
            createdEdgesAll.Add( createdEdge.Key, edges );
          }
        }
      }
      foreach ( var leafEdgeInfo in ParseVertex( vertices ) ) {
        AddVerties( bp, createdEdgesAll, leafEdgeInfo );
      }

      ApplySpecialMove( bp, createdEdgesAll, specialMove ) ;
      ApplySpecialDelete( bp, createdEdgesAll, specialDelete ) ;
      return bp;
    }

    static HashSet<IdfFileIndexKey> ParseBoundaryTarget(List<string> files)
    {
      var result = new HashSet<IdfFileIndexKey>();
      foreach ( var path in files ) {
        using ( var sr = new StreamReader( path, Encoding.UTF8 ) ) {
          while ( ! sr.EndOfStream ) {
            var line = sr.ReadLine() ;
            if ( string.IsNullOrEmpty( line ) ) {
              continue ;
            }
            var sp = line.Split( ',' ) ;
            result.Add( new IdfFileIndexKey( sp[ 1 ].Trim(), int.Parse( sp[ 2 ] ) ) ) ;
          }
        }
      }
      return result ;
    }

    static void UpdateLeafEdgeCodSys(Chiyoda.CAD.Topology.BlockPattern idfBp, IEnumerable<(LeafEdge le, string file, int lineIndex)> edges, Vector3d trans)
    {
      foreach ( var (edge, file, lineIndex) in edges ) {
        var p1 = idfBp.GlobalCod.GlobalizePoint( edge.LocalCod.Origin );
        var p2 = p1 + idfBp.GlobalCod.Origin - trans;
        var leafCod = idfBp.GlobalCod.LocalizePoint( p2 );
        edge.LocalCod = new LocalCodSys3d( leafCod, edge.LocalCod );
      }
    }

    static void UpdateIdfBlockCodSys(Chiyoda.CAD.Topology.BlockPattern bp, Chiyoda.CAD.Topology.BlockPattern idfBp, List<LocalCodSys3d> codSys)
    {
      if (codSys.Count <= 1) {
        return;
      }
      for (var i = 1; i < codSys.Count; ++i) {
        var cod = codSys[i];
        idfBp.LocalCod = new LocalCodSys3d( cod.Origin, idfBp.LocalCod.Rotation * cod.Rotation, cod.IsMirrorType );
      }
    }


    private static void DeleteIdfs(Chiyoda.CAD.Topology.BlockPattern idfBp, Dictionary<string, List<(LeafEdge le, string file, int lineIndex)>> createdEdges, List<string> deleteIDFs)
    {
      var leafPosition = new List<LeafEdgeInfo>() ;
      ParseDeleteIdfPosition( deleteIDFs, ref leafPosition ) ;

      var targets = new HashSet<(LeafEdge le, int lineIndex)>() ;
      foreach ( var info in leafPosition ) {
        foreach ( var (edge, lineIndex) in GetLeafEdgeLocal( info, createdEdges ) ) {
          targets.Add( ( edge, lineIndex ) ) ;
        }
      }

      foreach ( var (edge, lineIndex) in targets ) {
        idfBp.RemoveEdge( edge ) ;
        edge.Line.EraseByPipingPiece( edge.PipingPiece ) ;
      }
    }

    private static void ParseDeleteIdfPosition(List<string> deleteIDFs, ref List<LeafEdgeInfo> leafPosition)
    {
      foreach ( var path in deleteIDFs ) {
        using ( var sr = new StreamReader( path, Encoding.UTF8 ) ) {
          while (!sr.EndOfStream) {
            var line = sr.ReadLine();
            if (string.IsNullOrEmpty( line )) {
              continue;
            }

            var val = line.Split( ',' ).Select( item => item.Trim() ).ToArray();
            var li = new LeafEdgeInfo
              {LeafName = val[0], Position = new Vector3d( double.Parse( val[1] ), double.Parse( val[2] ), double.Parse( val[3] ) )};
            leafPosition.Add( li );
          }
        }
      }
    }

    /// <summary>
    /// Vertexの行に書かれた内容をパースする
    /// (外側の(エッジ, 座標), 内側の（エッジ、座標), 内側の（エッジ、座標), ...の順に書かれている
    /// 内側のエッジに接していない端点が付けたいvertexの位置となる
    /// </summary>
    /// <param name="vertices"></param>
    /// <returns></returns>
    private static List<List<LeafEdgeInfo>> ParseVertex(List<List<string>> vertices)
    {
      var leafEdgeInfos = new List<List<LeafEdgeInfo>>();
      foreach ( var v in vertices ) {
        var infos = new List<LeafEdgeInfo>();
        var inEdge = new LeafEdgeInfo
        {
          NozzleId = v[0],
          LeafName = v[1],
          Position = new Vector3d( double.Parse( v[2] ), double.Parse( v[3] ), double.Parse( v[4] ) )
        };
        infos.Add( inEdge );
        for (int i = 5; i < v.Count; i += 4) {
          var outEdge = new LeafEdgeInfo
          {
            LeafName = v[i],
            Position = new Vector3d( double.Parse( v[i + 1] ), double.Parse( v[i + 2] ), double.Parse( v[i + 3] ) )
          };
          infos.Add( outEdge );
        }
        leafEdgeInfos.Add( infos );
      }

      return leafEdgeInfos;
    }

    /// <summary>
    /// leafEdgeInfosの先頭要素にvertexを設定
    /// </summary>
    /// <param name="createdEdges"></param>
    /// <param name="leafEdgeInfos"></param>
    private static void AddVerties(Chiyoda.CAD.Topology.BlockPattern bp, Dictionary<string, List<LeafEdge>> createdEdges, List<LeafEdgeInfo> leafEdgeInfos)
    {
      List<Vector3d> innerTerms = new List<Vector3d>();
      foreach ( var nfo in leafEdgeInfos.Skip( 1 ) ) {
        foreach ( var edge in GetLeafEdgeRelative( bp, nfo, createdEdges ) ) {
          innerTerms.AddRange( edge.PipingPiece.ConnectPoints.Select( t => edge.GlobalCod.GlobalizePoint( t.Point ) ) );
        }
      }

      var curDoc = DocumentCollection.Instance.Current ?? DocumentCollection.Instance.CreateNew();

      var edges = GetLeafEdgeRelative( bp, leafEdgeInfos[0], createdEdges );
      var leafEdges = edges.ToList();
      if (!leafEdges.Any()) {
        return;
      }
      LeafEdge outerEdge = leafEdges.First();
      foreach ( var cp in outerEdge.PipingPiece.ConnectPoints ) {
        var p = outerEdge.GlobalCod.GlobalizePoint( cp.Point ) ;
        if ( innerTerms.Any( inn => Tolerance.MergeToleranceForImporter > Vector3d.Distance( p, inn ) ) ) continue ;
        var vertex = outerEdge.AddVertex( cp.ConnectPointNumber ) ;
        vertex.Name = leafEdgeInfos[ 0 ].NozzleId ;
        return ;
      }
    }

    /// <summary>
    /// idfBPから相対座標(LeafEdgeのローカル座標)
    /// </summary>
    /// <param name="leafEdgeInfo"></param>
    /// <param name="createdEdges"></param>
    /// <returns></returns>
    private static IEnumerable<(LeafEdge le, int lineIndex)> GetLeafEdgeLocal(LeafEdgeInfo leafEdgeInfo, Dictionary<string, List<(LeafEdge le, string file, int lineIndex)>> createdEdges)
    {
      if (!createdEdges.TryGetValue( leafEdgeInfo.LeafName, out var leafEdges )) yield break;
      foreach ( var (leafEdge, file, lineIndex) in leafEdges ) {
        if (Tolerance.MergeToleranceForImporter > Vector3d.Distance(
              leafEdge.LocalCod.Origin,
              leafEdgeInfo.Position )) {
          yield return (leafEdge, lineIndex);
        }
      }
    }
    
    /// <summary>
    /// 所属する親のブロックパターンからの相対座標
    /// vertexの場合はidfを反転して使うのでidfBPからの相対座標では個別に指定する事ができない
    /// </summary>
    /// <param name="idfBp"></param>
    /// <param name="leafEdgeInfo"></param>
    /// <param name="createdEdges"></param>
    /// <returns></returns>
    private static IEnumerable<LeafEdge> GetLeafEdgeRelative(Chiyoda.CAD.Topology.BlockPattern bp, LeafEdgeInfo leafEdgeInfo, Dictionary<string, List<LeafEdge>> createdEdges)
    {
      if (!createdEdges.TryGetValue( leafEdgeInfo.LeafName, out var leafEdges )) yield break;
      foreach ( var leafEdge in leafEdges ) {
        var p = bp.LocalCod.LocalizePoint( leafEdge.GlobalCod.Origin );
        if (Tolerance.MergeToleranceForImporter > Vector3d.Distance(
              p,
              leafEdgeInfo.Position )) {
          yield return leafEdge;
        }
      }
    }

    /// <summary>
    /// デモ用特別機能
    /// 自動ルーティングがAirFinCooloerのパイプラックと平行な抜きに対応出来ていないのでため
    /// </summary>
    /// <param name="bp"></param>
    /// <param name="createdEdges"></param>
    /// <param name="specialMoves"></param>
    private static void ApplySpecialMove(Chiyoda.CAD.Topology.BlockPattern bp, Dictionary<string, List<LeafEdge>> createdEdges, List<List<string>> specialMoves)
    {
      foreach ( var move in specialMoves ) {
        if ( move.Count < 7 ) {
          continue ;
        }

        if ( ! createdEdges.TryGetValue( move[ 0 ], out var leafEdges ) ) continue ;
        foreach ( var leafEdge in leafEdges ) {
          var p = bp.LocalCod.LocalizePoint( leafEdge.GlobalCod.Origin ) ;
          if ( Tolerance.MergeToleranceForImporter <= Vector3d.Distance(
                 p,
                 new Vector3d( double.Parse( move[ 1 ] ), double.Parse( move[ 2 ] ), double.Parse( move[ 3 ] ) ) ) ) {
            continue ;
          }

          var cod = leafEdge.LocalCod ;
          var org = cod.Origin + new Vector3d( double.Parse( move[ 4 ] ), double.Parse( move[ 5 ] ),
                      double.Parse( move[ 6 ] ) ) ;
          var rot = cod.Rotation ;
          if ( move.Count == 8 ) {
            rot = ImportUtil.ParseRot( move[ 7 ] ) * rot ;
          }
          leafEdge.LocalCod = new LocalCodSys3d( org, rot, cod.IsMirrorType ) ;
        }
      }
    }

    /// <summary>
    /// デモ用特別機能
    /// 自動ルーティングがAirFinCooloerのパイプラックと平行な抜きに対応出来ていないのでため
    /// </summary>
    /// <param name="bp"></param>
    /// <param name="createdEdges"></param>
    /// <param name="specialDelete"></param>
    private static void ApplySpecialDelete( Chiyoda.CAD.Topology.BlockPattern bp,
      Dictionary<string, List<LeafEdge>> createdEdges, List<List<string>> specialDelete )
    {
      foreach ( var del in specialDelete ) {
        if ( del.Count != 4 ) {
          continue ;
        }

        if ( ! createdEdges.TryGetValue( del[ 0 ], out var leafEdges ) ) continue ;
        foreach ( var leafEdge in leafEdges ) {
          var p = bp.LocalCod.LocalizePoint( leafEdge.GlobalCod.Origin ) ;
          if ( Tolerance.MergeToleranceForImporter <= Vector3d.Distance(
                 p,
                 new Vector3d( double.Parse( del[ 1 ] ), double.Parse( del[ 2 ] ), double.Parse( del[ 3 ] ) ) ) ) {
            continue ;
          }

          foreach ( var block in bp.Children
            .Where( b => b is Chiyoda.CAD.Topology.BlockPattern )
            .Where( b => ( (Chiyoda.CAD.Topology.BlockPattern) b ).Type == BlockPatternType.Type.Idf ) ) {
            var b = (Chiyoda.CAD.Topology.BlockPattern) block ;
            b.RemoveEdge( leafEdge ) ;
            leafEdge.Line.EraseByPipingPiece( leafEdge.PipingPiece ) ;
            break ;
          }
        }
      }
    }
  }
}
