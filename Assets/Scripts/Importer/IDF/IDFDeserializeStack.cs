using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Chiyoda.CAD.Util ;
using UnityEngine ;

namespace IDF
{
  public class IDFImporterStacks
  {
    /// <summary>
    /// 枝が派生する毎にIDFEntityImporterのListを生成して保持し、枝が末端まで達したら随時枝を削除する
    /// </summary>
    private RecordManager RecordManager { get ; set ; }

    private Document Doc { get ; set ; }
    private Line Line { get ; set ; }
    private string FilePath { get ; set ; }
    private string PipingSpecification { get ; set ; }

    private bool ShouldCreateVertex { get ; set ; }


    public IDFImporterStacks( Document doc, Line line, string path, string pipingSpecification,
      bool shouldCreateVertex = true )
    {
      Doc = doc ;
      Line = line ;
      RecordManager = new RecordManager( Line ) ;
      FilePath = path ;
      PipingSpecification = pipingSpecification ;
      ShouldCreateVertex = shouldCreateVertex ;
    }

    private void CreateBranch( IDFEntityImporter importer )
    {
      RecordManager.AddAsBranch( importer ) ;
    }

    public void Add( IDFEntityImporter importer )
    {
      if ( RecordManager.FrontOfCurrentBranch() == null ) {
        CreateBranch( importer ) ;
        return ;
      }

      var impLast = RecordManager.LastOfCurrentBranch() ;
      if ( importer.FittingType == IDFRecordType.FittingType.Olet ) {
        CreateBranch( importer ) ;
      }
      else if ( impLast.entityState == IDFEntityImporter.EntityState.In ||
                impLast.entityState == IDFEntityImporter.EntityState.Out ||
                impLast.FittingType == IDFRecordType.FittingType.Olet ) {
        RecordManager.Add( importer ) ;
      }
      else {
        CreateBranch( importer ) ;
      }
    }

    public void Build()
    {
      BuildImpl() ;

      BuildEccentricReducer() ;

      BuildSupport() ;

      if ( ShouldCreateVertex ) {
        CreateVertexAll() ;
      }
    }

    /// <summary>
    /// oletやstubのvertexは後から設定したいので、一旦避けて後回し
    /// </summary>
    /// <param name="oletOrStubs"></param>
    /// <param name="oletOrStubRecord"></param>
    private void CollectOletOrStub(List<(HalfVertex pipingVertex, List<PipingPiece> oletOrStubs)> oletOrStubs, Record oletOrStubRecord )
    {
      var prevPiping = oletOrStubRecord.PrevRecord.Body.LeafEdge.PipingPiece ;
      if ( oletOrStubRecord.PrevRecord.Children.Count > 1 &&
           oletOrStubRecord.PrevRecord.Body.LeafEdge.PipingPiece is StubInReinforcingWeld ) {
        prevPiping = oletOrStubRecord.PrevRecord.PrevRecord.Body.LeafEdge.PipingPiece ;
      }

      var oletOrStub = oletOrStubRecord.Body.LeafEdge.PipingPiece ;
      var found = oletOrStubs.FirstOrDefault( o => o.pipingVertex.LeafEdge.PipingPiece == prevPiping ) ;
      if ( found.Equals( default ) ) {
        var v = GetNearestVertexOfPipe( prevPiping, oletOrStub ) ;
        oletOrStubs.Add( ( v, new List<PipingPiece> { oletOrStub } ) ) ;
      }
      else {
        found.oletOrStubs.Add( oletOrStub ) ;
      }
    }

    /// <summary>
    /// oletやstubに接続しているブランチの配管を取得
    /// </summary>
    /// <param name="branchOriginEdges"></param>
    /// <param name="record"></param>
    private void CollectBranchOriginPiece(List<(LeafEdge branch, PipingPiece oletOrStub)> branchOriginEdges, Record record)
    {
      if ( record.PrevRecord?.Body.LeafEdge.PipingPiece is WeldOlet ol ) {
        branchOriginEdges.Add( ( record.Body.LeafEdge, ol ) ) ;
      }
      else {
        if ( ! IsBranchTop( record ) ) return ;
        var parentPiping = record.ParentBranch?.Body.LeafEdge.PipingPiece ;
        if ( IsDegeneratedMainTerm( parentPiping ) ) {
          branchOriginEdges.Add( ( record.Body.LeafEdge, parentPiping ) ) ;
        }
      }
    }

