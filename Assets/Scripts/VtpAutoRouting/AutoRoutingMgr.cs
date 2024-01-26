using System ;
using System.Linq;
using System.Collections.Generic;
using Chiyoda;

using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Routing ;
using Chiyoda.CAD.Model.Structure.CommonEntities ;
using Chiyoda.CAD.Topology;

using Routing.Model;
using Chiyoda.UI ;
using Chiyoda.UI.Dialog ;
using Routing ;
using UnityEngine;
using VtpAutoRouting.BridgeEntities ;
using Debug = UnityEngine.Debug ;

namespace VtpAutoRouting
{
  public class AutoRoutingMgr : MonoBehaviour
  {
    // モーダルダイアログ
    [SerializeField] ModalDialog ModalDialog ;

    // 座標入力ダイアログ
    [SerializeField] CodInputModalDialog CodInputDialog ;

    // メインカメラ
    [SerializeField] CameraOperator MainCamera ;
    
    // 通過候補点やエルボ頂点などを表示するか？のフラグ
    [SerializeField] private bool addCandidatePoints = true ; // trueにすると通過候補点が生成される

    [SerializeField] private bool addThroughPoints = false ; // trueにすると通過点が生成される
 
    #region Singleton

    public static AutoRoutingMgr Instance()
    {
      return _instance ;
    }

    static AutoRoutingMgr _instance = null ;

    #endregion

    #region Unity Triggered

    private void Awake()
    {
      if ( _instance == null ) {
        _instance = this ;
      }
    }

    // Start is called before the first frame update
    void Start()
    {      
      CodInputDialog.OKClickedHandler += ( s, e ) => {
        if (MainCamera != null && CodInputDialog.GetCod(out Vector3 vec)) {
          // Boundaryのサイズは適当
          MainCamera.SetBoundary( new Bounds( vec, new Vector3(1,1,1)) );
        }
      } ;
      //Execute() ;
    }

    // Update is called once per frame
    void Update()
    {
      if ( (Input.GetKey( KeyCode.LeftShift) || Input.GetKey( KeyCode.RightShift)) && 
           (Input.GetKey( KeyCode.LeftControl) || Input.GetKey( KeyCode.RightControl)) && 
           (Input.GetKey( KeyCode.J))) {
        CodInputDialog.Show("Input Coordinate", "Let's go to...");
      }
    }

    #endregion

    #region Import

    internal void ImportAll( )
    {
    }

    #endregion

    #region CreateRoute

    internal Route CreateRoute( Document curDoc, Dictionary<HalfVertex, HalfVertex.FlowType> vertEntityTypeMap )
    {
      // ルート生成
      var route = curDoc.CreateEntity<Route>() ;
      curDoc.AddEdge( route ) ;
      route.Name = "Route" ;
      route.LineId = "LineID" ;
      route.ServiceType = "" ;
      route.LineType = "P" ;
      route.Color = "Green" ;
      route.FluidPhase = "L" ;
      route.IsEndPointDirectionFix = true ;
      
      var starts = new List<IEndPoint>() ;
      var ends = new List<IEndPoint>() ;
      foreach ( var (vertex, type) in vertEntityTypeMap ) {
        switch ( type )
        {
          case HalfVertex.FlowType.FromThisToAnother:
            starts.Add( curDoc.RegisterEndPoint( vertex ) ) ;
            break;
          case HalfVertex.FlowType.FromAnotherToThis:
            ends.Add( curDoc.RegisterEndPoint( vertex ) ) ;
            break;
        }
      }
      
      route.SetMainRoute( starts.First(), ends.First() );
      starts.Skip( 1 ).Select( p => curDoc.RegisterBranch( p, true ) )
        .ForEach( b => route.AddBranch( b ) );
      ends.Skip( 1 ).Select( p => curDoc.RegisterBranch( p, false ) )
        .ForEach( b => route.AddBranch( b ) );
      
      StaticParameters.SetDiameterProvider( new PipePropertyProvider() );
      var dir = new DirectoryUtility( Application.dataPath, Application.streamingAssetsPath );
      API.CreateRoute( 
        AutoRoutingModelFactory.GetCurrentAllRacks(), new [] { route },
        ModelConverter.ToDebugFilePath( dir ) ) ;
      return route;
    }

    #endregion

    internal void Execute()
    {
      var dirUtil = new DirectoryUtility( Application.dataPath, Application.streamingAssetsPath ) ;
      Execute( DocumentCollection.Instance.Current.Routes, dirUtil ) ;      
    }

    internal void Execute( IEnumerable<Route> routes )
    {
      Execute( routes, new DirectoryUtility( Application.dataPath, Application.streamingAssetsPath ) ) ;
    }

    private void Execute( IEnumerable<Route> targetRoutes, DirectoryUtility dir )
    {
      if ( StaticParameters.DiameterProvider == null ) {
        StaticParameters.SetDiameterProvider( new PipePropertyProvider() );
      }
      
      try {
        var (success, failure, calcTime) = API.CreateRoute( 
          AutoRoutingModelFactory.GetCurrentAllRacks(), targetRoutes,ModelConverter.ToDebugFilePath( dir ) ) ;
        targetRoutes.Select( r => r.Document ).Distinct()
          .ForEach( d => d.Streams.RefreshStreamEdges() );
        
        var resultMsg = $"Routing process finished!" + Environment.NewLine ;
        resultMsg += $"Success:{success}" + Environment.NewLine ;
        resultMsg += $"Failure:{failure}" + Environment.NewLine ;
        resultMsg += $"{( calcTime.TotalMilliseconds / 1000.0 ):F1} sec" + Environment.NewLine ;
        ModalDialog.Show( "Auto Routing", resultMsg ) ;
      }
      catch ( Exception exp ) {
        Debug.LogError( exp.Message ) ;
      }
    }
    
    #region Delete

    internal void DeleteResult()
    {
      DeleteResult( DocumentCollection.Instance.Current.Routes ) ;
    }

    internal void DeleteResult( IEnumerable<Route> targetRoutes )
    {
      targetRoutes.ForEach( route => route.DeleteResult() ) ;
    }

    internal void DeleteAll( )
    {
      var current = DocumentCollection.Instance.Current ;
      if ( current == null ) return ;
      // chiyoDev要素の削除
      foreach ( var edge in current.EdgeList.ToList() ) {
        current.RemoveEdge( edge ) ;
      }

      current.Structures.Clear() ;
      current.Structures.ForEach( str => current.Structures.Clear() ) ;
      foreach ( var root in Chiyoda.UI.DocumentTreeView.Instance().TreeView.Items ) {
        foreach ( var item in root.Items ) {
          switch ( item.GetTreeViewItemSource() ) {
            case PlacementEntity str :
              current.Structures.Remove( str ) ;
              break ;
            case Route route :
              current.RemoveEdge( route ) ;
              break ;
            case Edge edge :
              current.RemoveEdge( edge ) ;
              break ;
            default :
              break ;
          }
        }
      }
    }

    #endregion

    #region ProDraw

    public void UpdateEndPoints( IEnumerable<Route> routes )
    {
      routes.ForEach( r => r.SyncEndPointsToLinkPoint() );
    }

    // ProFileに自動ルーティング結果を保存
    internal void SaveProFile()
    {
    }

    #endregion

    #region Debug Test

    #endregion
  }
}