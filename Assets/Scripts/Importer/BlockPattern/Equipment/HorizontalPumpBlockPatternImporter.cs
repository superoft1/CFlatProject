using System ;
using Chiyoda ;
using Chiyoda.CAD.BP ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using Importer.Equipment ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment
{
  public static class HorizontalPumpImporter
  {
    public static Chiyoda.CAD.Model.Equipment PumpImport( string id, Chiyoda.CAD.Topology.BlockPattern bp,
      Action<Edge> onFinish = null )
    {
      var dataSet = PipingPieceDataSet.GetPipingPieceDataSet() ;
      var equipmentTable = PipingPieceTableFactory.Create( bp.Type, dataSet ) ;
      var (equipment, origin, rot) = equipmentTable.Generate( bp.Document, id, createNozzle: true ) ;
      BlockPatternFactory.CreateInstrumentEdgeVertex( bp, equipment as Chiyoda.CAD.Model.Equipment, origin, rot ) ;

      bp.LocalCod = new LocalCodSys3d(Vector3d.zero, bp.LocalCod);

      foreach (var leafEdge in bp.GetAllLeafEdges())
      {
        AlignAllLeafEdges(bp, leafEdge);
      }

      bp.RuleList.BindChangeEvents(true);
      onFinish?.Invoke(bp) ;
      return equipment as Chiyoda.CAD.Model.Equipment ;
    }
    
    public static void AlignAllLeafEdges(Chiyoda.CAD.Topology.BlockPattern baseBp, LeafEdge basePump)
    {
      var offset = -(Vector3) basePump.LocalCod.Origin ;
      baseBp.GetAllLeafEdges().ForEach( l => l.MoveLocalPos( offset ) ) ;
    }
  }
  
  /// <summary>
  /// Dischargeの径で指定された値がMiniflowの径で指定出来る上限値となるようなルール
  /// </summary>
  public class ChangeMinimumFlowDiameterRangeRule : IUserDefinedRule
  {
    private readonly CompositeBlockPattern _blockPatternArray ;
    private readonly string _propertyName ;
    private readonly string _edgeName ;

    internal ChangeMinimumFlowDiameterRangeRule( CompositeBlockPattern bpArray, string propertyName, string edgeName )
    {
      _blockPatternArray = bpArray ;
      _propertyName = propertyName ;
      _edgeName = edgeName ;
    }

    public void Run(IPropertiedElement owner, IUserDefinedNamedProperty property)
    {
      var elm = owner.GetElementByName( _edgeName ) ;
      if ( ! ( elm is PipingPiece pipingPiece ) ) return ;
      if ( ! ( pipingPiece is Pipe pipe ) ) return ;
      if ( ! ( _blockPatternArray.GetProperty( _propertyName ) is UserDefinedNamedProperty originalProp ) ) return ;
      var diameter = pipe.DiameterObj ;
      var prop = new UserDefinedNamedProperty( _blockPatternArray, originalProp.PropertyName, originalProp.Type, originalProp.Value, originalProp.MinValue, diameter.NpsMm );
      originalProp.ModifyFrom( prop );
    }
  }
}
