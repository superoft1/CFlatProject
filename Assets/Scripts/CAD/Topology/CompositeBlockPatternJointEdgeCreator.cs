using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using System.Threading.Tasks ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Chiyoda.CAD.Topology
{
  class CompositeBlockPatternJointEdgeCreator
  {
    private readonly CompositeBlockPattern _compositeBlockPattern ;
    private readonly CompositeBlockPattern.JoinType _joinType ;
    private readonly string _firstPipeName ;

    public CompositeBlockPatternJointEdgeCreator( CompositeBlockPattern compositeBlockPattern, CompositeBlockPattern.JoinType joinType, string firstPipeName )
    {
      _compositeBlockPattern = compositeBlockPattern ;
      _joinType = joinType ;
      _firstPipeName = firstPipeName ;
    }

    public LeafEdge[] CreateEdges( LeafEdge firstEdge, IList<HalfVertex> vertices )
    {
      var jointEdges = CreateHeaderEdges( firstEdge, vertices ) ;
      if ( 1 < jointEdges.Length ) jointEdges[ 1 ].ObjectName = _firstPipeName ;  // 最初のパイプの名称を変更

      var result = new LeafEdge[ jointEdges.Length + vertices.Count ] ;
      Array.Copy( jointEdges, result, jointEdges.Length ) ;

      var vertexLocalPos = _compositeBlockPattern.BaseBlockPattern.GlobalCod.LocalizePoint( vertices[ 0 ].GlobalPoint ) ;
      for ( int i = 0, j = jointEdges.Length, n = vertices.Count ; i < n ; ++i, ++j ) {
        var p = _compositeBlockPattern.GetBlockPattern( i ).LocalCod.GlobalizePoint( vertexLocalPos ) ;
        result[ j ] = CreateExtenderEdge( vertices[ i ], p ) ;
      }
      return result ;
    }

    private LeafEdge[] CreateHeaderEdges( LeafEdge firstEdge, IList<HalfVertex> vertices )
    {
      var jointEdges = CreateJointEdgeBase( firstEdge, vertices ).ToArray() ;

      switch ( _joinType ) {
        case CompositeBlockPattern.JoinType.StraightInnerDir : return FilterForStraightInnerDir( jointEdges ) ;
        case CompositeBlockPattern.JoinType.StraightOuterDir : return FilterForStraightOuterDir( jointEdges ) ;
        case CompositeBlockPattern.JoinType.MiddleTeeInnerDir : return FilterForMiddleTeeInnerDir( jointEdges, 0 ) ;
        case CompositeBlockPattern.JoinType.MiddleTeeOuterDir : return FilterForMiddleTeeOuterDir( jointEdges, 0 ) ;
        case CompositeBlockPattern.JoinType.MiddleTeeInnerDir90 : return FilterForMiddleTeeInnerDir( jointEdges, 90 ) ;
        case CompositeBlockPattern.JoinType.MiddleTeeOuterDir90 : return FilterForMiddleTeeOuterDir( jointEdges, 90 ) ;
        case CompositeBlockPattern.JoinType.MiddleTeeInnerDir180 : return FilterForMiddleTeeInnerDir( jointEdges, 180 ) ;
        case CompositeBlockPattern.JoinType.MiddleTeeOuterDir180 : return FilterForMiddleTeeOuterDir( jointEdges, 180 ) ;
        case CompositeBlockPattern.JoinType.MiddleTeeInnerDir270 : return FilterForMiddleTeeInnerDir( jointEdges, 270 ) ;
        case CompositeBlockPattern.JoinType.MiddleTeeOuterDir270 : return FilterForMiddleTeeOuterDir( jointEdges, 270 ) ;
        default : return jointEdges ;
      }
    }

    private LeafEdge CreateExtenderEdge( HalfVertex vertex, Vector3d vertexPos )
    {
      var v1 = vertex ;
      var v2 = vertex.Partner ;
      if ( null == v2 ) throw new InvalidOperationException() ;

      var bpaCod = _compositeBlockPattern.GlobalCod ;
      var pos2 = v2.LeafEdge.LocalCod.GlobalizePoint( v2.ConnectPoint.Point ) ;  // v2はまだCompositeBlockPatternに登録されていないため、LeafEdgeのみを使用して取得

      v1.Partner = null ;
      
      Vector3d xDir = bpaCod.LocalizeVector( v1.LeafEdge.GlobalCod.GlobalizeVector( v1.GetConnectVector() ) ) ;

      return CreatePipeEdge( v1, vertexPos, v2, pos2, v1.ConnectPoint.Diameter.OutsideMeter, xDir ) ;
    }

    private LeafEdge[] FilterForStraightInnerDir( LeafEdge[] jointEdges )
    {
      jointEdges[ jointEdges.Length - 1 ] = SwapEndJointEdge( jointEdges[ jointEdges.Length - 1 ] ) ;
      jointEdges[ jointEdges.Length - 2 ].ChangeEdgeLengthFromEndVertices() ;

      return jointEdges ;
    }

    private LeafEdge[] FilterForStraightOuterDir( LeafEdge[] jointEdges )
    {
      jointEdges[ 0 ] = SwapStartJointEdge( jointEdges[ 0 ] ) ;
      jointEdges[ 1 ].ChangeEdgeLengthFromEndVertices() ;
      return jointEdges ;
    }

    private LeafEdge[] FilterForMiddleTeeInnerDir( LeafEdge[] jointEdges, int rotationDegreeT )
    {
      if ( ( jointEdges[ 1 ].PipingPiece as Pipe ).Length < ( jointEdges[ 0 ].PipingPiece as PipingTee ).MainLength ) {
        // パイプが短すぎるので FilterForStraightInnerDir と一致
        return FilterForStraightInnerDir( jointEdges ) ;
      }

      // 最初と最後の要素をElbowに変え、途中にTを挿入
      // ①━②┻③━④ または ①━②┻③━④━⑤
      return FilterForMiddleTee( jointEdges, ( ( jointEdges.Length - 2 ) / 4 ) * 2 + 1, rotationDegreeT ) ;
    }

    private LeafEdge[] FilterForMiddleTeeOuterDir( LeafEdge[] jointEdges, int rotationDegreeT )
    {
      if ( ( jointEdges[ 1 ].PipingPiece as Pipe ).Length < ( jointEdges[ 0 ].PipingPiece as PipingTee ).MainLength ) {
        // パイプが短すぎるので FilterForStraightOuterDir と一致
        return FilterForStraightOuterDir( jointEdges ) ;
      }

      // 最初と最後の要素をElbowに変え、途中にTを挿入
      // ①━②┻③━④ または ①━②━③┻④━⑤
      return FilterForMiddleTee( jointEdges, ( jointEdges.Length / 4 ) * 2 + 1, rotationDegreeT ) ;
    }

    private LeafEdge[] FilterForMiddleTee( LeafEdge[] jointEdges, int index, int rotationDegreeT )
    {
      var oldTee = jointEdges[ 0 ] ;
      var oldPipe = jointEdges[ index ] ;
      jointEdges[ 0 ] = SwapStartJointEdge( jointEdges[ 0 ] ) ;
      jointEdges[ 1 ].ChangeEdgeLengthFromEndVertices() ;
      jointEdges[ jointEdges.Length - 1 ] = SwapEndJointEdge( jointEdges[ jointEdges.Length - 1 ] ) ;
      jointEdges[ jointEdges.Length - 2 ].ChangeEdgeLengthFromEndVertices() ;

      ReverseBranch( oldTee ) ;

      LeafEdge[] appendEdges = null ;
      if ( index * 2 + 1 == jointEdges.Length ) {
        // 中央に挿入
        var v1 = oldPipe.GetVertex( (int) Pipe.ConnectPointType.Term1 ) ;
        var v2 = oldPipe.GetVertex( (int) Pipe.ConnectPointType.Term2 ) ;
        var pipeLength = Vector3d.Distance( v1.ConnectPoint.Point, v2.ConnectPoint.Point ) ;
        var teeLength = Vector3d.Distance( oldTee.GetVertex( (int) PipingTee.ConnectPointType.MainTerm1 ).ConnectPoint.Point, oldTee.GetVertex( (int) PipingTee.ConnectPointType.MainTerm2 ).ConnectPoint.Point ) ;
        oldTee.LocalCod = new LocalCodSys3d( oldPipe.LocalCod.Origin, oldTee.LocalCod ) ;
        if ( Math.Abs( pipeLength - teeLength ) < Tolerance.DistanceTolerance ) {
          // 純粋に置換
          jointEdges[ index ] = oldTee ;
          ReplacePartner( oldPipe.GetVertex( (int) Pipe.ConnectPointType.Term1 ), oldTee.GetVertex( (int) PipingTee.ConnectPointType.MainTerm2 ) ) ;
          ReplacePartner( oldPipe.GetVertex( (int) Pipe.ConnectPointType.Term2 ), oldTee.GetVertex( (int) PipingTee.ConnectPointType.MainTerm1 ) ) ;
        }
        else {
          // 置換して左右にパイプを作成
          var v3 = v2.Partner ;
          v2.Partner = null ;
          var vt1 = oldTee.GetVertex( (int) PipingTee.ConnectPointType.MainTerm2 ) ;
          var vt2 = oldTee.GetVertex( (int) PipingTee.ConnectPointType.MainTerm1 ) ;
          v2.Partner = vt1 ;
          oldPipe.ChangeEdgeLengthFromEndVertices() ;
          var pos1 = vt2.LeafEdge.LocalCod.GlobalizePoint( vt2.ConnectPoint.Point ) ;
          var pos2 = v3.LeafEdge.LocalCod.GlobalizePoint( v3.ConnectPoint.Point ) ;
          var appendEdge = CreatePipeEdge( vt2, pos1, v3, pos2, vt2.ConnectPoint.Diameter.OutsideMeter,oldPipe.LocalCod.DirectionX ) ;
          appendEdges = new[] { oldTee, appendEdge } ;
        }
      }
      else if ( index * 2 + 1 < jointEdges.Length ) {
        // 指定パイプと次のティーの間に作成
        var v2 = oldPipe.GetVertex( (int) Pipe.ConnectPointType.Term2 ) ;
        var v3 = v2.Partner ;
        v2.Partner = null ;
        v2.Partner = oldTee.GetVertex( (int) PipingTee.ConnectPointType.MainTerm2 ) ;
        v3.Partner = oldTee.GetVertex( (int) PipingTee.ConnectPointType.MainTerm1 ) ;
        var pos1 = v3.LeafEdge.LocalCod.GlobalizePoint( v3.ConnectPoint.Point ) ;
        var pos2 = oldTee.LocalCod.GlobalizePoint( v3.Partner.ConnectPoint.Point ) ;
        oldTee.LocalCod = oldTee.LocalCod.Translate( pos1 - pos2 ) ;
        oldPipe.ChangeEdgeLengthFromEndVertices() ;
        appendEdges = new[] { oldTee } ;
      }
      else {
        // 指定パイプと前のティーの間に作成
        var v1 = oldPipe.GetVertex( (int) Pipe.ConnectPointType.Term1 ) ;
        var v0 = v1.Partner ;
        v1.Partner = null ;
        v1.Partner = oldTee.GetVertex( (int) PipingTee.ConnectPointType.MainTerm1 ) ;
        v0.Partner = oldTee.GetVertex( (int) PipingTee.ConnectPointType.MainTerm2 ) ;
        var pos1 = v0.LeafEdge.LocalCod.GlobalizePoint( v0.ConnectPoint.Point ) ;
        var pos2 = oldTee.LocalCod.GlobalizePoint( v0.Partner.ConnectPoint.Point ) ;
        oldTee.LocalCod = oldTee.LocalCod.Translate( pos1 - pos2 ) ;
        oldPipe.ChangeEdgeLengthFromEndVertices() ;
        appendEdges = new[] { oldTee } ;
      }

      if ( 0 != rotationDegreeT ) {
        double rotationRadianT = ( (double) rotationDegreeT ).Deg2Rad() ;
        var teeCod = oldTee.LocalCod ;
        var y = Math.Cos( rotationRadianT ) * teeCod.DirectionY + Math.Sin( rotationRadianT ) * teeCod.DirectionZ ;
        oldTee.LocalCod = new LocalCodSys3d( teeCod.Origin, teeCod.DirectionX, y, Vector3d.zero ) ;
      }

      if ( null == appendEdges ) {
        return jointEdges ;
      }
      else {
        return ConcatArrays( jointEdges, appendEdges ) ;
      }
    }

    private static T[] ConcatArrays<T>( T[] array1, T[] array2 )
    {
      var result = new T[ array1.Length + array2.Length ] ;
      Array.Copy( array1, result, array1.Length ) ;
      Array.Copy( array2, 0, result, array1.Length, array2.Length ) ;
      return result ;
    }

    private void ReverseBranch( LeafEdge tee )
    {
      var codsys = tee.LocalCod ;
      tee.LocalCod = new LocalCodSys3d( codsys.Origin, codsys.DirectionX, -codsys.DirectionY, -codsys.DirectionZ ) ;
    }

    private static LeafEdge SwapStartJointEdge( LeafEdge startTeeEdge )
    {
      var tee = startTeeEdge.PipingPiece as PipingTee ;
      var elbow = startTeeEdge.Document.CreateEntity<PipingElbow90>() ;
      elbow.ChangeSizeNpsMm( 0, DiameterFactory.FromOutsideMeter( tee.MainDiameter ).NpsMm ) ;

      var teeLocalCod = startTeeEdge.LocalCod ;
      var leafEdge = startTeeEdge.Document.CreateLeafEdge( elbow ) ;
      leafEdge.LocalCod = new LocalCodSys3d( teeLocalCod.Origin, -teeLocalCod.DirectionY, teeLocalCod.DirectionX, teeLocalCod.DirectionZ ) ;

      ReplacePartner( startTeeEdge.GetVertex( (int) PipingTee.ConnectPointType.MainTerm1 ), leafEdge.GetVertex( (int) PipingElbow90.ConnectPointType.Term1 ) ) ;
      ReplacePartner( startTeeEdge.GetVertex( (int) PipingTee.ConnectPointType.BranchTerm ), leafEdge.GetVertex( (int) PipingElbow90.ConnectPointType.Term2 ) ) ;

      return leafEdge ;
    }

    private LeafEdge SwapEndJointEdge( LeafEdge endTeeEdge )
    {
      var tee = endTeeEdge.PipingPiece as PipingTee ;
      var elbow = endTeeEdge.Document.CreateEntity<PipingElbow90>() ;
      elbow.ChangeSizeNpsMm( 0, DiameterFactory.FromOutsideMeter(tee.MainDiameter).NpsMm) ;

      var teeLocalCod = endTeeEdge.LocalCod ;
      var leafEdge = endTeeEdge.Document.CreateLeafEdge( elbow ) ;
      leafEdge.LocalCod = teeLocalCod ;

      ReplacePartner( endTeeEdge.GetVertex( (int) PipingTee.ConnectPointType.BranchTerm ), leafEdge.GetVertex( (int) PipingElbow90.ConnectPointType.Term1  ) ) ;
      ReplacePartner( endTeeEdge.GetVertex( (int) PipingTee.ConnectPointType.MainTerm2 ), leafEdge.GetVertex( (int) PipingElbow90.ConnectPointType.Term2 ) ) ;

      return leafEdge ;
    }

    private IEnumerable<LeafEdge> CreateJointEdgeBase( LeafEdge firstEdge, IList<HalfVertex> vertices )
    {
      var firstCodSys = GetFirstCodSys( firstEdge, vertices[ 0 ] ) ;

      var firstTee = CreateTeeEdge( firstCodSys, vertices[ 0 ] ) ;
      yield return firstTee ;

      var lastTee = firstTee ;
      var vertexLocalPos = _compositeBlockPattern.BaseBlockPattern.GlobalCod.LocalizePoint( vertices[ 0 ].GlobalPoint ) ;
      var pos0 = _compositeBlockPattern.BaseBlockPattern.LocalCod.GlobalizePoint( vertexLocalPos ) ;
      for ( int i = 1, n = vertices.Count ; i < n ; ++i ) {
        var posi = _compositeBlockPattern.GetBlockPattern( i ).LocalCod.GlobalizePoint( vertexLocalPos ) ;
        var nextTee = CopyTeeEdge( firstTee, posi - pos0, vertices[ i ] ) ;
        yield return CreatePipeEdge( lastTee, nextTee ) ;
        yield return nextTee ;
        lastTee = nextTee ;
      }
    }

    private LocalCodSys3d GetFirstCodSys( LeafEdge edge, HalfVertex vertex )
    {
      var p = vertex.ConnectPoint.Point ;
      var v = vertex.GetConnectVector() ;
      var bpGlobalCod = _compositeBlockPattern.GlobalCod ;
      var edgeGlobalCod = edge.GlobalCod ;

      var vertexOrg = bpGlobalCod.LocalizePoint( edgeGlobalCod.GlobalizePoint( p ) ) ;
      var vertexDir = bpGlobalCod.LocalizeVector( edgeGlobalCod.GlobalizeVector( v ) ) ;
      var xDir = _compositeBlockPattern.GetBlockPattern( 1 ).LocalCod.Origin - _compositeBlockPattern.BaseBlockPattern.LocalCod.Origin ;

      return new LocalCodSys3d( vertexOrg, xDir, -vertexDir, Vector3d.zero ) ;
    }

    private LeafEdge CreateTeeEdge( LocalCodSys3d codsys, HalfVertex branchVertex )
    {
      var cp = branchVertex.ConnectPoint ;

      var tee = _compositeBlockPattern.Document.CreateEntity<PipingTee>() ;
      tee.MainDiameter =  cp.Diameter.OutsideMeter ;
      tee.BranchDiameter = cp.Diameter.OutsideMeter ;

      tee.MainLength = PipingTee.GetDefaultMainLength( cp.Diameter, cp.Diameter ) ;
      tee.BranchLength = PipingTee.GetDefaultBranchLength( cp.Diameter, cp.Diameter ) ;

      var diff = Math.Max( tee.BranchLength, PipingElbow90.GetDefaultLongBendLength( cp.Diameter ) ) ;

      var leafEdge = _compositeBlockPattern.Document.CreateLeafEdge( tee ) ;
      leafEdge.LocalCod = codsys.Translate( -diff * codsys.DirectionY ) ;

      leafEdge.GetVertex( (int) PipingTee.ConnectPointType.BranchTerm ).Partner = branchVertex ;

      return leafEdge ;
    }

    private static LeafEdge CreatePipeEdge( LeafEdge lastTee, LeafEdge nextTee )
    {
      var v1 = lastTee.GetVertex( (int) PipingTee.ConnectPointType.MainTerm1 ) ;
      var v2 = nextTee.GetVertex( (int) PipingTee.ConnectPointType.MainTerm2 ) ;

      var pos1 = lastTee.LocalCod.GlobalizePoint( v1.ConnectPoint.Point ) ;
      var pos2 = nextTee.LocalCod.GlobalizePoint( v2.ConnectPoint.Point ) ;

      return CreatePipeEdge( v1, pos1, v2, pos2, v1.ConnectPoint.Diameter.OutsideMeter, lastTee.LocalCod.DirectionX ) ;
    }

    private static LeafEdge CreatePipeEdge( HalfVertex v1, in Vector3d pos1, HalfVertex v2, in Vector3d pos2, double diameter, in Vector3d dir )
    {
      var pipe = v1.Document.CreateEntity<Pipe>() ;
      pipe.Length = ( pos2 - pos1 ).magnitude ;
      pipe.Diameter = diameter ;

      var leafEdge = v1.Document.CreateLeafEdge( pipe ) ;
      leafEdge.LocalCod = new LocalCodSys3d( ( pos1 + pos2 ) / 2, dir, Vector3d.zero, Vector3d.zero ) ;

      leafEdge.GetVertex( (int) Pipe.ConnectPointType.Term1 ).Partner = v1 ;
      leafEdge.GetVertex( (int) Pipe.ConnectPointType.Term2 ).Partner = v2 ;

      return leafEdge ;
    }

    private LeafEdge CopyTeeEdge( LeafEdge teeEdge, Vector3d dir, HalfVertex branchVertex )
    {
      LeafEdge copiedLeafEdge ;
      using ( var storage = new CopyObjectStorage() ) {
        copiedLeafEdge = teeEdge.Clone( storage ) ;
      }

      copiedLeafEdge.LocalCod = copiedLeafEdge.LocalCod.Translate( dir ) ;

      copiedLeafEdge.GetVertex( (int) PipingTee.ConnectPointType.BranchTerm ).Partner = branchVertex ;

      return copiedLeafEdge ;
    }

    private static void ReplacePartner( HalfVertex oldEdgeVertex, HalfVertex newEdgeVertex )
    {
      var v = oldEdgeVertex.Partner ;
      if ( null == v ) {
        newEdgeVertex.Partner = null ;
      }
      else {
        v.Partner = null ;
        newEdgeVertex.Partner = v ;
      }
    }
  }
}