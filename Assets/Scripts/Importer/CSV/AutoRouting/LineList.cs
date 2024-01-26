using System ;
using System.Collections.Generic;
using System.Data;
using System.Linq ;
using System.Text;
using Chiyoda ;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Routing;
using Chiyoda.CAD.Topology;
using Chiyoda.DB ;
using UnityEngine;

namespace Importer.CSV.AutoRouting
{
  internal class RouteCSVBranch
  {
    public string LineId { get ; set ; }
    public string MainLineId { get ; set ; }
    public Vector3d LeafPoint { get ; set ; }
    public double NPS { get ; set ; }
    public Vector3d Direction { get ; set ; }
    
    public bool IsSuctionAtConnection { get ; set ; }
    
    public IEndPoint RegisterEndPoint( Document doc )
      => doc.RegisterEndPoint( LineId, LeafPoint, Direction, NPS ) ;
  }

  internal static class LineList
  {
    private static class ColumnName
    {
      // CSV列名
      public static string SirNo => "Sir.No." ;
      public static string ServiceType => "Service Type" ;
      public static string HeaderBranch => "Header/Branch" ;
      public static string UnitCode => "Unit Code" ;
      public static string FluidCode => "Fluid Code" ;
      public static string Color => "Color" ;
      public static string SequenceLineNumber => "Sequence Line Number" ;
      public static string NominalDiameterInch => "Nominal Diameter(Inch)" ;
      public static string PipingSpecSVCName => "Piping Spec.(SVC Name)" ;
      public static string LineNoOnPID_UHD => "Line No. on P&ID/UHD" ;
      public static string LineNo => "Line No." ;
      public static string RoutingPriority => "Routing Priority(仮)" ;
      public static string LayerNo => "Layer No." ;
      public static string ProcessRequirement_1 => "Process Requirement-1" ;
      public static string ProcessRequirement_2 => "Process Requirement-2" ;
      public static string ProcessRequirement_3 => "Process Requirement-3" ;
      public static string StartID => "Start ID" ;
      public static string StartPointX => "Start PointX" ;
      public static string StartPointY => "Start PointY" ;
      public static string StartPointZ => "Start PointZ" ;
      public static string StartDirection => "Start Dir" ;
      public static string EndID => "End ID" ;
      public static string EndPointX => "End PointX" ;
      public static string EndPointY => "End PointY" ;
      public static string EndPointZ => "End PointZ" ;
      public static string EndDirection => "End Dir" ;
      public static string DesignPressureMpa => "Design Pressure(Mpa)" ;
      public static string DesignTemperaturedegC => "Design Temperature(degC)" ;
      public static string FluidPhaseL_V_2P => "Fluid PhaseL/V/2P" ;
      public static string InsulationType => "Insulation Type" ;
      public static string InsulationThicknessMM => "Insulation Thickness(mm)" ;
      public static string Remark => "Remark" ;
    }
    
    internal static Action<Document, string> GetImportFunc(
      ILineListImportCondition condition, IPositionInterpreter interpreter )
    {
      return ( doc, path ) => ImportData( doc, path, condition, interpreter ) ;
    }

    private static void ImportData( 
      Document doc, string path, 
      ILineListImportCondition condition, IPositionInterpreter interpreter ) 
    {
      var routes = new Dictionary<string, Route>() ;
      var branchList = new Dictionary<string, RouteCSVBranch>() ;

      // CSVからデータテーブルを読み込む
      var table = CSV2DataTable.Load( path, Encoding.UTF8, "LineListCSV" ) ;

      foreach ( DataRow row in table.Rows ) {
        var lineId = GetLineId( row ) ;
        
        if ( condition.IsIgnoringLine( lineId ) ) continue ;

        var serviceType = (string) row[ ColumnName.ServiceType ] ;
        var lineType = (string) row[ ColumnName.HeaderBranch ] ;
        var color = (string) row[ ColumnName.Color ] ;
        var NPSMeter = GetNPSMeter( row ) ;
        var branchLineFrom = (string) row[ ColumnName.StartID ] ;
        var branchLineTo = (string) row[ ColumnName.EndID ] ;

        var startPosition = interpreter.GetPosition( branchLineFrom,
          (string) row[ ColumnName.StartPointX ],
          (string) row[ ColumnName.StartPointY ],
          (string) row[ ColumnName.StartPointZ ], 
          NPSMeter, serviceType ) ;
        var endPosition = interpreter.GetPosition( 
          branchLineTo,
          (string) row[ ColumnName.EndPointX ],
          (string) row[ ColumnName.EndPointY ],
          (string) row[ ColumnName.EndPointZ ],
          NPSMeter, serviceType ) ;

        if ( startPosition == Vector3d.zero || endPosition == Vector3d.zero ) continue ;

        var fluidPhase = (string) row[ ColumnName.FluidPhaseL_V_2P ] ;

        var startDirection = interpreter.GetDirection( (string) row[ ColumnName.StartDirection ] ) ;
        var endDirection = interpreter.GetDirection( (string) row[ ColumnName.EndDirection ] ) ;

        if ( lineType == "Branch" ) {
          var branch = NeedReverse( (string)row[ ColumnName.FluidCode] )
            ? CreateBranch( NPSMeter, lineId, endPosition, startPosition, branchLineTo, branchLineFrom, endDirection )
            : CreateBranch( NPSMeter, lineId, startPosition, endPosition, branchLineFrom, branchLineTo, endDirection ) ;
          
          branchList.Add( lineId, branch ) ;
        }
        else {
          var newRoute = CreateNewRoute( doc, NPSMeter, lineId, 
            startPosition, endPosition, serviceType, color, fluidPhase, startDirection, endDirection ) ;
          routes.Add( lineId, newRoute ) ;
        }
      }

      AddBranchToHeader( routes, branchList ) ;
    }

