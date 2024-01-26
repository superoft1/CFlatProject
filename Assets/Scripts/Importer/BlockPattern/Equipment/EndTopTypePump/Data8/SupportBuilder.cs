using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;

namespace Importer.BlockPattern.Equipment.EndTopTypePump.Data8
{
  public class EndTopTypePumpBPSupportBuilder
  {
    private Chiyoda.CAD.Topology.BlockPattern bp { get ; }
    private SingleBlockPatternIndexInfo info { get ; }

    public EndTopTypePumpBPSupportBuilder( Chiyoda.CAD.Topology.BlockPattern bp, SingleBlockPatternIndexInfo info )
    {
      this.bp = bp ;
      this.info = info ;
    }

    public void Build()
    {
      Support mirroringSupport = null ;
      if ( null != info.SupportTypes && 0 < info.SupportTypes.Count ) {
        var supports = bp.Supports.ToArray() ;
        foreach ( var pair in info.SupportTypes ) {
          if ( pair.Key < 0 || supports.Length <= pair.Key ) continue ;
          supports[ pair.Key ].SupportType = pair.Value ;
          if ( pair.Value == SupportType.TType ) {
            // Tタイプの場合、名前をつける
            supports[ pair.Key ].ObjectName = "TTypeSupportRight" ;
            mirroringSupport = supports[ pair.Key ] ;
          }
        }
      }
      // 不要なサポートを削除する
      foreach ( var support in bp.Supports.ToArray() ) {
        if ( SupportType.None == support.SupportType ) bp.Supports.Remove( support ) ;
      }
      // mirroringSupportのコピーを作成
      if ( null != mirroringSupport ) {
        var mirroredSupport = CreateMirroredSupport( mirroringSupport ) ;
        if ( null != mirroredSupport ) {
          ( mirroringSupport.Parent as ISupportParentElement )?.Supports.Add( mirroredSupport ) ;
        }
      }
    }


    Support CreateMirroredSupport( Support mirroringSupport )
    {
      var mirroredSupport = mirroringSupport.Document.CreateEntity<Support>() ;
      var mirroredSupportPosition = CreateMirroredSupportPosition( mirroredSupport, mirroringSupport.SupportPosition as PipeSupportPosition ) ;
      if ( null == mirroredSupportPosition ) return null ;

      mirroredSupport.Enabled = false ;
      mirroredSupport.ObjectName = "TTypeSupportLeft" ;
      mirroredSupport.SupportDirection = mirroringSupport.SupportDirection ;
      mirroredSupport.SupportPosition = mirroredSupportPosition ;
      mirroredSupport.SupportType = mirroringSupport.SupportType ;
      return mirroredSupport ;
    }

    PipeSupportPosition CreateMirroredSupportPosition( Support mirroredSupport, PipeSupportPosition pipeSupportPosition )
    {
      if ( null == pipeSupportPosition ) return null ;

      var myEdge = pipeSupportPosition.Target ;
      var tee = myEdge.GetNextLeafEdges().FirstOrDefault( e => e.PipingPiece is PipingTee ) ;
      if ( null == tee ) return null ;

      var anotherEdge = tee.GetNextLeafEdges()
        .FirstOrDefault( e => e.PipingPiece is Pipe && e.LocalCod.DirectionX.IsParallelTo( myEdge.LocalCod.DirectionX ) ) ;
      if ( null == anotherEdge ) return null ;

      var mirroredPos = new PipeSupportPosition( mirroredSupport ) ;
      mirroredPos.Target = anotherEdge ;
      if ( 0 < Vector3d.Dot( myEdge.LocalCod.DirectionX, anotherEdge.LocalCod.DirectionX ) ) {
        // 同一方向ならパラメータ逆転
        mirroredPos.PositionParameter = -pipeSupportPosition.PositionParameter ;
      }
      else {
        // 逆方向ならパラメータ同一でよい
        mirroredPos.PositionParameter = pipeSupportPosition.PositionParameter ;
      }

      return mirroredPos ;
    }
  }
}