    /// <summary>
    /// コンポーネントの周りにvertexをすべて作成
    /// </summary>
    private void CreateVertexAll()
    {
      var oletOrStubs = new List<(HalfVertex pipingVertex, List<PipingPiece> oletOrStubs)>() ;
      var branchOriginPieces = new List<(LeafEdge branch, PipingPiece oletOrStub)>() ;
      foreach ( var records in RecordManager.AllRecords() ) {
        foreach ( var record in records.Where( r => r.BuildSuccess ) ) {
          var bodyLeafEdge = record.Body.LeafEdge ;
          switch ( bodyLeafEdge.PipingPiece ) {
            case WeldOlet _ :
            case StubInReinforcingWeld _ :
              // oletやstubのvertexは後から設定したいので、一旦避けて後回し
              CollectOletOrStub( oletOrStubs, record ) ;

              break ;
            default :
              Doc.CreateHalfVerticesAndMakePairs( bodyLeafEdge ) ;
              // branch側のcomponentは後で接続しなおす
              // stubでTern1,2が同位置にある場合に、自動でvertexを設定すると意図しないvertexと接続してしまうため
              CollectBranchOriginPiece( branchOriginPieces, record ) ;

              break ;
          }
        }
      }

      // 後回しにしていたvertexを作成をここで行う
      foreach ( var (_, oss) in oletOrStubs ) {
        oss.ForEach(o=>o.LeafEdge.CreateAllHalfVertices());
      }

      // ブランチ側のComponentを分岐元のBranchVertexと接続させる
      foreach ( var (branch, oletOrStub) in branchOriginPieces ) {
        var free = branch.Vertices.FirstOrDefault( v => v.Partner == null ) ;
        if ( free != null ) {
          free.Partner = oletOrStub.LeafEdge.GetVertex( (int) WeldOlet.ConnectPointType.BranchTerm ) ;
        }
      }

      // oletやstubのvertexをここで設定
      foreach ( var (pipingVertex, oss) in oletOrStubs ) {
        var pipePartner = pipingVertex.Partner ;
        HalfVertex backVertex = pipingVertex ;
        foreach ( var t in oss ) {
          var nearestVertex = t.LeafEdge.GetVertex( (int) WeldOlet.ConnectPointType.MainTerm1 ) ;
          var anotherVertex = t.LeafEdge.GetVertex( (int) WeldOlet.ConnectPointType.MainTerm2 ) ;
          if ( nearestVertex.Partner != null ) {
            nearestVertex.Partner = null ;
          }

          if ( anotherVertex.Partner != null ) {
            anotherVertex.Partner = null ;
          }

          if ( null == backVertex.Partner ) {
            backVertex.Partner = nearestVertex ;
          }
          else if ( backVertex.Partner.LeafEdge == t.LeafEdge ) {
            // CreateHalfVerticesAndMakePairsの中で自動的にペアリングされたため、Diameter設定のみ
          }
          else {
            backVertex.Partner = null ;
            backVertex.Partner = nearestVertex ;
          }

          backVertex = anotherVertex ;

          var diameter = backVertex.ConnectPoint.Diameter ;
          nearestVertex.ConnectPoint.Diameter = diameter ;
          anotherVertex.ConnectPoint.Diameter = diameter ;
        }

        backVertex.Partner = null ;
        backVertex.Partner = pipePartner ;
      }
    }

    private bool IsDegeneratedMainTerm( PipingPiece piece )
    {
      const double tol = Tolerance.DistanceTolerance ;
      return Vector3d.Distance( piece.GetConnectPoint( 0 ).GlobalPoint, piece.GetConnectPoint( 1 ).GlobalPoint ) < tol ;
    }

    private bool IsBranchTop( Record record )
    {
      if ( record.ParentBranch != null ) {
        return record.ParentBranch?.Body.LeafEdge == record.PrevRecord?.Body.LeafEdge ;
      }

      return false ;
    }

    /// <summary>
    /// oletに近い側のPipeのHalfVertexを取得
    /// </summary>
    /// <param name="piping"></param>
    /// <param name="oletOrStub"></param>
    /// <returns></returns>
    private static HalfVertex GetNearestVertexOfPipe( PipingPiece piping, PipingPiece oletOrStub )
    {
      int connectPointNumber = -1 ;
      double length2 = double.MaxValue ;
      foreach ( var os in oletOrStub.ConnectPoints ) {
        foreach ( var cp in piping.ConnectPoints ) {
          var l2 = ( cp.GlobalPoint - os.GlobalPoint ).sqrMagnitude ;
          if ( l2 < length2 ) {
            connectPointNumber = cp.ConnectPointNumber ;
            length2 = l2 ;
          }
        }
      }

      var v = piping.LeafEdge.GetVertex( connectPointNumber ) ;
      if ( v == null ) {
        throw new InvalidOperationException() ;
      }

      return v ;
    }


