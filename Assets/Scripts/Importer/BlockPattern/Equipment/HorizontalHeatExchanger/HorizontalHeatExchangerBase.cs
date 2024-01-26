using System ;
using System.Collections.Generic ;
using System.IO ;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;

namespace Importer.BlockPattern.Equipment.HorizontalHeatExchanger
{
  public class HorizontalHeatExchangerBase
  {
    protected Document Doc { get ; }
    protected Chiyoda.CAD.Topology.BlockPattern BaseBp { get ; }
    protected Chiyoda.CAD.Model.Equipment BaseEquipment { get ; set ; }
    protected string IdfFolderPath { get ; }
    private string EquipmentShapeName { get ; }

    public HorizontalHeatExchangerBase( Document doc, string equipmentShapeName )
    {
      Doc = doc ;
      EquipmentShapeName = equipmentShapeName ;
      BaseBp = BlockPatternFactory.CreateBlockPattern( BlockPatternType.Type.HorizontalHeatExchanger ) ;
      IdfFolderPath = Path.Combine( ImportManager.IDFBlockPatternDirectoryPath(), "HeatExchanger/IDF" ) ;
    }

    public Chiyoda.CAD.Topology.BlockPattern Create( Action<Edge> onFinish )
    {
      using ( Doc.ConfineVerticesMakePairs( false ) ) { // プレビュー時に生成されたvertexがペアリング決定時に悪さをするので必要
        BaseEquipment = ImportEquipment() ;
        PostProcess() ;
      }

      onFinish?.Invoke( BaseBp ) ;

      return BaseBp ;
    }

    /// <summary>
    /// ポンプの読み込み
    /// </summary>
    /// <returns></returns>
    private Chiyoda.CAD.Model.Equipment ImportEquipment()
    {
      return HorizontalHeatExchangerBlockPatternImporter.HorizontalHeatExchangerImport( EquipmentShapeName, BaseBp ) ;
    }

    protected virtual void PostProcess()
    {
      SetBlockPatternInfo() ;
      BaseBp.Document.MaintainEdgePlacement() ;
    }


    protected static Nozzle GetNozzle( Chiyoda.CAD.Topology.BlockPattern block, string nozzleName )
    {
      foreach ( var equipment in block.Equipments ) {
        var ret = equipment.Nozzles.FirstOrDefault( n => n.Name == nozzleName ) ;
        if ( ret != null ) {
          return ret ;
        }
      }

      throw new InvalidOperationException() ;
    }


    protected virtual void SetBlockPatternInfo()
    {
      BaseEquipment.LeafEdge.ObjectName = "Base" ;
      var factory = new PipingLayoutFactory( Doc, IdfFolderPath, BaseEquipment ) ;

      var layoutsUpper = new List<PipingLayout>
      {
        factory.Create( PipingLayoutFactory.UpperLayout.A_1_IN ),
        factory.Create( PipingLayoutFactory.UpperLayout.A_1_T_OUT ),
      } ;

      var layoutsLower = new List<PipingLayout>
      {
        factory.Create( PipingLayoutFactory.LowerLayout.A_1_OUT ),
        factory.Create( PipingLayoutFactory.LowerLayout.A_1_T_IN ),
      } ;
      
      List<(string name, string nozzle, Edge pattern)> upPipes 
        = layoutsUpper.Select( l => ( l.BlockName, l.NozzleName, (Edge)l.GetRuledBlockPattern() ) ).ToList() ;
      upPipes.Add((string.Empty, string.Empty, null));
      
      List<(string name, string nozzle, Edge pattern)> lowPipes 
        = layoutsLower.Select( l => ( l.BlockName, l.NozzleName, (Edge)l.GetRuledBlockPattern() ) ).ToList() ;
      lowPipes.Add((string.Empty, string.Empty, null));

      
      upPipes.Take( 2 ).ForEach( p => BaseBp.AddEdge( p.pattern ) ) ;
      lowPipes.Take( 2 ).ForEach( p => BaseBp.AddEdge( p.pattern ) ) ;

      // 配管の取り替えルール設定（上部）
      for ( int i = 0 ; i < 2 ; ++i ) {
        var prop = BaseBp.RegisterUserDefinedProperty( upPipes[ i ].nozzle, i, new Dictionary<string, double>
        {
          // TODO: 名称は適当なので要修正
          { "Pipe", 0 },
          { "Pipe+Flange", 1 },
          { "Empty", 2 }
        } );
        prop.AddUserDefinedRule( 
          new InterChangeablePipingRule( interChangeableName: PipingLayout.GetInterChangeableName(), GetNozzle( BaseBp, upPipes[ i ].nozzle ), upPipes,
            PipingLayout.GetKeepProperties())) ;
      }

      // 配管の取り替えルール設定（下部）
      for ( int i = 0 ; i < 2 ; ++i ) {
        var prop = BaseBp.RegisterUserDefinedProperty( lowPipes[ i ].nozzle, i, new Dictionary<string, double>
        {
          // TODO: 名称は適当なので要修正
          { "Pipe", 0 },
          { "Pipe+Flange", 1 },
          { "Empty", 2 }
        } );
        prop.AddUserDefinedRule( 
          new InterChangeablePipingRule(  interChangeableName: PipingLayout.GetInterChangeableName(), GetNozzle( BaseBp, lowPipes[ i ].nozzle ), lowPipes, 
            PipingLayout.GetKeepProperties() ) ) ;
      }

      upPipes.Take( 2 ).ForEach( p => p.pattern.RuleList.BindChangeEvents( true ) ) ;
      lowPipes.Take( 2 ).ForEach( p => p.pattern.RuleList.BindChangeEvents( true ) ) ;
      BaseBp.RuleList.BindChangeEvents( true ) ;
    }
  }
}