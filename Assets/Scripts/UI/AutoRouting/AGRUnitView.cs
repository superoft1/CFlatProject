//#define EXPORT_LOG
using System;
using System.Collections.Generic;
using System.Data ;
using System.IO;
using System.Linq;
using IDF;
using Importer.BlockPattern;
using Chiyoda.CAD.BP;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Structure;
using Chiyoda.CAD.Topology;
using Importer.BlockPattern.Equipment.EndTopTypePump.Data8 ;
using Importer.Equipment ;
using VtpAutoRouting ;
using UnityEngine;

public class AGRUnitView : MonoBehaviour
{
  [SerializeField]
  AutoRoutingMgr autoRoutingMgr;

  [SerializeField]
  bool importIDF = false;

  bool isOnDown = false;
  bool isOnCreate = false;

  public static string DemoPath()
  {
    return "" ;
  }

  public static string NoPipesBlockPatternPath()
  {
    return "" ;
  }


  public static string FixedBlockPatternPath()
  {
    return "" ;
  }

  public static string InstrumentsPath()
  {
    return "" ;
  }
  
  private void LateUpdate()
  {
    if ( Input.GetMouseButtonDown( 0 ) || Input.GetMouseButtonDown( 1 ) || Input.GetMouseButtonDown( 2 ) ) {
      isOnDown = true;
    }
    else if ( Input.GetMouseButtonUp( 0 ) || Input.GetMouseButtonUp( 1 ) || Input.GetMouseButtonUp( 2 ) ) {
      if ( isOnDown && !isOnCreate ) {
        Finish();
      }
      isOnDown = false;
    }
  }

  void Finish()
  {
    isOnCreate = false;
    isOnDown = false;
    Hide();
  }

  public void Cancel()
  {
    Hide();
  }

  void Hide()
  {
  }

  public void Show()
  {
  }

  public void ImportPipeRack()
  {
  }

  public void ImportFixedBlockPattern()
  {
  }

  private void ImportShowOpenDialogAndImport(Action<string, Action> action)
  {
  }

  private void ImportShowOpenDialogAndImport(Func<DataSet, string, bool, bool, BlockPattern> func)
  {}

  public void ImportProdraw()
  {}

  public static void ImportProdrawStatic()
  {}

  public static DataSet InstrumentsDataSet()
  {
    return null ;
  }

  public void ImportAll()
  {
    throw new NotImplementedException() ;
  }

  public void ImportPlotData()
  {
  }

  public void OptimizePlot()
  {

  }

  public static void FineTuneSubPipeRack( IEnumerable<Entity> structures )
  {}


  public static void SetupDummyRacks()
  { }

  public static void ImportNoPipesBlockPatterns(DataSet dataSet)
  {
  }


  private void ImportAllPlotPlanBlockPatterns()
  {}

  public static void ImportFixedBlockPatterns(DataSet dataSet)
  {
  }

  public static void ImportAdjustableBlockPatterns(DataSet dataSet)
  {
  }
  
  void PostProcess()
  {
  }
}