    private void BuildEccentricReducer()
    {
      foreach ( var records in RecordManager.AllRecords() ) {
        var recordSuccessed = records.Where( r => r.BuildSuccess ).ToList() ;
        for ( int i = 0 ; i < recordSuccessed.Count ; ++i ) {
          var reduImp = recordSuccessed[ i ].Body as IDFReducerImporter ;

          if ( ! ( reduImp?.Entity is EccentricPipingReducerCombination ) ) {
            continue ;
          }

          var neighborEdges = new List<LeafEdge>() ;
          if ( i > 0 ) {
            neighborEdges.Add( recordSuccessed[ i - 1 ].Body.LeafEdge ) ;
            if ( i + 1 < recordSuccessed.Count ) {
              neighborEdges.Add( recordSuccessed[ i + 1 ].Body.LeafEdge ) ;
            }
          }
          else if ( i == 0 ) {
            if ( recordSuccessed[ i ].ParentBranch != null && recordSuccessed[ i ].ParentBranch.BuildSuccess ) {
              neighborEdges.Add( recordSuccessed[ i ].ParentBranch.Body.LeafEdge ) ;
            }

            if ( i + 1 < recordSuccessed.Count ) {
              neighborEdges.Add( recordSuccessed[ i + 1 ].Body.LeafEdge ) ;
            }
          }

          foreach ( var edge in neighborEdges ) {
            if ( CreateEccentricReducer( reduImp, edge ) ) {
              break ;
            }
          }
        }
      }
    }


    private void BuildImpl()
    {
      foreach ( var records in RecordManager.AllRecords() ) {
        var sameBranch = records.Where( r => r.BuildSuccess ).ToList() ;
        foreach ( var record in sameBranch ) {
          LeafEdge prev = null ;
          if ( record.PrevRecord != null ) {
            prev = record.PrevRecord.Body.LeafEdge ;
          }

          var ret = record.Body.Build( prev, record.NextRecord?.Body ) ;
          record.BuildSuccess = ret == IDFEntityImporter.ErrorPosition.None ;
          if ( record.BuildSuccess ) continue ;
          Line.EraseByPipingPiece( record.Body.LeafEdge.PipingPiece ) ;
          if ( record.Body.LeafEdge.PipingPiece is WeldNeckFlange ) {
            // 端についているFlangeの軸長が入ってないケースが割とあるのでErrorとせずにWarningとする
            Debug.LogWarning(
              $"IDF Importer Failed. Component: {record.Body.LeafEdge.PipingPiece.Name}, \nFile: {FilePath}" ) ;
          }
          else {
            Debug.LogError(
              $"IDF Importer Failed. Component: {record.Body.LeafEdge.PipingPiece.Name}, \nFile: {FilePath}" ) ;
          }
        }
      }
    }


    private void BuildSupport()
    {
      foreach ( var record in RecordManager.Support ) {
        if ( RecordManager.AllImporter().Any( i => i == record.PrevRecord.Body ) ) {
          record.Body.Build( record.PrevRecord.Body.LeafEdge, null ) ;
        }
      }
    }

    private bool CreateEccentricReducer( IDFReducerImporter reduImporter, LeafEdge neighbor )
    {
      if ( neighbor != null ) {
        reduImporter.CreateEccentricPipingReducer( neighbor ) ;
        return true ;
      }

      return false ;
    }

    public void Update( IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType, string[] columns )
    {
      if ( RecordManager.FrontOfCurrentBranch() == null ) {
        return ;
      }

      if ( legType == IDFRecordType.LegType.OutLeg && fittingType == IDFRecordType.FittingType.Olet ) {
        RecordManager.FrontOfCurrentBranch().Update( fittingType, legType, columns ) ;
        RecordManager.TerminateCurrentBranch() ;
        return ;
      }

      if ( RecordManager.LastOfCurrentBranch().FittingType == fittingType ||
           fittingType == IDFRecordType.FittingType.BoreRecord ) {
        if ( ! RecordManager.LastOfCurrentBranch().Update( fittingType, legType, columns ) ) {
          // TeeのFirtLegの末端にTeeが接続している場合、TeeのOutLegが二回連続で呼ばれる事の対応
          UpdateOutLeg( fittingType, legType, columns ) ;
        }
      }
      else if ( legType == IDFRecordType.LegType.OutLeg ) {
        UpdateOutLeg( fittingType, legType, columns ) ;
      }
    }

    private void UpdateOutLeg( IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType,
      string[] columns )
    {
      var parent = RecordManager.CurrentBranchRecords().First().ParentBranch ;
      if ( parent == null ) {
        return ;
      }

      if ( parent.Body.entityState != IDFEntityImporter.EntityState.In &&
           parent.Body.entityState != IDFEntityImporter.EntityState.Out ) {
        if ( parent.Body.FittingType == fittingType ) {
          parent.Body.Update( fittingType, legType, columns ) ;
        }

        RecordManager.TerminateCurrentBranch() ;
      }
    }
  }
}