    private static bool NeedReverse( string fluidCode )
    {
      return fluidCode == "CWR" || fluidCode == "LPCW" ;
    }
    
    private static string GetLineId( DataRow row )
    {
      var lineId = (string) row[ ColumnName.LineNoOnPID_UHD ] ;
      if ( ! string.IsNullOrEmpty( lineId ) ) return lineId ;

      lineId = (string) row[ ColumnName.LineNo ] ;
      if ( ! string.IsNullOrEmpty( lineId ) ) return lineId ;

      Debug.Log( "Please set lineID." );
      return "" ;
    }

    private static double GetNPSMeter( DataRow row )
    {
      double npsInchi ;
      if ( double.TryParse( (string) row[ ColumnName.NominalDiameterInch ], out npsInchi ) ) {
        var NPSMM = DB.Get<NPSTable>().Records.First( rec => Math.Abs( rec.Inchi - npsInchi ) < 1e-3 ).mm ;
        return NPSMM * 0.001;
      }
      
      Debug.Log( "Please set diameter." );
      return 0.1 ;
    }
    
    private static Route CreateNewRoute(
      Document doc, double NPSMeter, string lineId,
      Vector3d startPosition, Vector3d endPosition, 
      string serviceType, string color, string fluidPhase,
      Vector3d startDirection, Vector3d endDirection )
    {
      var route = doc.CreateEntity<Route>() ;
      doc.AddEdge( route ) ;

      route.Name = $"Route({lineId})" ;
      route.LineId = lineId ;
      route.ServiceType = serviceType ;
      route.LineType = serviceType ;
      route.Color = color ;
      route.FluidPhase = fluidPhase ;

      var isValidStartDir = ( startDirection.magnitude > 1e-4 ) ;
      var sDir = isValidStartDir ? startDirection : Vector3.right ;
      var sttPoint = doc.RegisterEndPoint( lineId, startPosition, sDir, NPSMeter ) ;
      
      var isValidEndDir = ( endDirection.magnitude > 1e-4 ) ;
      var eDir = isValidEndDir ? endDirection : Vector3.right ;
      var endPoint = doc.RegisterEndPoint( lineId, endPosition, eDir, NPSMeter ) ;
      route.SetMainRoute( sttPoint, endPoint ) ;
      
      route.IsEndPointDirectionFix = isValidStartDir && isValidEndDir ;
      return route ;
    }
    
    private static void AddBranchToHeader( IDictionary<string, Route> routeList, IDictionary<string, RouteCSVBranch> branchList )
    {
      foreach ( var (_, branch) in branchList ) {
        var mainBranchList = new HashSet<string>() ;     
        var main = SearchHeader( routeList, branchList, branch, mainBranchList ) ;
        if ( main == null ) {
          continue ;
        }
        
        var p = branch.RegisterEndPoint( main.Document ) ;
        main.AddBranch( main.Document.RegisterBranch( p, branch.IsSuctionAtConnection ) );
      }
    }

    private static Route SearchHeader( 
      IDictionary<string, Route> routeList, 
      IDictionary<string, RouteCSVBranch> branchList, 
      RouteCSVBranch branch, 
      ISet<string> mainBranchList )
    {

      if ( routeList.TryGetValue( branch.MainLineId, out var main ) ) {
        return main ;
      }

      if ( !branchList.TryGetValue( branch.MainLineId, out var branchMain ) ) {
        Debug.Log( "error : " + branch.LineId + "," + branch.MainLineId ) ;
        return null ;  
      }
      
      if ( mainBranchList.Contains( branchMain.MainLineId ) ) {
        Debug.Log( "error infinite loop: " + branch.MainLineId ) ;
        return null ;
      }
      mainBranchList.Add( branchMain.MainLineId ) ;
      return SearchHeader( routeList, branchList, branchMain, mainBranchList ) ;
    }

    private static RouteCSVBranch CreateBranch( 
      double NPSMeter, string lineId, Vector3d startPosition, Vector3d endPosition, 
      string branchLineFrom, string branchLineTo, Vector3d dir )
    {
      var branch = new RouteCSVBranch
      {
        NPS = NPSMeter,
        LineId = lineId,
        Direction = dir
      } ;
      if ( ! branchLineFrom.Equals( "Dead End" ) && branchLineFrom != "" ) {
        branch.MainLineId = branchLineFrom ;
        branch.LeafPoint = endPosition ;
        branch.IsSuctionAtConnection = false ;
      }
      else if ( ! branchLineTo.Equals( "Dead End" ) && branchLineTo != "" ) {
        branch.MainLineId = branchLineTo ;
        branch.LeafPoint = startPosition ;
        branch.IsSuctionAtConnection = true ;
      }
      
      return branch ;
    }
  }
}